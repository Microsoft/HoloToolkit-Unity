﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// Basic press event receiver
    /// </summary>
    public class OnPressReceiver : ReceiverBase
    {
        [InspectorField(Type = InspectorField.FieldTypes.Event, Label = "On Deselect", Tooltip = "The toggle is deselected")]
        public UnityEvent OnRelease = new UnityEvent();

        private bool hasDown;
        private State lastState;

        public OnPressReceiver(UnityEvent ev) : base(ev)
        {
            Name = "OnPress";
        }

        public override void OnUpdate(InteractableStates state, Interactable source)
        {
            bool changed = state.CurrentState() != lastState;

            bool hadDown = hasDown;
            hasDown = state.GetState(InteractableStates.InteractableStateEnum.Pressed).Value > 0;

            bool focused = state.GetState(InteractableStates.InteractableStateEnum.Focus).Value > 0;

            if (changed && hasDown != hadDown && focused)
            {
                if (hasDown)
                {
                    uEvent.Invoke();
                }
                else
                {
                    OnRelease.Invoke();
                }
            }
            
            lastState = state.CurrentState();
        }
    }
}
