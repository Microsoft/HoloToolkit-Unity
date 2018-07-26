﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Internal.EventDatum.Input;
using Microsoft.MixedReality.Toolkit.Internal.Extensions;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem.Handlers;
using Microsoft.MixedReality.Toolkit.Internal.Managers;
using Microsoft.MixedReality.Toolkit.Internal.Utilities;
using Microsoft.MixedReality.Toolkit.SDK.Input;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX.Controllers
{
    /// <summary>
    /// The Mixed Reality Visualization component is primarily responsible for rendering the users controllers in a scene.
    /// It will ensure whether you have a specific model configured against the controller definition and fall back to using the generic default models defined in the Controllers Profile.
    /// The visualizer can be enabled / disabled on demand (after startup) 
    /// </summary>
    /// <example>
    /// To Start, ensure there is a "Controllers Profile" configured against the main Mixed Reality Configuration Profile and that Controller Rendering is enabled in that profile, as well as configuring at least one controller for each platform you intend to support.
    /// Once ready, then simply add this script to a GameObject in the scene, for example the Camera / head.
    /// </example>
    /// <remarks>
    /// For Alpha, this visualizer only does basic rendering and management of controllers.  In future versions, it will expose specific transforms on a controller model (if it has them) to be able to attach items to any part of the model
    /// It will also support animation of controller parts and possibly fading in and our of controllers.
    /// </remarks>
    /// <seealso cref="MixedRealityControllerMappingProfile"/>
    public class MixedRealityControllerVisualizer : InputSystemGlobalListener, IMixedRealitySourcePoseHandler, IMixedRealityInputHandler
    {

        #region Private Properties

        private static GameObject leftControllerModel;
        private static IMixedRealityController leftController;
        private static MixedRealityPose leftControllerOffsetPose = MixedRealityPose.ZeroIdentity;
        private static GameObject rightControllerModel;
        private static IMixedRealityController rightController;
        private static MixedRealityPose rightControllerOffsetPose = MixedRealityPose.ZeroIdentity;

        #endregion Private Properties

        #region SourcePose Event Handlers

        /// <summary>
        /// The selected controller is moving in the scene, update it's pose in relation to the users movements
        /// </summary>
        /// <param name="eventData"></param>
        public void OnSourcePoseChanged(SourcePoseEventData eventData)
        {
            //Handles.PositionHandle(eventData.MixedRealityPose.Position, eventData.MixedRealityPose.Rotation);

            // Update the respective controller if it has been initialized
            switch (eventData.Controller.ControllerHandedness)
            {
                case Handedness.Left:
                    if (leftControllerModel != null)
                    {
                        leftControllerModel.transform.localPosition = eventData.MixedRealityPose.Position + leftControllerOffsetPose.Position;
                        leftControllerModel.transform.localRotation = eventData.MixedRealityPose.Rotation * leftControllerOffsetPose.Rotation;
                    }
                    break;
                case Handedness.Right:
                    if (rightControllerModel != null)
                    {
                        rightControllerModel.transform.localPosition = eventData.MixedRealityPose.Position + rightControllerOffsetPose.Position;
                        rightControllerModel.transform.localRotation = eventData.MixedRealityPose.Rotation * rightControllerOffsetPose.Rotation;
                    }
                    break;
            }
        }

        /// <summary>
        /// Controller found, create the configured model and position it in the scene
        /// </summary>
        /// <param name="eventData"></param>
        public void OnSourceDetected(SourceStateEventData eventData)
        {
            // Capture the respective controller when it's detected.  However, It'll only be rendered if visualization is enabled.
            if (eventData.Controller != null )
            {
                if (eventData.Controller.ControllerHandedness == Handedness.Right)
                {
                    rightController = eventData.Controller;
                }
                else
                {
                    leftController = eventData.Controller;
                }
                CreateControllerInstance(eventData.Controller.ControllerHandedness);
            }
        }

        /// <summary>
        /// Controller removed, remove it from the scene
        /// </summary>
        /// <param name="eventData"></param>
        public void OnSourceLost(SourceStateEventData eventData)
        {
            // Clean up when a controller is disconnected
            if (eventData.Controller != null)
            {
                DestroyControllerInstance(eventData.Controller.ControllerHandedness);
            }
        }

        #endregion SourcePose Event Handlers

        #region Input Handlers

        /// <summary>
        /// Visualize the pressed button in the controller model, if supported
        /// </summary>
        /// <remarks>
        /// Reserved for future implementation
        /// </remarks>
        /// <param name="eventData"></param>
        public void OnInputDown(InputEventData eventData)
        {
            //Visualize button down
        }


        /// <summary>
        /// Visualize the released button in the controller model, if supported
        /// </summary>
        /// <remarks>
        /// Reserved for future implementation
        /// </remarks>
        /// <param name="eventData"></param>
        public void OnInputUp(InputEventData eventData)
        {
            //Visualize button up
        }


        /// <summary>
        /// Visualize the held trigger in the controller model, if supported
        /// </summary>
        /// <remarks>
        /// Reserved for future implementation
        /// </remarks>
        /// <param name="eventData"></param>
        public void OnInputPressed(InputEventData<float> eventData)
        {
            //Visualize single axis controls
        }


        /// <summary>
        /// Visualize the movement of a dual axis input in the controller model, if supported
        /// </summary>
        /// <remarks>
        /// Reserved for future implementation
        /// </remarks>
        /// <param name="eventData"></param>
        public void OnPositionInputChanged(InputEventData<Vector2> eventData)
        {
            //Visualize dual axis controls
        }

        #endregion Input Handlers

        #region Enable / Disable

        /// <summary>
        /// When the visualizer is enabled, create any attached controllers in the scene.
        /// </summary>
        /// <remarks>
        /// Visualizers must start enabled in the scene in order to capture the controllers from the platform.
        /// </remarks>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (MixedRealityManager.Instance.ActiveProfile.ControllersProfile != null && MixedRealityManager.Instance.ActiveProfile.ControllersProfile.RenderMotionControllers)
            {
                CreateControllerInstance(Handedness.Left);
                CreateControllerInstance(Handedness.Right);
            }
        }

        /// <summary>
        /// If the visualizer is disabled, ensure all rendered controllers are cleaned up
        /// </summary>
        protected override void OnDisable()
        {
            DestroyControllerInstance(Handedness.Left);
            DestroyControllerInstance(Handedness.Right);

            base.OnDisable();
        }

        #endregion Enable / Disable

        #region Controller Visualization Functions

        /// <summary>
        /// Get the currently configured model for the controller
        /// </summary>
        /// <remarks>
        /// Controller is detected in the following order:
        /// 1: The controller model attached to the specific controller type's configuration profile
        /// 2: The generic controller model attached to the main controller configuration profile
        /// </remarks>
        /// <param name="sourceController"></param>
        /// <param name="controllerModel"></param>
        /// <returns>Returns true if a controller model is found and outputs the GameObject definition.  if no controller is found, the response is false</returns>
        public bool TryGetControllerModel(IMixedRealityController sourceController, out GameObject controllerModel, out MixedRealityPose poseOffset)
        {
            controllerModel = null;
            poseOffset = MixedRealityPose.ZeroIdentity;

            //Try and get the controller model from the specific COntroller definition
            MixedRealityManager.Instance.ActiveProfile.ControllersProfile?.MixedRealityControllerMappingProfiles.GetControllerModelOverride(sourceController.GetType(), sourceController.ControllerHandedness, out controllerModel, out poseOffset);
            if (controllerModel != null)
            {
                return true;
            }

            //If no specific controller model found for the device type, try and get the generic override models from the main Controllers profile
            if (sourceController.ControllerHandedness == Handedness.Left && MixedRealityManager.Instance.ActiveProfile.ControllersProfile?.GlobalLeftHandModel != null)
            {
                controllerModel = MixedRealityManager.Instance.ActiveProfile.ControllersProfile.GlobalLeftHandModel;
                poseOffset = MixedRealityManager.Instance.ActiveProfile.ControllersProfile.LeftHandModelPoseOffset;
                return true;
            }

            if (sourceController.ControllerHandedness == Handedness.Right && MixedRealityManager.Instance.ActiveProfile.ControllersProfile?.GlobalRightHandModel != null)
            {
                controllerModel = MixedRealityManager.Instance.ActiveProfile.ControllersProfile.GlobalRightHandModel;
                poseOffset = MixedRealityManager.Instance.ActiveProfile.ControllersProfile.RightHandModelPoseOffset;
                return true;
            }

            //No model found, give up, go home and bake cookies.  Nothing to see here.
            controllerModel = null;
            return false;
        }

        private void CreateControllerInstance(Handedness controllingHand)
        {
            GameObject controllerModelGameObject = null;
            MixedRealityPose poseOffset = MixedRealityPose.ZeroIdentity;

            switch (controllingHand)
            {
                case Handedness.Left:
                    if (leftController != null && TryGetControllerModel(leftController, out controllerModelGameObject, out poseOffset))
                    {
                        leftControllerModel = Instantiate(controllerModelGameObject, CameraCache.Main.transform.parent);
                        leftControllerOffsetPose = poseOffset;
                    }
                    break;
                case Handedness.Right:
                    if (rightController != null && TryGetControllerModel(rightController, out controllerModelGameObject, out poseOffset))
                    {
                        rightControllerModel = Instantiate(controllerModelGameObject, CameraCache.Main.transform.parent);
                        rightControllerOffsetPose = poseOffset;
                    }
                    break;
            }
        }

        private void DestroyControllerInstance(Handedness controllingHand)
        {
            switch (controllingHand)
            {
                case Handedness.Left:
                    Destroy(leftControllerModel);
                    leftControllerModel = null;
                    break;
                case Handedness.Right:
                    Destroy(rightControllerModel);
                    rightControllerModel = null;
                    break;
            }
        }

        #endregion Controller Visualization Functions

    }
}