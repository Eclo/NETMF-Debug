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
using System;
using System.Collections.Generic;
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
        /// Device description.
        /// </summary>
        public string Description { get; set; }

        public async Task<IMFDeviceInfo> GetDeviceInfoAsync()
        {
            var deviceInfo = new MFDeviceInfo(this);

            if(await deviceInfo.GetDeviceInfo())
            {
                return deviceInfo;
            }

            return deviceInfo;
        }

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

        public delegate void AppDomainAction(IAppDomainInfo adi);

        public async Task DoForEachAppDomainAsync(AppDomainAction appDomainAction)
        {
            if (DebugEngine.Capabilities.AppDomains)
            {
                Commands.Debugging_TypeSys_AppDomains.Reply domainsReply = await DebugEngine.GetAppDomainsAsync();
                if (domainsReply != null)
                {
                    foreach (uint id in domainsReply.m_data)
                    {
                        Commands.Debugging_Resolve_AppDomain.Reply reply = await DebugEngine.ResolveAppDomainAsync(id);
                        if (reply != null)
                        {
                            appDomainAction(new AppDomainInfo(id, reply));
                        }
                    }
                }
            }
        }

        public delegate void AssemblyAction(IAssemblyInfo ai);
        public async Task DoForEachAssemblyAsync(AssemblyAction assemblyAction)
        {
            List<IAppDomainInfo> theDomains = new List<IAppDomainInfo>();

            await DoForEachAppDomainAsync(
                delegate (IAppDomainInfo adi)
                {
                    theDomains.Add(adi);
                }
            );

            Commands.Debugging_Resolve_Assembly[] reply = await DebugEngine.ResolveAllAssembliesAsync();

            if (reply != null)
                foreach (Commands.Debugging_Resolve_Assembly resolvedAssm in reply)
                {
                    AssemblyInfoFromResolveAssembly ai = new AssemblyInfoFromResolveAssembly(resolvedAssm);

                    foreach (IAppDomainInfo adi in theDomains)
                    {
                        if (Array.IndexOf<uint>(adi.AssemblyIndicies, ai.Index) != -1)
                        {
                            ai.AddDomain(adi);
                        }
                    }

                    assemblyAction(ai);
                }
        }
    }
}
