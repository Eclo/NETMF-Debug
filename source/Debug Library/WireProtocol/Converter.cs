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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using static Microsoft.SPOT.Debugger.TypeExtensions;

namespace Microsoft.SPOT.Debugger.WireProtocol
{
    public interface IConverter
    {
        void PrepareForDeserialize(int size, byte[] data, Converter converter);
    }

    public class Converter
    {
        public Converter() : this(null)
        {
        }

        public Converter(CLRCapabilities capabilities)
        {
            if (capabilities == null)
            {
                Capabilities = new CLRCapabilities();
            }
            else
            {
                Capabilities = capabilities;
            }
        }

        public CLRCapabilities Capabilities { get; protected set; }

        public byte[] Serialize(object o)
        {
            MemoryStream stream = new MemoryStream();

            Serialize(stream, o);

            return stream.ToArray();
        }

        public void Deserialize(object o, byte[] buf)
        {
            MemoryStream stream = new MemoryStream(buf != null ? buf : new byte[1]);

            IConverter itf = o as IConverter; if (itf != null) itf.PrepareForDeserialize(buf.Length, buf, this);

            Deserialize(stream, o);
        }

        private void Serialize(Stream stream, object o)
        {
            BinaryWriter writer = new BinaryWriter(stream, Encoding.Unicode);

            InternalSerializeFields(writer, o);
        }

        private void InternalSerializeFields(BinaryWriter writer, object o)
        {
            Type t = o.GetType();

            if (t.IsArray)
            {
                InternalSerializeInstance(writer, o);
            }

            while (t != null)
            {
                foreach (FieldInfo f in t.GetRuntimeFields().Where(f => f.Attributes == FieldAttributes.Public))
                {
                    InternalSerializeInstance(writer, f.GetValue(o));
                }

                t = t.GetTypeInfo().BaseType;
            }
        }

        private void InternalSerializeInstance(BinaryWriter writer, object o)
        {
            Type t = o.GetType();

            switch (TypeExtensions.GetTypeCode(t))
            {
                case TypeCode.Boolean: writer.Write((bool)o); break;
                case TypeCode.Char: writer.Write((char)o); break;
                case TypeCode.SByte: writer.Write((sbyte)o); break;
                case TypeCode.Byte: writer.Write((byte)o); break;
                case TypeCode.Int16: writer.Write((short)o); break;
                case TypeCode.UInt16: writer.Write((ushort)o); break;
                case TypeCode.Int32: writer.Write((int)o); break;
                case TypeCode.UInt32: writer.Write((uint)o); break;
                case TypeCode.Int64: writer.Write((long)o); break;
                case TypeCode.UInt64: writer.Write((ulong)o); break;
                case TypeCode.Single:
                    if (Capabilities.FloatingPoint) writer.Write((float)o);
                    else writer.Write((int)((float)o * 1024));
                    break;
                case TypeCode.Double:
                    if (Capabilities.FloatingPoint) writer.Write((double)o);
                    else writer.Write((long)((double)o * 65536));
                    break;
                case TypeCode.String:
                    byte[] buf = Encoding.UTF8.GetBytes((string)o);

                    writer.Write(buf.Length);
                    writer.Write(buf);
                    break;
                default:
                    if (t == typeof(void))
                    {
                    }
                    else if (t.IsArray)
                    {
                        Array arr = (Array)o;

                        foreach (object arrItem in arr)
                        {
                            InternalSerializeInstance(writer, arrItem);
                        }
                    }
                    else if (t.GetTypeInfo().IsValueType || t.GetTypeInfo().IsClass)
                    {
                        InternalSerializeFields(writer, o);
                    }
                    else
                    {
                        throw new SerializationException();
                    }

                    break;
            }
        }

        private void Deserialize(Stream stream, object o)
        {
            BinaryReader reader = new BinaryReader(stream, Encoding.Unicode);

            InternalDeserializeFields(reader, o);
        }

        private void InternalDeserializeFieldsHelper(BinaryReader reader, object o, Type t)
        {
            if (t.GetTypeInfo().BaseType != null)
            {
                InternalDeserializeFieldsHelper(reader, o, t.GetTypeInfo().BaseType);
            }

            foreach (FieldInfo f in t.GetRuntimeFields().Where(f => f.Attributes == FieldAttributes.Public))
            {
                Type ft = f.FieldType;
                object objValue = f.GetValue(o);

                objValue = InternalDeserializeInstance(reader, objValue, ft);

                f.SetValue(o, objValue);
            }
        }

        private void InternalDeserializeFields(BinaryReader reader, object o)
        {
            Type t = o.GetType();

            if (t.IsArray)
            {
                InternalDeserializeInstance(reader, o, t);
            }
            else
            {
                InternalDeserializeFieldsHelper(reader, o, t);
            }
        }

        private object InternalDeserializeInstance(BinaryReader reader, object o, Type t)
        {
            object ret = null;
            if (o != null)
            {
                //This allows PrepareForDeserialize to subclass the expected type if appropriate
                t = o.GetType();
            }

            switch (TypeExtensions.GetTypeCode(t))
            {
                case TypeCode.Boolean: ret = reader.ReadBoolean(); break;
                case TypeCode.Char: ret = reader.ReadChar(); break;
                case TypeCode.SByte: ret = reader.ReadSByte(); break;
                case TypeCode.Byte: ret = reader.ReadByte(); break;
                case TypeCode.Int16: ret = reader.ReadInt16(); break;
                case TypeCode.UInt16: ret = reader.ReadUInt16(); break;
                case TypeCode.Int32: ret = reader.ReadInt32(); break;
                case TypeCode.UInt32: ret = reader.ReadUInt32(); break;
                case TypeCode.Int64: ret = reader.ReadInt64(); break;
                case TypeCode.UInt64: ret = reader.ReadUInt64(); break;
                case TypeCode.Single:
                    if (Capabilities.FloatingPoint) ret = reader.ReadSingle();
                    else ret = (float)reader.ReadInt32() / 1024;
                    break;
                case TypeCode.Double:
                    if (Capabilities.FloatingPoint) ret = reader.ReadDouble();
                    else ret = (double)reader.ReadInt64() / 65536;
                    break;
                case TypeCode.String:
                    int num = reader.ReadInt32();
                    byte[] buf = reader.ReadBytes(num);

                    ret = Encoding.UTF8.GetString(buf, 0, num);

                    break;
                default:
                    if (t.IsArray)
                    {
                        Array arr = (Array)o;

                        for (int i = 0; i < arr.Length; i++)
                        {
                            object objValue = arr.GetValue(i);

                            objValue = InternalDeserializeInstance(reader, objValue, t.GetElementType());
                            arr.SetValue(objValue, i);
                        }

                        ret = o;
                    }
                    else if (t.GetTypeInfo().IsValueType || t.GetTypeInfo().IsClass)
                    {
                        if (o != null)
                        {
                            if (o.GetType() != t) throw new System.Runtime.Serialization.SerializationException();
                        }
                        else
                        {
                            o = Activator.CreateInstance(t);
                        }

                        InternalDeserializeFields(reader, o);

                        ret = o;
                    }
                    else
                    {
                        throw new System.Runtime.Serialization.SerializationException();
                    }

                    break;
            }

            return ret;
        }
    }
}
