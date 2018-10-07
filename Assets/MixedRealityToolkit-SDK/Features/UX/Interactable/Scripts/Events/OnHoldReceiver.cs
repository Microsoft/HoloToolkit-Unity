﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// Basic hold event receiver
    /// </summary>
    public class OnHoldReceiver : ReceiverBase
    {
        [InspectorField(Type = InspectorField.FieldTypes.Float, Label = "Hold Time", Tooltip = "The amount of time to press before triggering event")]
        public float HoldTime = 1f;

        private float clickTimer = 0;

        private bool hasDown;
        private State lastState;

        public OnHoldReceiver(UnityEvent ev): base(ev)
        {
            Name = "OnHold";
        }

        public override void OnUpdate(InteractableStates state, Interactable source)
        {
            
            if (state.GetState(InteractableStates.InteractableStateEnum.Pressed).Value > 0 && !hasDown)
            {
                hasDown = true;
                clickTimer = 0;
            }
            else if(state.GetState(InteractableStates.InteractableStateEnum.Pressed).Value < 1)
            {
                hasDown = false;
            }

            Debug.Log(HoldTime);

            if (hasDown && clickTimer < HoldTime)
            {
                clickTimer += Time.deltaTime;

                if (clickTimer >= HoldTime)
                {
                    Debug.Log("Hold!!");
                    uEvent.Invoke();
                }
            }
            
            lastState = state.CurrentState();
        }
    }
}
