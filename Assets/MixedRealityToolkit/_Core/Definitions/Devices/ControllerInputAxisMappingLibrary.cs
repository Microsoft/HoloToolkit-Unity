﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Internal.Devices.OpenVR;
using System.Collections.Generic;

namespace Microsoft.MixedReality.Toolkit.Internal.Definitions.Devices
{
    /// <summary>
    /// Helper utility to manage all the required Axis configuration for platforms, where required
    /// </summary>
    public static class ControllerInputAxisMappingLibrary
    {
        #region Controller axis mapping configuration

        //Axis and Input mapping configuration for each controller type.  Centralized here... Because Unity..

        #region OpenVR

        private static readonly string[] OpenVRInputMappings =
        {
            "OPENVR_TOUCHPAD_LEFT_CONTROLLER_HORIZONTAL",   // 0 - TOUCHPAD_LEFT_CONTROLLER_HORIZONTAL
            "OPENVR_TOUCHPAD_LEFT_CONTROLLER_VERTICAL",     // 1 - TOUCHPAD_LEFT_CONTROLLER_VERTICAL
            "OPENVR_TOUCHPAD_RIGHT_CONTROLLER_HORIZONTAL",  // 2 - TOUCHPAD_RIGHT_CONTROLLER_HORIZONTAL
            "OPENVR_TOUCHPAD_RIGHT_CONTROLLER_VERTICAL",    // 3 - TOUCHPAD_RIGHT_CONTROLLER_VERTICAL
            "OPENVR_TOUCHPAD_LEFT_CONTROLLER_HORIZONTAL",   // 4 - THUMBSTICK_LEFT_CONTROLLER_HORIZONTAL
            "OPENVR_TOUCHPAD_LEFT_CONTROLLER_VERTICAL",     // 5 - THUMBSTICK_LEFT_CONTROLLER_VERTICAL
            "OPENVR_TOUCHPAD_RIGHT_CONTROLLER_HORIZONTAL",  // 6 - THUMBSTICK_RIGHT_CONTROLLER_HORIZONTAL
            "OPENVR_TOUCHPAD_RIGHT_CONTROLLER_VERTICAL",    // 7 - THUMBSTICK_RIGHT_CONTROLLER_VERTICAL
            "OPENVR_TRIGGER_LEFT_CONTROLLER",               // 8 - TRIGGER_LEFT_CONTROLLER
            "OPENVR_TRIGGER_RIGHT_CONTROLLER",              // 9 - TRIGGER_RIGHT_CONTROLLER
            "OPENVR_GRIP_LEFT_CONTROLLER",                  // 10 - GRIP_LEFT_CONTROLLER
            "OPENVR_GRIP_RIGHT_CONTROLLER"                  // 11 - GRIP_RIGHT_CONTROLLER
        };

        private static readonly InputManagerAxis[] OpenVRControllerAxisMappings =
        {
            new InputManagerAxis { Name = OpenVRInputMappings[0], Dead = 0.1f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 1 },
            new InputManagerAxis { Name = OpenVRInputMappings[1], Dead = 0.1f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 2 },
            new InputManagerAxis { Name = OpenVRInputMappings[2], Dead = 0.1f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 4 },
            new InputManagerAxis { Name = OpenVRInputMappings[3], Dead = 0.1f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 5 },
            new InputManagerAxis { Name = OpenVRInputMappings[4], Dead = 0.1f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 1 },
            new InputManagerAxis { Name = OpenVRInputMappings[5], Dead = 0.1f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 2 },
            new InputManagerAxis { Name = OpenVRInputMappings[6], Dead = 0.1f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 4 },
            new InputManagerAxis { Name = OpenVRInputMappings[7], Dead = 0.1f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 5 },
            new InputManagerAxis { Name = OpenVRInputMappings[8], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 9 },
            new InputManagerAxis { Name = OpenVRInputMappings[9], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 10 },
            new InputManagerAxis { Name = OpenVRInputMappings[10], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 11 },
            new InputManagerAxis { Name = OpenVRInputMappings[11], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 12 }
        };

        #endregion OpenVR

        #region OculusTouch

