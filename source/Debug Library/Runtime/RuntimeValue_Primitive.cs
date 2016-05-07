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
using static Microsoft.SPOT.Debugger.TypeExtensions;

namespace Microsoft.SPOT.Debugger
{
    public class RuntimeValue_Primitive : RuntimeValue
    {
        protected object m_value;

        protected internal RuntimeValue_Primitive(Engine eng, WireProtocol.Commands.Debugging_Value handle) : base(eng, handle)
        {
            Type t;

            switch ((RuntimeDataType)handle.m_dt)
            {
                case RuntimeDataType.DATATYPE_BOOLEAN: t = typeof(bool); break;
                case RuntimeDataType.DATATYPE_I1: t = typeof(sbyte); break;
                case RuntimeDataType.DATATYPE_U1: t = typeof(byte); break;

                case RuntimeDataType.DATATYPE_CHAR: t = typeof(char); break;
                case RuntimeDataType.DATATYPE_I2: t = typeof(short); break;
                case RuntimeDataType.DATATYPE_U2: t = typeof(ushort); break;

                case RuntimeDataType.DATATYPE_I4: t = typeof(int); break;
                case RuntimeDataType.DATATYPE_U4: t = typeof(uint); break;
                case RuntimeDataType.DATATYPE_R4: t = typeof(float); break;

                case RuntimeDataType.DATATYPE_I8: t = typeof(long); break;
                case RuntimeDataType.DATATYPE_U8: t = typeof(ulong); break;
                case RuntimeDataType.DATATYPE_R8: t = typeof(double); break;

                default: throw new ArgumentException(String.Format("Not a primitive: {0}", handle.m_dt));
            }

            m_value = Activator.CreateInstance(t);

            m_eng.CreateConverter().Deserialize(m_value, handle.m_builtinValue);
        }

        public override bool IsReference { get { return false; } }
        public override bool IsNull { get { return false; } }
        public override bool IsPrimitive { get { return true; } }
        public override bool IsValueType { get { return false; } }
        public override bool IsArray { get { return false; } }
        public override bool IsReflection { get { return false; } }

        public override object Value
        {
            get
            {
                return m_value;
            }

            set
            {
                if (value != null)
                {
                    Type t = value.GetType();

                    if (m_value.GetType() == t)
                    {
                        object valToSerialize;

                        //
                        // Sign- or zero-extend to at least 32 bits.
                        //
                        switch (TypeExtensions.GetTypeCode(t))
                        {
                            case TypeCode.Boolean: valToSerialize = (bool)value ? 1 : 0; break;
                            case TypeCode.Char: valToSerialize = (uint)(char)value; break;
                            case TypeCode.SByte: valToSerialize = (int)(sbyte)value; break;
                            case TypeCode.Byte: valToSerialize = (uint)(byte)value; break;
                            case TypeCode.Int16: valToSerialize = (int)(short)value; break;
                            case TypeCode.UInt16: valToSerialize = (uint)(ushort)value; break;
                            default: valToSerialize = value; break;
                        }

                        byte[] data = m_eng.CreateConverter().Serialize(valToSerialize);

                        // need to call SetBlockAsync in a sync fashion
                        var task = SetBlockAsync(m_handle.m_dt, data);
                        task.Start();
                        if (task.Wait(5000))
                        {
                            if (task.Result)
                            {
                                m_value = value;
                            }
                        }
                    }
                }
            }
        }
    }
}
