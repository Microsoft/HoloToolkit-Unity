﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Internal.Definitions.Devices;
using Microsoft.MixedReality.Toolkit.Internal.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;

namespace Microsoft.MixedReality.Toolkit.Internal.Devices.OpenXR
{
    public struct OpenXRController : IMixedRealityController
    {
        public ControllerState ControllerState { get; }

        public Handedness ControllerHandedness { get; }

        public IMixedRealityInputSource InputSource { get; }

        public Dictionary<DeviceInputType, InteractionMapping> Interactions { get; }

        public void SetupInputSource<T>(IMixedRealityInputSystem inputSystem, T state)
        {
           // TODO
        }

        public void UpdateInputSource<T>(IMixedRealityInputSystem inputSystem, T state)
        {
            //TODO
        }
    }
}
