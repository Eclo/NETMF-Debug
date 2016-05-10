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

using Microsoft.SPOT.Debugger.WireProtocol;
using System;
using System.Threading.Tasks;

namespace Microsoft.NetMicroFramework.Tools
{
    public class MFDevice<T> : MFDeviceBase, IDisposable, IMFDevice where T : new()
    {
        public T Device { get; set; }

        public MFDevice()
        {
            Device = new T();
            DeviceObject = Device;
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
                    try
                    {
                        // release managed components
                        ((IMFDevice)Device).Disconnect();
                    }
                    catch { }
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

        /// <summary>
        /// Connect to NETMF device
        /// </summary>
        /// <returns>True if operation is successful</returns>
        public async Task<bool> ConnectAsync()
        {
            return await ((IMFDevice)Device).ConnectAsync();
        }

        /// <summary>
        /// Disconnect NETMF device
        /// </summary>
        public void Disconnect()
        {
            ((IMFDevice)Device).Disconnect();
        }
    }
}
