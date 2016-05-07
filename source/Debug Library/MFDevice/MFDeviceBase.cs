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

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.SPOT.Debugger;
using Microsoft.SPOT.Debugger.WireProtocol;
using System.Threading.Tasks;

namespace Microsoft.NetMicroFramework.Tools
{
    public class MFDeviceBase
    {
        /// <summary>
        /// .NETMF debug engine
        /// </summary>
        public Engine DebugEngine { get; set; }

        /// <summary>
        /// Transport to the device. 
        /// </summary>
        public TransportType Transport { get; protected set; }

        public object DeviceObject { get; set; }

        /// <summary>
        /// Pings MF device for presence.
        /// </summary>
        /// <returns></returns>
        public async Task<PingConnectionType> PingAsync()
        {
            var reply = await DebugEngine.GetConnectionSourceAsync();

            if (reply != null)
            {
                switch (reply.m_source)
                {
                    case Commands.Monitor_Ping.c_Ping_Source_TinyCLR:
                        return PingConnectionType.TinyCLR;

                    case Commands.Monitor_Ping.c_Ping_Source_TinyBooter:
                        return PingConnectionType.TinyBooter;
                }
            }

            return PingConnectionType.NoConnection;
        }
    }
}
