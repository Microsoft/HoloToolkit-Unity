﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine.EventSystems;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// Interface to implement to react to hold gestures.
    /// </summary>
    public interface IHoldHandler : IEventSystemHandler
    {
        void OnHoldStarted(InputEventData eventData);
        void OnHoldCompleted(InputEventData eventData);
        void OnHoldCanceled(InputEventData eventData);
    }
}