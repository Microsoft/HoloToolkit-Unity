﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.Core.EventDatum.Input
{
    /// <summary>
    /// Describes an Input Event associated with a specific pointer's focus state change.
    /// </summary>
    public class FocusEventData : BaseInputEventData
    {
        /// <summary>
        /// The pointer associated with this event.
        /// </summary>
        public IMixedRealityPointer Pointer { get; private set; }

        /// <summary>
        /// The focusProvider associated with this event.
        /// </summary>
        public IMixedRealityFocusProvider FocusProvider { get; private set; }

        /// <summary>
        /// The old focused object.
        /// </summary>
        public GameObject OldFocusedObject { get; private set; }

        /// <summary>
        /// The new focused object.
        /// </summary>
        public GameObject NewFocusedObject { get; private set; }

        /// <inheritdoc />
        public FocusEventData(EventSystem eventSystem) : base(eventSystem) { }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="pointer"></param>
        public void Initialize(IMixedRealityPointer pointer, IMixedRealityFocusProvider focusProvider)
        {
            BaseInitialize(pointer.InputSourceParent, Definitions.InputSystem.MixedRealityInputAction.None);
            Pointer = pointer;
            FocusProvider = focusProvider;
        }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="oldFocusedObject"></param>
        /// <param name="newFocusedObject"></param>
        public void Initialize(IMixedRealityPointer pointer, GameObject oldFocusedObject, GameObject newFocusedObject)
        {
            BaseInitialize(pointer.InputSourceParent, Definitions.InputSystem.MixedRealityInputAction.None);
            Pointer = pointer;
            OldFocusedObject = oldFocusedObject;
            NewFocusedObject = newFocusedObject;
        }
    }
}

