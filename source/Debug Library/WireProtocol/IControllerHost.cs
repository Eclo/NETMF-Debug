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
using System.Threading.Tasks;

namespace Microsoft.SPOT.Debugger.WireProtocol
{
    public interface IControllerHost<T> where T : MFDevice
    {
        DateTime LastActivity { get; set; }

        bool IsConnected { get; }

        Task StartSessionAsync(T device);

        void StopSession(T device);

        void SpuriousCharacters(byte[] buf, int offset, int count);

        void ProcessExited();
    }
}