        private static readonly string[] OculusTouchInputMappings =
        {
            "OTOUCH_TOUCHPAD_LEFT_CONTROLLER_HORIZONTAL",       // 0 - TOUCHPAD_LEFT_CONTROLLER_HORIZONTAL
            "OTOUCH_TOUCHPAD_LEFT_CONTROLLER_VERTICAL",         // 1 - TOUCHPAD_LEFT_CONTROLLER_VERTICAL
            "OTOUCH_TOUCHPAD_RIGHT_CONTROLLER_HORIZONTAL",      // 2 - TOUCHPAD_RIGHT_CONTROLLER_HORIZONTAL
            "OTOUCH_TOUCHPAD_RIGHT_CONTROLLER_VERTICAL",        // 3 - TOUCHPAD_RIGHT_CONTROLLER_VERTICAL
            "OTOUCH_THUMBSTICK_LEFT_CONTROLLER_HORIZONTAL",     // 4 - THUMBSTICK_LEFT_CONTROLLER_HORIZONTAL
            "OTOUCH_THUMBSTICK_LEFT_CONTROLLER_VERTICAL",       // 5 - THUMBSTICK_LEFT_CONTROLLER_VERTICAL
            "OTOUCH_THUMBSTICK_RIGHT_CONTROLLER_HORIZONTAL",    // 6 - THUMBSTICK_RIGHT_CONTROLLER_HORIZONTAL
            "OTOUCH_THUMBSTICK_RIGHT_CONTROLLER_VERTICAL",      // 7 - THUMBSTICK_RIGHT_CONTROLLER_VERTICAL
            "OTOUCH_TRIGGER_LEFT_CONTROLLER",                   // 8 - TRIGGER_LEFT_CONTROLLER
            "OTOUCH_TRIGGER_RIGHT_CONTROLLER",                  // 9 - TRIGGER_RIGHT_CONTROLLER
            "OTOUCH_GRIP_LEFT_CONTROLLER",                      // 10 - GRIP_LEFT_CONTROLLER
            "OTOUCH_GRIP_RIGHT_CONTROLLER",                     // 11 - GRIP_RIGHT_CONTROLLER
            "OTOUCH_THUMBSTICK_NEARTOUCH_LEFT_CONTROLLER",      // 12 - THUMBSTICK_NEARTOUCH_LEFT_CONTROLLER
            "OTOUCH_THUMBSTICK_NEARTOUCH_RIGHT_CONTROLLER",     // 13 - THUMBSTICK_NEARTOUCH_RIGHT_CONTROLLER
            "OTOUCH_TRIGGER_NEARTOUCH_LEFT_CONTROLLER",         // 14 - TRIGGER_NEARTOUCH_LEFT_CONTROLLER
            "OTOUCH_TRIGGER_NEARTOUCH_RIGHT_CONTROLLER",        // 15 - TRIGGER_NEARTOUCH_RIGHT_CONTROLLER
            "OTOUCH_THUMBREST_NEARTOUCH_LEFT_CONTROLLER",       // 16 - THUMBREST_NEARTOUCH_LEFT_CONTROLLER
            "OTOUCH_THUMBREST_NEARTOUCH_RIGHT_CONTROLLER"        // 17 - THUMBREST_EARTOUCH_RIGHT_CONTROLLER
        };

        private static readonly InputManagerAxis[] OculusTouchControllerAxisMappings =
        {
            new InputManagerAxis { Name = OculusTouchInputMappings[4], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 1 },
            new InputManagerAxis { Name = OculusTouchInputMappings[5], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 2 },
            new InputManagerAxis { Name = OculusTouchInputMappings[6], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 4 },
            new InputManagerAxis { Name = OculusTouchInputMappings[7], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 5 },
            new InputManagerAxis { Name = OculusTouchInputMappings[8], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 9 },
            new InputManagerAxis { Name = OculusTouchInputMappings[9], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 10 },
            new InputManagerAxis { Name = OculusTouchInputMappings[10], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 11 },
            new InputManagerAxis { Name = OculusTouchInputMappings[11], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 12 },
            new InputManagerAxis { Name = OculusTouchInputMappings[12], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 15 },
            new InputManagerAxis { Name = OculusTouchInputMappings[13], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 16 },
            new InputManagerAxis { Name = OculusTouchInputMappings[14], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 13 },
            new InputManagerAxis { Name = OculusTouchInputMappings[15], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 14 },
            new InputManagerAxis { Name = OculusTouchInputMappings[16], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 17 },
            new InputManagerAxis { Name = OculusTouchInputMappings[17], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 18 }
        };

        #endregion

        #region ViveWand

