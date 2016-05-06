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

using Microsoft.NetMicroFramework.Tools;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Microsoft.SPOT.Debugger
{
    public abstract class Port<T> /*: IPort<T>*/ where T : MFDevice
    {
        //protected PortBase(string displayName, string port)
        //{
        //    this.DisplayName = displayName;
        //    this.Port = port;
        //}

        public override bool Equals(object obj)
        {
            Port<T> pd = obj as Port<T>; if (pd == null) return false;

            return (pd.UniqueId.Equals(UniqueId));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Task<uint> SendBufferAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<DataReader> ReadBufferAsync(uint bytesToRead, TimeSpan waiTimeout, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConnectDeviceAsync(T device)
        {
            throw new NotImplementedException();
        }

        public void DisconnectDevice(T device)
        {
            throw new NotImplementedException();
        }

        //static public PortDefinition CreateInstanceForSerial(string displayName, string port, uint baudRate)
        //{
        //    return new PortDefinition_Serial(displayName, port, baudRate);
        //}

        //static public PortDefinition CreateInstanceForUsb(string displayName, string port)
        //{
        //    return new PortDefinition_Usb(displayName, port, new ListDictionary());
        //}

        //static public PortDefinition CreateInstanceForWinUsb(string displayName, string port)
        //{
        //    return new PortDefinition_WinUsb(displayName, port, new ListDictionary());
        //}

        //static public PortDefinition CreateInstanceForEmulator(string displayName, string port, int pid)
        //{
        //    return new PortDefinition_Emulator(displayName, port, pid);
        //}

        //static public PortDefinition CreateInstanceForTcp(IPEndPoint ipEndPoint)
        //{
        //    return new PortDefinition_Tcp(ipEndPoint);
        //}

        //static public PortDefinition CreateInstanceForTcp(string name)
        //{
        //    PortDefinition portDefinition = null;

        //    //From CorDebug\DebugPort.cs
        //    string hostName = name;
        //    int port = PortDefinition_Tcp.WellKnownPort;
        //    int portIndex = hostName.IndexOf(':');
        //    IPAddress address = null;

        //    if (portIndex > 0)
        //    {
        //        hostName = name.Substring(0, portIndex);

        //        if (portIndex < name.Length - 1)
        //        {
        //            string portString = name.Substring(portIndex + 1);

        //            int portT;

        //            if (int.TryParse(portString, out portT))
        //            {
        //                port = portT;
        //            }
        //        }
        //    }

        //    if (!IPAddress.TryParse(hostName, out address))
        //    {
        //        //Does DNS resolution make sense here?

        //        IPHostEntry iPHostEntry = Dns.GetHostEntry(hostName);

        //        if (iPHostEntry.AddressList.Length > 0)
        //        {
        //            //choose the first one?
        //            address = iPHostEntry.AddressList[0];
        //        }
        //    }

        //    if (address != null)
        //    {
        //        IPEndPoint ipEndPoint = new IPEndPoint(address, port);

        //        portDefinition = new PortDefinition_Tcp(ipEndPoint);

        //        //ping to see if it is alive?
        //    }

        //    return portDefinition;
        //}

        //static public ArrayList Enumerate(params PortFilter[] args)
        //{
        //    ArrayList lst = new ArrayList();

        //    foreach (PortFilter pf in args)
        //    {
        //        PortDefinition[] res;

        //        switch (pf)
        //        {
        //            case PortFilter.Emulator:
        //                res = Emulator.EnumeratePipes();
        //                break;

        //            case PortFilter.Serial:
        //                res = AsyncSerialStream.EnumeratePorts();
        //                break;

        //            case PortFilter.LegacyPermiscuousWinUsb:
        //            case PortFilter.Usb:
        //                {
        //                    res = WinUsb_AsyncUsbStream.EnumeratePorts(pf == PortFilter.LegacyPermiscuousWinUsb);

        //                    lst.AddRange(res);

        //                    res = AsyncUsbStream.EnumeratePorts();
        //                    // res will be added to list below...
        //                }
        //                break;

        //            case PortFilter.TcpIp:
        //                res = PortDefinition_Tcp.EnumeratePorts();
        //                break;
        //            default: res = null; break;
        //        }

        //        if (res != null)
        //        {
        //            lst.AddRange(res);
        //        }
        //    }

        //    return lst;
        //}

        public string DisplayName { get; internal set; }

        public string PortName { get; internal set; }

        public virtual object UniqueId
        {
            get
            {
                return PortName;
            }
        }

        public string PersistName
        {
            get
            {
                return UniqueId.ToString();
            }
        }


        public abstract ObservableCollection<T> MFDevices { get; protected set; }

        //public Task<uint> SendBufferAsync(byte[] buffer, CancellationToken cancellationToken);

        //public Task<DataReader> ReadBufferAsync(uint bytesToRead, TimeSpan waiTimeout, CancellationToken cancellationToken);
        //public Task<bool> ConnectDeviceAsync(T device);
        //public void DisconnectDevice(T device);

        //public abstract Task<bool> ConnectDeviceAsync(T device);

        //public abstract void DisconnectDevice(T device);

    }

}
