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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SPOT.Debugger
{
    public static class OutputExtensions
    {
        /// <summary>
        /// Prints a nicely formated output of a MemoryMap array.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static string ToFriendlyString(this Commands.Monitor_MemoryMap.Range[] range)
        {
            StringBuilder output = new StringBuilder();

            if (range != null && range.Length > 0)
            {
                output.AppendLine("Type     Start       Size");
                output.AppendLine("--------------------------------");
                for (int i = 0; i < range.Length; i++)
                {
                    string mem = "";
                    switch (range[i].m_flags)
                    {
                        case Commands.Monitor_MemoryMap.c_FLASH:
                            mem = "FLASH";
                            break;
                        case Commands.Monitor_MemoryMap.c_RAM:
                            mem = "RAM";
                            break;
                    }
                    output.AppendLine(string.Format("{0,-6} 0x{1:x08}  0x{2:x08}", mem, range[i].m_address, range[i].m_length));
                }
            }

            return output.ToString();
        }
    }
}
