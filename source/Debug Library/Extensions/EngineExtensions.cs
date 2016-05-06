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
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.SPOT.Debugger.WireProtocol;
using System.Threading.Tasks;

namespace Microsoft.SPOT.Debugger
{
    public static class EngineExtensions
    {
        public static async Task<PingConnectionType> Ping<T>(this Engine<T> e) where T : MFDevice
        {
            try
            {
                var reply = await e.GetConnectionSourceAsync();

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
            }
            catch { }

            // default to no connection
            return PingConnectionType.NoConnection;
        }

    }
}
