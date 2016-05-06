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

using Microsoft.SPOT.Debugger;
using Microsoft.SPOT.Debugger.WireProtocol;
using System;
using System.Threading.Tasks;

namespace Microsoft.NetMicroFramework.Tools
{
    public class MFDevice : IDisposable, IMFDevice
    {
        /// <summary>
        /// .NETMF debug engine
        /// </summary>
        public Engine<MFDevice> DebugEngine { get; protected set; }

        /// <summary>
        /// Transport to the device. 
        /// </summary>
        public TransportType Transport { get; protected set; }

        /// <summary>
        /// Device description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This property is a placeholder for the parent of the derived MFDevice.
        /// </summary>
        // It is used in the abstract constructor to store the parent. Can't use a generic T here because the IController depends on 
        // this MFDeviceBase class and this causes kind of a "circular reference" with the generics.
        public IPort<MFDevice> ProtectedParent { get; set; }

        protected MFDevice(object parent)
        {
            ProtectedParent = (IPort<MFDevice>) parent;

            // create debug engine for device
            // has to be implemented in the derived class
            //CreateDebugEngine();
        }

        #region Disposable implementation

        public bool disposed { get; private set; }

        ~MFDevice()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // release managed components
                    //Disconnect();
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Standard Dispose method for releasing resources such as the connection to the device.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        //public abstract bool Disconnect();

        //public abstract Task<PingConnectionType> Ping();

        public Task<bool> ConnectAsync() { return new Task<bool>(()=> { return false; }); }

        /// <summary>
        /// Creates the debug engine for the MF device. Needs to be implemented at the derived classes.
        /// </summary>
        //protected abstract void CreateDebugEngine();
    }
}
