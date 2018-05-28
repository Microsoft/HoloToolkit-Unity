﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions;
using Microsoft.MixedReality.Toolkit.Internal.Definitions.InputSystem;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.Internal.EventDatum.Input
{
    /// <summary>
    /// Describes an Input Event that involves a tap, click, or touch.
    /// </summary>
    public class MixedRealityPointerEventData : InputEventData
    {
        /// <summary>
        /// Number of Clicks, Taps, or Presses that triggered the event.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Pointer for the Input Event
        /// </summary>
        public IMixedRealityPointer Pointer { get; private set; }

        /// <inheritdoc />
        public MixedRealityPointerEventData(EventSystem eventSystem) : base(eventSystem) { }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="count"></param>
        public void Initialize(IMixedRealityPointer pointer, int count)
        {
            BaseInitialize(pointer.InputSourceParent);
            Pointer = pointer;
            Count = count;
        }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="count"></param>
        /// <param name="handedness"></param>
        public void Initialize(IMixedRealityPointer pointer, int count, Handedness handedness)
        {
            Initialize(pointer.InputSourceParent, handedness);
            Pointer = pointer;
            Count = count;
        }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="count"></param>
        /// <param name="inputAction"></param>
        /// <param name="handedness"></param>
        public void Initialize(IMixedRealityPointer pointer, int count, InputAction inputAction, Handedness handedness)
        {
            Initialize(pointer.InputSourceParent, handedness, inputAction);
            Pointer = pointer;
            Count = count;
        }
    }
}