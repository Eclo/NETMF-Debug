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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
    [Deprecated("Deprecated. Use MFDeviceInfo class instead.", DeprecationType.Deprecate, 1)]
    class MFDeviceInfoImpl : IMFDeviceInfo
    {
        public IAppDomainInfo[] AppDomains
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAssemblyInfo[] Assemblies
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ClrBuildInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Version ClrBuildVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string HalBuildInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Version HalBuildVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public byte Model
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ModuleSerialNumber
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public byte OEM
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ushort SKU
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SolutionBuildInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Version SolutionBuildVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SystemSerialNumber
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Version TargetFrameworkVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool Valid
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    class MFDeviceInfo : IMFDeviceInfo
    {
        private MFDeviceBase m_self;
        private bool m_fValid;

        private List<IAppDomainInfo> m_Domains = new List<IAppDomainInfo>();
        private List<IAssemblyInfo> m_AssemblyInfos = new List<IAssemblyInfo>();

        public MFDeviceInfo(MFDeviceBase device)
        {
            m_self = device;

            m_fValid = false;
        }

        public async Task<bool> GetDeviceInfo()
        {
            if (!Dbg.IsConnectedToTinyCLR) return false;

            //await m_self.DoForEachAppDomainAsync(
            //    delegate (IAppDomainInfo adi)
            //    {
            //        m_Domains.Add(adi);
            //    }
            //);

            //await m_self.DoForEachAssemblyAsync(
            //    delegate (IAssemblyInfo ai)
            //    {
            //        m_AssemblyInfos.Add(ai);
            //    }
            //);

            m_fValid = true;

            return true;
        }

        private Microsoft.SPOT.Debugger.Engine Dbg { get { return m_self.DebugEngine; } }

        public bool Valid { get { return m_fValid; } }

        public System.Version HalBuildVersion
        {
            get { return Dbg.Capabilities.HalSystemInfo.halVersion; }
        }

        public string HalBuildInfo
        {
            get { return Dbg.Capabilities.HalSystemInfo.halVendorInfo; }
        }

        public byte OEM
        {
            get { return Dbg.Capabilities.HalSystemInfo.oemCode; }
        }

        public byte Model
        {
            get { return Dbg.Capabilities.HalSystemInfo.modelCode; }
        }

        public ushort SKU
        {
            get { return Dbg.Capabilities.HalSystemInfo.skuCode; }
        }

        public string ModuleSerialNumber
        {
            get { return Dbg.Capabilities.HalSystemInfo.moduleSerialNumber; }
        }

        public string SystemSerialNumber
        {
            get { return Dbg.Capabilities.HalSystemInfo.systemSerialNumber; }
        }

        public System.Version ClrBuildVersion
        {
            get { return Dbg.Capabilities.ClrInfo.clrVersion; }
        }

        public string ClrBuildInfo
        {
            get { return Dbg.Capabilities.ClrInfo.clrVendorInfo; }
        }

        public System.Version TargetFrameworkVersion
        {
            get { return Dbg.Capabilities.ClrInfo.targetFrameworkVersion; }
        }

        public System.Version SolutionBuildVersion
        {
            get { return Dbg.Capabilities.SolutionReleaseInfo.solutionVersion; }
        }

        public string SolutionBuildInfo
        {
            get { return Dbg.Capabilities.SolutionReleaseInfo.solutionVendorInfo; }
        }

        public IAppDomainInfo[] AppDomains
        {
            get { return m_Domains.ToArray(); }
        }

        public IAssemblyInfo[] Assemblies
        {
            get { return m_AssemblyInfos.ToArray(); }
        }

        public override string ToString()
        {
            if (!m_fValid)
            {
                return "DeviceInfo is not valid!";
            }
            else
            {
                StringBuilder output = new StringBuilder();

                output.AppendLine("DeviceInfo:");
                output.AppendLine(String.Format("  HAL build info: {0}, {1}", HalBuildVersion.ToString(), HalBuildInfo));
                output.AppendLine(String.Format("  OEM Product codes (vendor, model, SKU): {0}, {1}, {2}", OEM.ToString(), Model.ToString(), SKU.ToString()));
                output.AppendLine("  Serial Numbers (module, system):");
                output.AppendLine("    " + ModuleSerialNumber);
                output.AppendLine("    " + SystemSerialNumber);
                output.AppendLine(String.Format("  Solution Build Info: {0}, {1}", SolutionBuildVersion.ToString(), SolutionBuildInfo));

                output.AppendLine("  AppDomains:");
                foreach (IAppDomainInfo adi in AppDomains)
                {
                    output.AppendLine(String.Format("    {0}, id={1}", adi.Name, adi.ID));
                }

                output.AppendLine("  Assemblies:");
                foreach (IAssemblyInfo ai in Assemblies)
                {
                    output.AppendLine(String.Format("    {0},{1}", ai.Name, ai.Version));
                }

                return output.ToString();
            }
        }
    }
}
