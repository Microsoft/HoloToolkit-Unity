﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions.Devices;
using Microsoft.MixedReality.Toolkit.Internal.Definitions.InputSystem;
using Microsoft.MixedReality.Toolkit.Internal.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Internal.Extensions;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using Microsoft.MixedReality.Toolkit.Internal.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace Microsoft.MixedReality.Toolkit.Internal.Devices.OpenXR
{
    // TODO - Implement
    public class OpenXRDevice : IMixedRealityDevice
    {
        /// <summary>
        /// Dictionary to capture all active controllers detected
        /// </summary>
        private readonly Dictionary<uint, IMixedRealityController> activeControllers = new Dictionary<uint, IMixedRealityController>();

        /// <summary>
        /// Input System reference
        /// </summary>
        private IMixedRealityInputSystem inputSystem;

        /// <summary>
        /// Public accessor for the currently attached controllers for a Windows Mixed Reality Device
        /// </summary>
        /// <returns></returns>
        public IMixedRealityController[] GetActiveControllers()
        {
            return activeControllers.ExportDictionaryValuesAsArray();
        }

        #region Device Initialization

        // TODO To be determined if needed
        /// <summary>
        /// Public Constructor
        /// </summary>
        public OpenXRDevice()
        {
            // TODO - Discover available controllers?
            // ForEach then initialize each
        }

        /// <summary>
        /// The initialize function is used to setup the device once created.
        /// This method is called once all managers have been registered in the Mixed Reality Manager.
        /// </summary>
        public void Initialize()
        {
            inputSystem = MixedRealityManager.Instance.GetManager<IMixedRealityInputSystem>();

            InitializeSources();
        }

        /// <summary>
        /// The internal initialize function is used to register for controller events.
        /// </summary>
        private void InitializeSources()
        {
            #region Experimental_WSA native device input

#if UNITY_STANDALONE

            //TODO - Investigate .Player project option for OpenXR

#endif
            #endregion Experimental_WSA native device input

            InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
            InteractionManager.InteractionSourcePressed += InteractionManager_InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += InteractionManager_InteractionSourceReleased;
            InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;

            InteractionSourceState[] states = InteractionManager.GetCurrentReading();

            // NOTE: We update the source state data, in case an app wants to query it on source detected.
            for (var i = 0; i < states.Length; i++)
            {
                InteractionSourceDetected(states[i]);
            }
        }

        #endregion Device Initialization

        #region Experimental_PLAYER native device input

#if UNITY_STANDALONE

        //TODO - kept for reference - clean later.

#endif
        #endregion Experimental_PLAYER native device input

        #region OpenXR controller handlers

        /// <summary>
        /// Retrieve the source controller from the Active Store, or create a new device and register it
        /// </summary>
        /// <param name="interactionSourceState">Source State provided by the SDK</param>
        /// <returns>New or Existing Controller Input Source</returns>
        private IMixedRealityController GetOrAddOpenXRController(InteractionSourceState interactionSourceState)
        {
            //If a device is already registered with the ID provided, just return it.
            if (activeControllers.ContainsKey(interactionSourceState.source.id))
            {
                return activeControllers[interactionSourceState.source.id];
            }

            //TODO - Controller Type Detection?
            //Define new Controller
            var detectedController = new GenericOpenXRController(
                ControllerState.Tracked,
                interactionSourceState.source.handedness == InteractionSourceHandedness.Left ? Handedness.Left : Handedness.Right,
                null,
                null
                );

            detectedController.SetupInputSource(interactionSourceState);

            activeControllers.Add(interactionSourceState.source.id, detectedController);

            return detectedController;
        }

        /// <summary>
        /// Remove the selected controller from the Active Store
        /// </summary>
        /// <param name="interactionSourceState">Source State provided by the SDK to remove</param>
        private void RemoveOpenXRController(InteractionSourceState interactionSourceState)
        {
            var controller = GetOrAddOpenXRController(interactionSourceState);
            if (controller == null) { return; }

            if (MixedRealityManager.Instance.ActiveProfile.EnableInputSystem) inputSystem?.RaiseSourceLost(controller.InputSource);
            activeControllers.Remove(interactionSourceState.source.id);
        }

        /// <summary>
        /// Register a new controller in the Active Store
        /// </summary>
        /// <param name="interactionSourceState">Source State provided by the SDK to add</param>
        private void InteractionSourceDetected(InteractionSourceState interactionSourceState)
        {
            var controller = GetOrAddOpenXRController(interactionSourceState);
            if (controller == null) { return; }

            // NOTE: We update the source state data, in case an app wants to query it on source detected.
            controller.UpdateInputSource(interactionSourceState);
            if (MixedRealityManager.Instance.ActiveProfile.EnableInputSystem) inputSystem?.RaiseSourceDetected(controller.InputSource);
        }

        private void InteractionSourceUpdated(InteractionSourceState state)
        {
            var controller = GetOrAddOpenXRController(state);

            if (controller == null) { return; }

            controller.UpdateInputSource(state);

            if (MixedRealityManager.Instance.ActiveProfile.EnableInputSystem)
            {
                if (controller.Interactions.ContainsKey(DeviceInputType.SpatialPointer) && controller.Interactions.GetDictionaryValueChanged(DeviceInputType.SpatialPointer))
                {
                    inputSystem?.Raise6DofInputChanged(controller.InputSource, controller.ControllerHandedness, controller.Interactions[DeviceInputType.SpatialPointer].InputAction, controller.Interactions[DeviceInputType.SpatialPointer].GetTransform());
                }

                if (controller.Interactions.ContainsKey(DeviceInputType.SpatialPointer) && controller.Interactions.GetDictionaryValueChanged(DeviceInputType.SpatialGrip))
                {
                    inputSystem?.Raise6DofInputChanged(controller.InputSource, controller.ControllerHandedness, controller.Interactions[DeviceInputType.SpatialGrip].InputAction, controller.Interactions[DeviceInputType.SpatialGrip].GetTransform());
                }

                if (controller.Interactions.ContainsKey(DeviceInputType.SpatialPointer) && controller.Interactions.GetDictionaryValueChanged(DeviceInputType.TouchpadTouch))
                {
                    if (controller.Interactions[DeviceInputType.TouchpadTouch].GetBooleanValue())
                    {
                        inputSystem?.RaiseOnInputDown(controller.InputSource, controller.ControllerHandedness, controller.Interactions[DeviceInputType.TouchpadTouch].InputAction);
                    }
                    else
                    {
                        inputSystem?.RaiseOnInputUp(controller.InputSource, controller.ControllerHandedness, controller.Interactions[DeviceInputType.TouchpadTouch].InputAction);
                    }
                }

                if (controller.Interactions.ContainsKey(DeviceInputType.SpatialPointer) && controller.Interactions.GetDictionaryValueChanged(DeviceInputType.Touchpad))
                {
                    inputSystem?.Raise2DoFInputChanged(controller.InputSource, controller.ControllerHandedness, controller.Interactions[DeviceInputType.Touchpad].InputAction, controller.Interactions[DeviceInputType.Touchpad].GetVector2Value());
                }

                if (controller.Interactions.ContainsKey(DeviceInputType.SpatialPointer) && controller.Interactions.GetDictionaryValueChanged(DeviceInputType.ThumbStick))
                {
                    inputSystem?.Raise2DoFInputChanged(controller.InputSource, controller.ControllerHandedness, controller.Interactions[DeviceInputType.ThumbStick].InputAction, controller.Interactions[DeviceInputType.ThumbStick].GetVector2Value());
                }

                if (controller.Interactions.ContainsKey(DeviceInputType.SpatialPointer) && controller.Interactions.GetDictionaryValueChanged(DeviceInputType.Trigger))
                {
                    inputSystem?.RaiseOnInputPressed(controller.InputSource, controller.ControllerHandedness, controller.Interactions[DeviceInputType.Select].InputAction, controller.Interactions[DeviceInputType.Trigger].GetFloatValue());
                }
            }
        }

        private void InteractionSourcePressed(InteractionSourceState state, InteractionSourcePressType pressType)
        {
            var controller = GetOrAddOpenXRController(state);
            if (controller == null) { return; }

            var inputAction = PressInteractionSource(pressType, controller);

            if (MixedRealityManager.Instance.ActiveProfile.EnableInputSystem) inputSystem?.RaiseOnInputDown(controller.InputSource, controller.ControllerHandedness, inputAction);
        }

        private void InteractionSourceReleased(InteractionSourceState state, InteractionSourcePressType pressType)
        {
            var controller = GetOrAddOpenXRController(state);
            if (controller == null) { return; }

            var inputAction = ReleaseInteractionSource(pressType, controller);

            if (MixedRealityManager.Instance.ActiveProfile.EnableInputSystem) inputSystem.RaiseOnInputUp(controller.InputSource, controller.ControllerHandedness, inputAction);
        }


        /// <summary>
        /// React to Input "Press" events and update source data
        /// </summary>
        /// <param name="interactionSourcePressType">Type of press event received</param>
        /// <param name="controller">Source controller to update</param>
        /// <returns></returns>
        private InputAction PressInteractionSource(InteractionSourcePressType interactionSourcePressType, IMixedRealityController controller)
        {
            DeviceInputType pressedInput;

            switch (interactionSourcePressType)
            {
                case InteractionSourcePressType.None:
                    pressedInput = DeviceInputType.None;
                    break;
                case InteractionSourcePressType.Select:
                    pressedInput = DeviceInputType.Select;
                    break;
                case InteractionSourcePressType.Menu:
                    pressedInput = DeviceInputType.Menu;
                    break;
                case InteractionSourcePressType.Grasp:
                    pressedInput = DeviceInputType.GripPress;
                    break;
                case InteractionSourcePressType.Touchpad:
                    pressedInput = DeviceInputType.TouchpadPress;
                    break;
                case InteractionSourcePressType.Thumbstick:
                    pressedInput = DeviceInputType.ThumbStickPress;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (controller.Interactions.ContainsKey(pressedInput))
            {
                controller.Interactions.SetDictionaryValue(pressedInput, true);
                return controller.Interactions[pressedInput].InputAction;
            }

            // if no mapping found, no action can take place
            return null;
        }


        /// <summary>
        /// React to Input "Release" events and update source data
        /// </summary>
        /// <param name="interactionSourcePressType">Type of release event received</param>
        /// <param name="controller">Source controller to update</param>
        /// <returns></returns>
        private InputAction ReleaseInteractionSource(InteractionSourcePressType interactionSourcePressType, IMixedRealityController controller)
        {
            DeviceInputType releasedInput;

            switch (interactionSourcePressType)
            {
                case InteractionSourcePressType.None:
                    releasedInput = DeviceInputType.None;
                    break;
                case InteractionSourcePressType.Select:
                    releasedInput = DeviceInputType.Select;
                    break;
                case InteractionSourcePressType.Menu:
                    releasedInput = DeviceInputType.Menu;
                    break;
                case InteractionSourcePressType.Grasp:
                    releasedInput = DeviceInputType.GripPress;
                    break;
                case InteractionSourcePressType.Touchpad:
                    releasedInput = DeviceInputType.TouchpadPress;
                    break;
                case InteractionSourcePressType.Thumbstick:
                    releasedInput = DeviceInputType.ThumbStickPress;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (controller.Interactions.ContainsKey(releasedInput))
            {
                controller.Interactions.SetDictionaryValue(releasedInput, false);
                return controller.Interactions[releasedInput].InputAction;
            }

            // if no mapping found, no action can take place
            return null;
        }
        #endregion

        #region Unity InteractionManager Events

        /// <summary>
        /// SDK Interaction Source Detected Event handler
        /// </summary>
        /// <param name="args">SDK source detected event arguments</param>
        private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs args)
        {
            InteractionSourceDetected(args.state);
        }

        /// <summary>
        /// SDK Interaction Source Updated Event handler
        /// </summary>
        /// <param name="args">SDK source updated event arguments</param>
        private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
        {
            InteractionSourceUpdated(args.state);
        }


        /// <summary>
        /// SDK Interaction Source Pressed Event handler
        /// </summary>
        /// <param name="args">SDK source pressed event arguments</param>
        private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs args)
        {
            InteractionSourcePressed(args.state,args.pressType);
        }


        /// <summary>
        /// SDK Interaction Source Released Event handler
        /// </summary>
        /// <param name="args">SDK source released event arguments</param>
        private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs args)
        {
            InteractionSourceReleased(args.state, args.pressType);
        }

        /// <summary>
        /// SDK Interaction Source Lost Event handler
        /// </summary>
        /// <param name="args">SDK source updated event arguments</param>
        private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
        {
            RemoveOpenXRController(args.state);
        }

        #endregion Unity InteractionManager Events

        #region Runtime

        //TODO - runtime needs more thought

        public void Enable()
        {
            InitializeSources();
        }

        public void Disable()
        {
            InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourcePressed -= InteractionManager_InteractionSourcePressed;
            InteractionManager.InteractionSourceUpdated -= InteractionManager_InteractionSourceUpdated;
            InteractionManager.InteractionSourceReleased -= InteractionManager_InteractionSourceReleased;
            InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;

            InteractionSourceState[] states = InteractionManager.GetCurrentReading();
            for (var i = 0; i < states.Length; i++)
            {
                RemoveOpenXRController(states[i]);
            }
        }

        public void Destroy()
        {
            //Destroy Stuff
        }

        #endregion

        #region IMixedRealityManager Interface

        public string Name { get; }

        public uint Priority { get; }

        public void Reset()
        {
        }

        public void Update()
        {
        }

        #endregion IMixedRealityManager Interface
    }
}