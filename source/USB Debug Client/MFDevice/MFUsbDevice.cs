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

using Microsoft.NetMicroFramework.Tools.UsbDebug;
using Microsoft.SPOT.Debugger;
using Microsoft.SPOT.Debugger.WireProtocol;
using System;
using System.Threading.Tasks;

namespace Microsoft.NetMicroFramework.Tools
{
    public class MFUsbDevice : MFDevice //, IMFDevice<MFUsbDevice>
    {
        /// <summary>
        /// .NETMF debug engine
        /// </summary>
        public Engine<MFDevice> DebugEngine { get; protected set; }

        public UsbDeviceListEntry UsbDevice { get; set; }

//        public DebugSession DebugSession { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public UsbDebugClient Parent
        {
            get
            {
                //return (UsbDebugClient)ProtectedParent;
                throw new NotImplementedException();
            }
        }

        public MFUsbDevice(UsbDeviceListEntry usbDevice, UsbDebugClient owner):base(owner)
        {
            Transport = TransportType.Usb;
            UsbDevice = usbDevice;
        }

        public bool Disconnect()
        {
            return true;
        }

        /// <summary>
        /// Connect to NETMF device
        /// </summary>
        /// <returns></returns>
        public Task<bool> ConnectAsync()
        {
            return Parent.ConnectDeviceAsync(this);
        }

        /// <summary>
        /// Start debug session 
        /// </summary>
        /// <returns></returns>
        public Task StartSessionAsync()
        {
            return Parent.StartSessionAsync(this);
        }

        public void StopSession()
        {
            Parent.StopSession(this);
        }

        protected void CreateDebugEngine()
        {
           DebugEngine = new Engine<MFDevice>(Parent as IPort<MFDevice>, this);;
        }
    }
}
