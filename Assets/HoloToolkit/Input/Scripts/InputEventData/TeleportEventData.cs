﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine.EventSystems;

namespace HoloToolkit.Unity.InputModule
{
    public class TeleportEventData : InputEventData
    {
        public TeleportEventData(EventSystem eventSystem) : base(eventSystem) { }
    }
}