        private static readonly string[] ViveWandInputMappings =
        {
            "VIVE_TOUCHPAD_LEFT_CONTROLLER_HORIZONTAL",   // 0 - TOUCHPAD_LEFT_CONTROLLER_HORIZONTAL
            "VIVE_TOUCHPAD_LEFT_CONTROLLER_VERTICAL",     // 1 - TOUCHPAD_LEFT_CONTROLLER_VERTICAL
            "VIVE_TOUCHPAD_RIGHT_CONTROLLER_HORIZONTAL",  // 2 - TOUCHPAD_RIGHT_CONTROLLER_HORIZONTAL
            "VIVE_TOUCHPAD_RIGHT_CONTROLLER_VERTICAL",    // 3 - TOUCHPAD_RIGHT_CONTROLLER_VERTICAL
            "VIVE_TOUCHPAD_LEFT_CONTROLLER_HORIZONTAL",   // 4 - THUMBSTICK_LEFT_CONTROLLER_HORIZONTAL
            "VIVE_TOUCHPAD_LEFT_CONTROLLER_VERTICAL",     // 5 - THUMBSTICK_LEFT_CONTROLLER_VERTICAL
            "VIVE_TOUCHPAD_RIGHT_CONTROLLER_HORIZONTAL",  // 6 - THUMBSTICK_RIGHT_CONTROLLER_HORIZONTAL
            "VIVE_TOUCHPAD_RIGHT_CONTROLLER_VERTICAL",    // 7 - THUMBSTICK_RIGHT_CONTROLLER_VERTICAL
            "VIVE_TRIGGER_LEFT_CONTROLLER",               // 8 - TRIGGER_LEFT_CONTROLLER
            "VIVE_TRIGGER_RIGHT_CONTROLLER",              // 9 - TRIGGER_RIGHT_CONTROLLER
            "VIVE_GRIP_LEFT_CONTROLLER",                  // 10 - GRIP_LEFT_CONTROLLER
            "VIVE_GRIP_RIGHT_CONTROLLER"                  // 11 - GRIP_RIGHT_CONTROLLER
        };

        private static readonly InputManagerAxis[] HtcViveControllerAxisMappings =
        {
            new InputManagerAxis { Name = ViveWandInputMappings[0], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 1 },
            new InputManagerAxis { Name = ViveWandInputMappings[1], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 2 },
            new InputManagerAxis { Name = ViveWandInputMappings[2], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 4 },
            new InputManagerAxis { Name = ViveWandInputMappings[3], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 5 },
            new InputManagerAxis { Name = ViveWandInputMappings[8], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 9 },
            new InputManagerAxis { Name = ViveWandInputMappings[9], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 10 },
            new InputManagerAxis { Name = ViveWandInputMappings[10], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 11 },
            new InputManagerAxis { Name = ViveWandInputMappings[11], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 12 }
        };

        #endregion ViveWand

        #region ValveKnuckles

        private static readonly string[] ValveKnucklesInputMappings =
        {
            "VKNUCKLES_TOUCHPAD_LEFT_CONTROLLER_HORIZONTAL",   // 0 - TOUCHPAD_LEFT_CONTROLLER_HORIZONTAL
            "VKNUCKLES_TOUCHPAD_LEFT_CONTROLLER_VERTICAL",     // 1 - TOUCHPAD_LEFT_CONTROLLER_VERTICAL
            "VKNUCKLES_TOUCHPAD_RIGHT_CONTROLLER_HORIZONTAL",  // 2 - TOUCHPAD_RIGHT_CONTROLLER_HORIZONTAL
            "VKNUCKLES_TOUCHPAD_RIGHT_CONTROLLER_VERTICAL",    // 3 - TOUCHPAD_RIGHT_CONTROLLER_VERTICAL
            "VKNUCKLES_TOUCHPAD_LEFT_CONTROLLER_HORIZONTAL",   // 4 - THUMBSTICK_LEFT_CONTROLLER_HORIZONTAL
            "VKNUCKLES_TOUCHPAD_LEFT_CONTROLLER_VERTICAL",     // 5 - THUMBSTICK_LEFT_CONTROLLER_VERTICAL
            "VKNUCKLES_TOUCHPAD_RIGHT_CONTROLLER_HORIZONTAL",  // 6 - THUMBSTICK_RIGHT_CONTROLLER_HORIZONTAL
            "VKNUCKLES_TOUCHPAD_RIGHT_CONTROLLER_VERTICAL",    // 7 - THUMBSTICK_RIGHT_CONTROLLER_VERTICAL
            "VKNUCKLES_TRIGGER_LEFT_CONTROLLER",               // 8 - TRIGGER_LEFT_CONTROLLER
            "VKNUCKLES_TRIGGER_RIGHT_CONTROLLER",              // 9 - TRIGGER_RIGHT_CONTROLLER
            "VKNUCKLES_GRIP_LEFT_CONTROLLER",                  // 10 - GRIP_LEFT_CONTROLLER
            "VKNUCKLES_GRIP_RIGHT_CONTROLLER",                 // 11 - GRIP_RIGHT_CONTROLLER
            "VKNUCKLES_INDEXFINGER_LEFT_CONTROLLER",           // 12 - INDEXFINGER_LEFT_CONTROLLER
            "VKNUCKLES_INDEXFINGER_RIGHT_CONTROLLER",          // 13 - INDEXFINGER_RIGHT_CONTROLLER
            "VKNUCKLES_MIDDLEFINGER_LEFT_CONTROLLER",          // 14 - MIDDLEFINGER_LEFT_CONTROLLER
            "VKNUCKLES_MIDDLEFINGER_RIGHT_CONTROLLER",         // 15 - MIDDLEFINGER_RIGHT_CONTROLLER
            "VKNUCKLES_RINGFINGER_LEFT_CONTROLLER",            // 16 - RINGFINGER_LEFT_CONTROLLER
            "VKNUCKLES_RINGFINGER_RIGHT_CONTROLLER",           // 17 - RINGFINGER_RIGHT_CONTROLLER
            "VKNUCKLES_PINKYFINGER_LEFT_CONTROLLER",           // 18 - PINKYFINGER_LEFT_CONTROLLER
            "VKNUCKLES_PINKYFINGER_RIGHT_CONTROLLER"           // 19 - PINKYFINGER_RIGHT_CONTROLLER
        };

