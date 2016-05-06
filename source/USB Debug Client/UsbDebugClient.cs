//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Microsoft.NetMicroFramework.Tools.UsbDebug.Usb;
using Microsoft.SPOT.Debugger;
using Microsoft.SPOT.Debugger.WireProtocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace Microsoft.NetMicroFramework.Tools.UsbDebug
{
    public class UsbDebugClient : Port<MFUsbDevice>, IPort<MFUsbDevice>, IControllerHostLocal<MFUsbDevice>
    {
        // dictionary with mapping between USB device watcher and the device ID
        private Dictionary<DeviceWatcher, string> mapDeviceWatchersToDeviceSelector;

        // USB device watchers suspended flag
        private bool watchersSuspended = false;

        // USB device watchers started flag
        private bool watchersStarted = false;

        // counter of device watchers completed
        private int deviceWatchersCompletedCount = 0;
        private bool isAllDevicesEnumerated = false;

        private object cancelIoLock = new object();
        private static SemaphoreSlim semaphore;

        /// <summary>
        /// ObservableCollection of all USB devices that are enumerated.
        /// </summary>
        public override ObservableCollection<MFUsbDevice> MFDevices { get; protected set; }

        //public DateTime LastActivity { get; protected set; }

        /// <summary>
        /// Creates an USB debug client
        /// </summary>
        public UsbDebugClient(Application callerApp)
        {
            mapDeviceWatchersToDeviceSelector = new Dictionary<DeviceWatcher, String>();
            MFDevices = new ObservableCollection<MFUsbDevice>();

            // set caller app property
            EventHandlerForUsbDevice.CallerApp = callerApp;

            // start USB watchers in a another task
            Task.Factory.StartNew( () =>
            {
                StartUsbDeviceWatchers();
            });

            semaphore = new SemaphoreSlim(1, 1);
        }

        #region Device watchers initialization

        /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        Add a device watcher initialization method for each supported device that should be watched.
        That initialization method must be called from the InitializeDeviceWatchers() method above so the watcher is actually started.
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

        /// <summary>
        /// Initialize the device watcher for the ST Discovery4
        /// </summary>
        private void InitializeStDiscovery4DeviceWatcher()
        {
            // better use  most specific type of DeviceSelector: VID, PID and class GUID
            var stDiscovery4Selector = UsbDevice.GetDeviceSelector(StDiscovery4.DeviceVid, StDiscovery4.DevicePid, StDiscovery4.DeviceInterfaceClass);

            // Create a device watcher to look for instances of this device
            var stDiscovery4Watcher = DeviceInformation.CreateWatcher(stDiscovery4Selector);

            // Allow the EventHandlerForDevice to handle device watcher events that relates or effects this device (i.e. device removal, addition, app suspension/resume)
            AddDeviceWatcher(stDiscovery4Watcher, stDiscovery4Selector);
        }

        /// <summary>
        /// Registers for Added, Removed, and Enumerated events on the provided deviceWatcher before adding it to an internal list.
        /// </summary>
        /// <param name="deviceWatcher">The device watcher to subscribe the events</param>
        /// <param name="deviceSelector">The AQS used to create the device watcher</param>
        private void AddDeviceWatcher(DeviceWatcher deviceWatcher, String deviceSelector)
        {
            deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(OnDeviceAdded);
            deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(OnDeviceRemoved);
            deviceWatcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, Object>(OnDeviceEnumerationComplete);

            mapDeviceWatchersToDeviceSelector.Add(deviceWatcher, deviceSelector);
        }

        #endregion

        #region Device watcher management and host app status handling

        /// <summary>
        /// Initialize device watchers. Must call here the initialization methods for all devices that we want to set watch.
        /// </summary>
        private void InitializeDeviceWatchers()
        {
            // ST Discovery 4
            InitializeStDiscovery4DeviceWatcher();
        }

        public void StartUsbDeviceWatchers()
        {
            // Initialize the USB device watchers to be notified when devices are connected/removed
            InitializeDeviceWatchers();
            StartDeviceWatchers();
        }

        /// <summary>
        /// Starts all device watchers including ones that have been individually stopped.
        /// </summary>
        private void StartDeviceWatchers()
        {
            // Start all device watchers
            watchersStarted = true;
            deviceWatchersCompletedCount = 0;
            isAllDevicesEnumerated = false;

            foreach (DeviceWatcher deviceWatcher in mapDeviceWatchersToDeviceSelector.Keys)
            {
                if ((deviceWatcher.Status != DeviceWatcherStatus.Started)
                    && (deviceWatcher.Status != DeviceWatcherStatus.EnumerationCompleted))
                {
                    deviceWatcher.Start();
                }
            }
        }

        /// <summary>
        /// Should be called on host app OnAppSuspension() event to properly handle that status.
        /// The DeviceWatchers must be stopped because device watchers will continue to raise events even if
        /// the app is in suspension, which is not desired (drains battery). The device watchers will be resumed once the app resumes too.
        /// </summary>
        public void AppSuspending()
        {
            if (watchersStarted)
            {
                watchersSuspended = true;
                StopDeviceWatchers();
            }
            else
            {
                watchersSuspended = false;
            }
        }

        /// <summary>
        /// Should be called on host app OnAppResume() event to properly handle that status.
        /// See AppSuspending for why we are starting the device watchers again.
        /// </summary>
        public void AppResumed()
        {
            if (watchersSuspended)
            {
                watchersSuspended = false;
                StartDeviceWatchers();
            }
        }

        /// <summary>
        /// Stops all device watchers.
        /// </summary>
        private void StopDeviceWatchers()
        {
            // Stop all device watchers
            foreach (DeviceWatcher deviceWatcher in mapDeviceWatchersToDeviceSelector.Keys)
            {
                if ((deviceWatcher.Status == DeviceWatcherStatus.Started)
                    || (deviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted))
                {
                    deviceWatcher.Stop();
                }
            }

            // Clear the list of devices so we don't have potentially disconnected devices around
            ClearDeviceEntries();

            watchersStarted = false;
        }

        #endregion

        #region Methods to manage device list add, remove, etc

        /// <summary>
        /// Creates a DeviceListEntry for a device and adds it to the list of devices
        /// </summary>
        /// <param name="deviceInformation">DeviceInformation on the device to be added to the list</param>
        /// <param name="deviceSelector">The AQS used to find this device</param>
        private async void AddDeviceToList(DeviceInformation deviceInformation, String deviceSelector)
        {
            // search the device list for a device with a matching interface ID
            var match = FindDevice(deviceInformation.Id);

            // Add the device if it's new
            if (match == null)
            {
                //     Create a new element for this device interface, and queue up the query of its
                //     device information
                match = new MFUsbDevice(new UsbDeviceListEntry(deviceInformation, deviceSelector), this);

                Debug.WriteLine("Add new USB device to list: " + match.Description + " @ " + deviceSelector);

                // Add the new element to the end of the list of devices
                MFDevices.Add(match);

                // now fill in the description
                // try opening the device to read the descriptor
                if (await match.ConnectAsync().ConfigureAwait(false))
                {
                    // the device description format is kept to maintain backwards compatibility
                    match.Description = EventHandlerForUsbDevice.Current.DeviceInformation.Name + "_" + await GetDeviceDescriptor(5).ConfigureAwait(false);

                    // done here, close device
                    EventHandlerForUsbDevice.Current.CloseDevice();
                }
            }
        }

        private void RemoveDeviceFromList(string deviceId)
        {
            // Removes the device entry from the internal list; therefore the UI
            var deviceEntry = FindDevice(deviceId);

            MFDevices.Remove(deviceEntry);
        }

        private void ClearDeviceEntries()
        {
            MFDevices.Clear();
        }

        /// <summary>
        /// Searches through the existing list of devices for the first DeviceListEntry that has
        /// the specified device Id.
        /// </summary>
        /// <param name="deviceId">Id of the device that is being searched for</param>
        /// <returns>DeviceListEntry that has the provided Id; else a nullptr</returns>
        private MFUsbDevice FindDevice(String deviceId)
        {
            if (deviceId != null)
            {
                foreach (MFUsbDevice entry in MFDevices)
                {
                    if (entry.UsbDevice.DeviceInformation.Id == deviceId)
                    {
                        return entry;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Remove the device from the device list 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInformationUpdate"></param>
        private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
        {
            RemoveDeviceFromList(deviceInformationUpdate.Id);
        }

        /// <summary>
        /// This function will add the device to the listOfDevices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInformation"></param>
        private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation deviceInformation)
        {
            AddDeviceToList(deviceInformation, mapDeviceWatchersToDeviceSelector[sender]);
        }

        #endregion

        #region Handlers and events for Device Enumeration Complete 

        private void OnDeviceEnumerationComplete(DeviceWatcher sender, Object args)
        {
            // add another device watcher completed
            deviceWatchersCompletedCount++;

            if (deviceWatchersCompletedCount == mapDeviceWatchersToDeviceSelector.Count)
            {
                Debug.WriteLine("USB device enumeration completed. Found {0} devices", MFDevices.Count);

                // all watchers have completed enumeration
                isAllDevicesEnumerated = true;

                // fire event that USB enumeration is complete 
                OnDeviceEnumerationCompleted();
            }
        }

        private async Task<string> GetDeviceDescriptor(int index)
        {
            try
            {
                // maximum expected length of descriptor
                uint readBufferSize = 64;

                // prepare buffer to hold the descriptor data returned from the device
                var buffer = new Windows.Storage.Streams.Buffer(readBufferSize);

                // setup packet to perform the request
                UsbSetupPacket setupPacket = new UsbSetupPacket
                {
                    RequestType = new UsbControlRequestType
                    {
                        Direction = UsbTransferDirection.In,
                        Recipient = UsbControlRecipient.SpecifiedInterface,
                        ControlTransferType = UsbControlTransferType.Vendor,
                    },
                    // request to get a descriptor
                    Request = (byte)UsbDeviceRequestType.GetDescriptor,

                    // descriptor number to be read
                    Value = (uint)index,

                    // max length of response
                    Length = readBufferSize
                };

                // send control to device
                IBuffer responseBuffer = await EventHandlerForUsbDevice.Current.Device.SendControlInTransferAsync(setupPacket, buffer);

                // read from a buffer with a data reader
                DataReader reader = DataReader.FromBuffer(responseBuffer);

                // USB data is Little Endian 
                reader.ByteOrder = ByteOrder.LittleEndian;

                // set encoding to UTF16 & Little Endian
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;

                // read 1st byte (descriptor length)
                int descriptorLenght = reader.ReadByte();
                // read 2nd byte (descriptor type)
                int descryptorType = reader.ReadByte();

                // check if this a string (string descriptor type is 0x03)
                if (descryptorType == 0x03)
                {
                    // read a string with remaining bytes available
                    // the string length is half the available bytes because it's UTF16 encoded (2 bytes for each char)
                    return reader.ReadString(reader.UnconsumedBufferLength / 2);
                }
            }
            catch (Exception)
            {

            }

            // if we get here something went wrong above, so we don't have a descriptor
            return string.Empty;
        }

        protected virtual void OnDeviceEnumerationCompleted()
        {
            EventHandler handler = DeviceEnumerationCompleted;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Event that is raised when enumeration of all watched devices is complete.
        /// </summary>
        public event EventHandler DeviceEnumerationCompleted;

        #endregion

        public async Task<bool> ConnectDeviceAsync(MFUsbDevice device)
        {
            // Create an EventHandlerForDevice to watch for the device we are connecting to
            EventHandlerForUsbDevice.CreateNewEventHandlerForDevice();

            // Get notified when the device was successfully connected to or about to be closed
            EventHandlerForUsbDevice.Current.OnDeviceConnected = this.OnDeviceConnected;
            EventHandlerForUsbDevice.Current.OnDeviceClose = this.OnDeviceClosing;

            Debug.WriteLine("Trying to open USB device " + device.Description + " @ " + device.UsbDevice.InstanceId);

            // It is important that the FromIdAsync call is made on the UI thread because the consent prompt can only be displayed
            // on the UI thread. Since this method is invoked by the UI, we are already in the UI thread.
            bool openResult = await EventHandlerForUsbDevice.Current.OpenDeviceAsync(device.UsbDevice.DeviceInformation, device.UsbDevice.DeviceSelector).ConfigureAwait(false);

            Debug.WriteLineIf(openResult, "Device open successfully.");
            Debug.WriteLineIf(!openResult, "Failed to open device.");

            return openResult;
        }

        /// <summary>
        /// If all the devices have been enumerated, select the device in the list we connected to. Otherwise let the EnumerationComplete event
        /// from the device watcher handle the device selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInformation"></param>
        private void OnDeviceConnected(EventHandlerForUsbDevice sender, DeviceInformation deviceInformation)
        {
            // Find and select our connected device
            if (isAllDevicesEnumerated)
            {
                //SelectDeviceInList(EventHandlerForUSBDevice.Current.DeviceInformation.Id);

                //ButtonDisconnectFromDevice.Content = ButtonNameDisconnectFromDevice;
            }

            //rootPage.NotifyUser("Currently connected to: " + EventHandlerForUSBDevice.Current.DeviceInformation.Id, NotifyType.StatusMessage);
        }

        /// <summary>
        /// The device was closed. If we will auto-reconnect to the device, reflect that in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInformation"></param>
        private async void OnDeviceClosing(EventHandlerForUsbDevice sender, DeviceInformation deviceInformation)
        {
            //await rootPage.Dispatcher.RunAsync(
            //    CoreDispatcherPriority.Normal,
            //    new DispatchedHandler(() =>
            //    {
            //        // We were connected to the device that was unplugged, so change the "Disconnect from device" button
            //        // to "Do not reconnect to device"
            //        if (ButtonDisconnectFromDevice.IsEnabled && EventHandlerForDevice.Current.IsEnabledAutoReconnect)
            //        {
            //            ButtonDisconnectFromDevice.Content = ButtonNameDisableReconnectToDevice;
            //        }
            //    }));
        }


        public async Task StartSessionAsync(MFUsbDevice device)
        {
            //// check if this device already has a session
            //if(device.DebugSession != null)
            //{
            //    throw new DebugSessionAlreadyOpenException();
            //}

            //// connect to NETMF device
            //if(await ConnectDeviceAsync(device).ConfigureAwait(false))
            //{

            //}

            // couldn't connect to device so there is no debug session
            throw new CouldntOpenNetMFDeviceException();
        }

        #region Interface implementations

        public bool IsConnected
        {
            get
            {
                return EventHandlerForUsbDevice.Current.IsDeviceConnected;
            }
        }

        public DateTime LastActivity { get; set; }

        public void DisconnectDevice(MFUsbDevice device)
        {
            EventHandlerForUsbDevice.Current.CloseDevice();
        }

        public void StopSession(MFUsbDevice device)
        {
            MFUsbDevice thisDevice = device as MFUsbDevice;

            // close the device
            DisconnectDevice(thisDevice);
        }


        public async Task<uint> SendBufferAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            uint bytesWritten = 0;

            // device must be connected
            if (EventHandlerForUsbDevice.Current.IsDeviceConnected)
            {
                //sanity check for available OUT pipes (must have at least one!)
                if (EventHandlerForUsbDevice.Current.Device.DefaultInterface.BulkOutPipes.Count < 1)
                {
                    // FIXME
                    // throw exception?
                }

                // gets the 1st OUT stream for the device
                var stream = EventHandlerForUsbDevice.Current.Device.DefaultInterface.BulkOutPipes[(int)0].OutputStream;

                // create a data writer to access the device OUT stream
                var writer = new DataWriter(stream);

                // write buffer
                writer.WriteBytes(buffer);

                // need to have a timeout to cancel the read task otherwise it may end up waiting forever for this to return
                var timeoutCancelatioToken = new CancellationTokenSource(2000).Token;

                // because we have an external cancellation token and the above timeout cancellation token, need to combine both
                var linkedCancelationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancelatioToken).Token;

                Task<uint> storeAsyncTask;

                try
                {

                    // Don't start any IO if the task has been canceled
                    lock (cancelIoLock)
                    {
                        // set this makes sure that an exception is thrown when the cancellation token is set
                        linkedCancelationToken.ThrowIfCancellationRequested();

                        // Now the buffer data is actually flushed out to the device.
                        // We should implement a cancellation Token here so we are able to stop the task operation explicitly if needed
                        // The completion function should still be called so that we can properly handle a canceled task
                        storeAsyncTask = writer.StoreAsync().AsTask(linkedCancelationToken);
                    }

                    bytesWritten = await storeAsyncTask;

                    if (bytesWritten > 0)
                    {
                        LastActivity = DateTime.Now;
                    }
                }
                catch (TaskCanceledException)
                {
                    // this is expected to happen, don't do anything with this 
                }
            }
            else
            {
                // FIXME 
                // NotifyDeviceNotConnected
            }

            return bytesWritten;
        }

        public async Task<DataReader> ReadBufferAsync(uint bytesToRead, TimeSpan waiTimeout, CancellationToken cancellationToken)
        {
            // device must be connected
            if (EventHandlerForUsbDevice.Current.IsDeviceConnected)
            {
                //sanity check for available IN pipes (must have at least one!)
                if (EventHandlerForUsbDevice.Current.Device.DefaultInterface.BulkInPipes.Count < 1)
                {
                    // FIXME
                    // throw exception?
                }

                // gets the 1st IN stream for the device
                var stream = EventHandlerForUsbDevice.Current.Device.DefaultInterface.BulkInPipes[0].InputStream;

                DataReader reader = new DataReader(stream);
                uint bytesRead = 0;

                // need to have a timeout to cancel the read task otherwise it may end up waiting forever for this to return
                var timeoutCancelatioToken = new CancellationTokenSource(waiTimeout).Token;

                // because we have an external cancellation token and the above timeout cancellation token, need to combine both
                var linkedCancelationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancelatioToken).Token;

                Task<UInt32> loadAsyncTask;

                //Debug.WriteLine("### waiting");
                await semaphore.WaitAsync();
                //Debug.WriteLine("### got it");

                try
                {
                    // Don't start any IO if the task has been canceled
                    lock (cancelIoLock)
                    {
                        // set this makes sure that an exception is thrown when the cancellation token is set
                        linkedCancelationToken.ThrowIfCancellationRequested();

                        // We should implement a cancellation Token here so we are able to stop the task operation explicitly if needed
                        // The completion function should still be called so that we can properly handle a canceled task
                        loadAsyncTask = reader.LoadAsync(bytesToRead).AsTask(linkedCancelationToken);
                    }

                    bytesRead = await loadAsyncTask;
                }
                catch (TaskCanceledException)
                {
                    // this is expected to happen, don't do anything with this 
                }
                finally
                {
                    semaphore.Release();
                    //Debug.WriteLine("### released");
                }

                return reader;
            }
            else
            {
                // FIXME 
                // NotifyDeviceNotConnected
            }

            return null;
        }

        #endregion

        public Task<bool> QueueOutputAsync(MessageRaw raw)
        {
            throw new NotImplementedException();
        }

        #region Deprecated stuff


        public Packet NewPacket()
        {
            throw new NotImplementedException();
        }

        public void StopProcessing()
        {
            throw new NotImplementedException();
        }

        public void ResumeProcessing()
        {
            throw new NotImplementedException();
        }

        public uint GetUniqueEndpointId()
        {
            throw new NotImplementedException();
        }

        public bool QueueOutput(MessageRaw raw)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Engine methods and properties

        //private IncomingMessage SyncMessage(uint cmd, uint flags, object payload, int retries, int timeout)
        //{
        //    //Lock on m_ReqSyncLock object, so only one thread is active inside the block.
        //    //lock (m_ReqSyncLock)
        //    //{
        //        Request req = AsyncMessage(cmd, flags, payload, retries, timeout);

        //        return req.Wait();
        //    //}
        //    return null;
        //}

        //private Request AsyncMessage(uint cmd, uint flags, object payload, int retries, int timeout)
        //{
        //    OutgoingMessage msg = CreateMessage(cmd, flags, payload);

        //    return AsyncRequest(msg, retries, timeout);
        //}

        //private OutgoingMessage CreateMessage(uint cmd, uint flags, object payload)
        //{
        //    //return new OutgoingMessage(this, CreateConverter(), cmd, flags, payload);
        //    return null;
        //}

        //internal Request AsyncRequest(OutgoingMessage msg, int retries, int timeout)
        //{
        //    Request req = new Request(new SPOT.Debuger.Engine(), msg, retries, timeout, null);

        //    //lock (m_state.SyncObject)
        //    //{

        //    //    //Checking whether IsRunning and adding the request to m_requests
        //    //    //needs to be atomic to avoid adding a request after the Engine
        //    //    //has been stopped.

        //    //    if (!IsRunning)
        //    //    {
        //    //        throw new ApplicationException("Engine is not running or process has exited.");
        //    //    }

        //    //    m_requests.Add(req);

        //    //    req.SendAsync();
        //    //}

        //    return req;
        //}

        #endregion


        #region Controller methods and properties

        public bool ProcessMessage(IncomingMessage msg, bool fReply)
        {
            throw new NotImplementedException();
        }

        public void SpuriousCharacters(byte[] buf, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void ProcessExited()
        {
            throw new NotImplementedException();
        }

        public Task StartSessionAsync(MFDevice device)
        {
            throw new NotImplementedException();
        }

        public void StopSession(MFDevice device)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
