﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Services;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Tests.Services
{
    internal class TestExtensionService1 : BaseExtensionService, ITestExtensionService1
    {
        public TestExtensionService1(string name, uint priority, ScriptableObject profile) : base(name, priority, profile) { }

        public bool IsEnabled { get; private set; }

        public override void Enable()
        {
            IsEnabled = true;
        }

        public override void Disable()
        {
            IsEnabled = false;
        }

        public override void Destroy()
        {
        }
    }
}