        private static readonly InputManagerAxis[] ValveKnucklesControllerAxisMappings =
        {
            new InputManagerAxis { Name = ValveKnucklesInputMappings[0], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 1 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[1], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 2 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[2], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 4 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[3], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 5 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[8], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 9 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[9], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 10 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[10], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 11 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[11], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 12 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[12], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 20 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[13], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 21 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[14], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 22 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[15], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 23 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[16], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 24 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[17], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 25 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[18], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 26 },
            new InputManagerAxis { Name = ValveKnucklesInputMappings[19], Dead = 0.001f, Sensitivity = 1, Invert = false, Type = InputManagerAxisType.JoystickAxis, Axis = 27 }
        };

        #endregion ValveKnuckles

        #endregion Controller axis mapping configuration

        #region Controller axis library

        /// <summary>
        /// Collection of the above Controller mapping configuration, used to load the Unity Input Manager axis mappings.
        /// </summary>
        private static readonly Dictionary<string, InputManagerAxis[]> InputManagerAxis = new Dictionary<string, InputManagerAxis[]>
        {
            { typeof(GenericOpenVRController).FullName, OpenVRControllerAxisMappings },
            { typeof(ViveWandController).FullName, HtcViveControllerAxisMappings },
            { typeof(OculusTouchController).FullName, OculusTouchControllerAxisMappings },
            { typeof(ValveKnucklesController).FullName, ValveKnucklesControllerAxisMappings }
        };

        /// <summary>
        /// Collection of the above Controller axis mapping strings, used by controllers to query the correct axis name
        /// </summary>
        private static readonly Dictionary<string, string[]> InputManagerMappings = new Dictionary<string, string[]>
        {
            { typeof(GenericOpenVRController).FullName, OpenVRInputMappings },
            { typeof(ViveWandController).FullName, ViveWandInputMappings },
            { typeof(OculusTouchController).FullName, OculusTouchInputMappings },
            { typeof(ValveKnucklesController).FullName, ValveKnucklesInputMappings }
        };

        #endregion Controller axis library

        /// <summary>
        /// Get the InputManagerAxis data needed to configure the Input Mappings for a controller
        /// </summary>
        /// <param name="type">The type of controller to retrieve configuration for</param>
        /// <returns></returns>
        public static InputManagerAxis[] GetInputManagerAxes(string type)
        {
            return InputManagerAxis.ContainsKey(type) ? InputManagerAxis[type] : default(InputManagerAxis[]);
        }

        /// <summary>
        /// Get the Input Manager string Mappings for a specific controller type
        /// </summary>
        /// <param name="type">The type of controller to retrieve configuration for</param>
        /// <returns></returns>
        public static string[] GetInputManagerMappings(string type)
        {
            return InputManagerMappings.ContainsKey(type) ? InputManagerMappings[type] : default(string[]);
        }
    }
}