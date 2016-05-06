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
    public class RuntimeValue_Reflection<T> : RuntimeValue<T> where T : MFDevice
    {
        private ReflectionDefinition m_rd;

        protected internal RuntimeValue_Reflection(Engine<T> eng, WireProtocol.Commands.Debugging_Value handle) : base(eng, handle)
        {
            m_rd = (ReflectionDefinition)Activator.CreateInstance((typeof(ReflectionDefinition)));

            m_eng.CreateConverter().Deserialize(m_rd, handle.m_builtinValue);
        }

        public override bool IsReference { get { return false; } }
        public override bool IsNull { get { return false; } }
        public override bool IsPrimitive { get { return false; } }
        public override bool IsValueType { get { return false; } }
        public override bool IsArray { get { return false; } }
        public override bool IsReflection { get { return true; } }

        public ReflectionDefinition.Kind ReflectionType
        {
            get
            {
                return (ReflectionDefinition.Kind)m_rd.m_kind;
            }
        }

        public ushort ArrayDepth
        {
            get
            {
                return m_rd.m_levels;
            }
        }

        public uint ReflectionIndex
        {
            get
            {
                return m_rd.m_raw;
            }
        }
    }
}
