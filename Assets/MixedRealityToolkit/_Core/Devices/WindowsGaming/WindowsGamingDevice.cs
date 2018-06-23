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

#if WINDOWS_UWP
using Windows.Gaming.Input;
#endif // WINDOWS_UWP

namespace Microsoft.MixedReality.Toolkit.Internal.Devices.WindowsGaming
{
    // TODO - Implement
    public class WindowsGamingDevice : IMixedRealityDevice
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
        public WindowsGamingDevice()
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
#if WINDOWS_UWP
            Gamepad.GamepadAdded += OnGamepadDetected;
            Gamepad.GamepadRemoved += OnGamepadLost;

            ArcadeStick.ArcadeStickAdded += OnArcadeStickDetected;
            ArcadeStick.ArcadeStickRemoved += OnArcadeStickLost;

            FlightStick.FlightStickAdded += OnFlightStickDetected;
            FlightStick.FlightStickRemoved += OnFlightStickLost;

            RacingWheel.RacingWheelAdded += OnRacingWheelDetected;
            RacingWheel.RacingWheelRemoved += OnRacingWheelLost;
#endif //WINDOWS_UWP
        }

        #endregion Device Initialization

        #region Experimental_PLAYER native device input

#if UNITY_STANDALONE

        //TODO - kept for reference - clean later.

#endif
        #endregion Experimental_PLAYER native device input

        #region Windows Gaming controller handlers

        /// <summary>
        /// Retrieve the source controller from the Active Store, or create a new device and register it
        /// </summary>
        /// <param name="interactionSourceState">Source State provided by the SDK</param>
        /// <returns>New or Existing Controller Input Source</returns>
        private IMixedRealityController GetOrAddWindowsGamingController(InteractionSourceState interactionSourceState)
        {
            //If a device is already registered with the ID provided, just return it.
            if (activeControllers.ContainsKey(interactionSourceState.source.id))
            {
                return activeControllers[interactionSourceState.source.id];
            }

            //TODO - Controller Type Detection?
            //Define new Controller
            var detectedController = new GamepadController(
                ControllerState.None,
                Handedness.None,
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
        private void RemoveWindowsGamingController(InteractionSourceState interactionSourceState)
        {
            var controller = GetOrAddWindowsGamingController(interactionSourceState);
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
            var controller = GetOrAddWindowsGamingController(interactionSourceState);
            if (controller == null) { return; }

            // NOTE: We update the source state data, in case an app wants to query it on source detected.
            controller.UpdateInputSource(interactionSourceState);
            if (MixedRealityManager.Instance.ActiveProfile.EnableInputSystem) inputSystem?.RaiseSourceDetected(controller.InputSource);
        }

        private void InteractionSourceUpdated(InteractionSourceState state)
        {
            var controller = GetOrAddWindowsGamingController(state);

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
            var controller = GetOrAddWindowsGamingController(state);
            if (controller == null) { return; }

            var inputAction = PressInteractionSource(pressType, controller);

            if (MixedRealityManager.Instance.ActiveProfile.EnableInputSystem) inputSystem?.RaiseOnInputDown(controller.InputSource, controller.ControllerHandedness, inputAction);
        }

        private void InteractionSourceReleased(InteractionSourceState state, InteractionSourcePressType pressType)
        {
            var controller = GetOrAddWindowsGamingController(state);
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
        #endregion Windows Gaming

        #region Unity InteractionManager Events
#if WINDOWS_UWP
        private void OnGamepadDetected(object sender, Gamepad gamepad)
        {
            //InteractionSourceDetected(args.state);
        }

        private void OnGamepadLost(object sender, Gamepad gamepad)
        {
            //RemoveWindowsGamingController(args.state);
        }

        private void OnArcadeStickDetected(object sender, ArcadeStick arcadeStick)
        {
            //InteractionSourceDetected(args.state);
        }

        private void OnArcadeStickLost(object sender, ArcadeStick arcadeStick)
        {
            //RemoveWindowsGamingController(args.state);
        }

        private void OnFlightStickDetected(object sender, FlightStick flightStick)
        {
        }

        private void OnFlightStickLost(object sender, FlightStick flightStick)
        {
        }

        private void OnRacingWheelDetected(object sender, RacingWheel racingWheel)
        {
        }

        private void OnRacingWheelLost(object sender, RacingWheel racingWheel)
        {
        }
#endif //WINDOWS_UWP

        #endregion Unity InteractionManager Events

        #region Runtime

        //TODO - runtime needs more thought

        public void Enable()
        {
            InitializeSources();
        }

        public void Disable()
        {
#if WINDOWS_UWP
            Gamepad.GamepadAdded -= OnGamepadDetected;
            Gamepad.GamepadRemoved -= OnGamepadLost;

            ArcadeStick.ArcadeStickAdded -= OnArcadeStickDetected;
            ArcadeStick.ArcadeStickRemoved -= OnArcadeStickLost;

            FlightStick.FlightStickAdded -= OnFlightStickDetected;
            FlightStick.FlightStickRemoved -= OnFlightStickLost;

            RacingWheel.RacingWheelAdded -= OnRacingWheelDetected;
            RacingWheel.RacingWheelRemoved -= OnRacingWheelLost;
#endif //WINDOWS_UWP
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