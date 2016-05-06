﻿//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

namespace Microsoft.SPOT.Debugger
{
    public class RuntimeValue_ByRef<T> : RuntimeValue_Indirect<T> where T : MFDevice
    {
        protected internal RuntimeValue_ByRef(Engine<T> eng, WireProtocol.Commands.Debugging_Value[] array, int pos) : base(eng, array, pos)
        {
            if (m_value == null && m_handle.m_arrayref_referenceID != 0)
            {
                var task = m_eng.GetArrayElementAsync(m_handle.m_arrayref_referenceID, m_handle.m_arrayref_index);
                task.Start();
                if (task.Wait(5000))
                {
                    m_value = task.Result;
                }
            }

            if (m_value == null)
            {
                throw new ArgumentException();
            }
        }

        public override bool IsReference { get { return true; } }
        public override bool IsNull { get { return m_value.IsNull; } }
    }
}
