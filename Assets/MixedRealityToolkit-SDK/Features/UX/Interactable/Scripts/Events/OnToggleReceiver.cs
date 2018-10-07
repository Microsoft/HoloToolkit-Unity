﻿using System.Collections;
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// a receiver that listens to toggle events
    /// </summary>
    public class OnToggleReceiver : ReceiverBase
    {
        [InspectorField(Type = InspectorField.FieldTypes.Event, Label = "On Deselect", Tooltip = "The toggle is deselected")]
        public UnityEvent OnDeselect = new UnityEvent();
        
        private State lastState;
        private int lastIndex;

        public OnToggleReceiver(UnityEvent ev) : base(ev)
        {
            Name = "OnSelect";
        }

        public override void OnUpdate(InteractableStates state, Interactable source)
        {
            int currentIndex = source.GetDimensionIndex();

            if (currentIndex != lastIndex)
            {
                if (currentIndex%2 == 0)
                {
                    OnDeselect.Invoke();
                }
                else
                {
                    uEvent.Invoke();
                }
            }

            lastIndex = currentIndex;
            lastState = state.CurrentState();
        }
    }
}
