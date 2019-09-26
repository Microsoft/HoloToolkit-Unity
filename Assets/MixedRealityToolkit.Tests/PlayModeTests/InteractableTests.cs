﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if !WINDOWS_UWP
// When the .NET scripting backend is enabled and C# projects are built
// The assembly that this file is part of is still built for the player,
// even though the assembly itself is marked as a test assembly (this is not
// expected because test assemblies should not be included in player builds).
// Because the .NET backend is deprecated in 2018 and removed in 2019 and this
// issue will likely persist for 2018, this issue is worked around by wrapping all
// play mode tests in this check.

using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Microsoft.MixedReality.Toolkit.Tests
{
    public class InteractableTests : BasePlayModeTests
    {
        private const float ButtonPressAnimationDelay = 0.25f;
        private const float ButtonReleaseAnimationDelay = 0.25f;
        private const float EaseDelay = 0.25f;
        private const string DefaultInteractablePrefabAssetPath = "Assets/MixedRealityToolkit.Examples/Demos/UX/Interactables/Prefabs/Model_PushButton.prefab";
        private const string RadialSetPrefabAssetPath = "Assets/MixedRealityToolkit.SDK/Features/UX/Interactable/Prefabs/RadialSet.prefab";

        private readonly Color DefaultColor = Color.blue;
        private readonly Color FocusColor = Color.yellow;
        private readonly Color DisabledColor = Color.gray;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            TestUtilities.PlayspaceToOriginLookingForward();
        }

        /// <summary>
        /// Instantiates a push button prefab and uses simulated hand input to press it.
        /// </summary>
        [UnityTest]
        public IEnumerator TestHandInputOnPrefab()
        {
            // Load interactable prefab
            Interactable interactable;
            Transform translateTargetObject;

            InstantiatePressButtonPrefab(
                new Vector3(0.025f, 0.05f, 0.5f),
                new Vector3(-90f, 0f, 0f),
                out interactable,
                out translateTargetObject);

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });

            Vector3 targetStartPosition = translateTargetObject.localPosition;

            // Move the hand forward to intersect the interactable
            var inputSimulationService = PlayModeTestUtilities.GetInputSimulationService();
            int numSteps = 32;
            Vector3 p1 = new Vector3(0.0f, 0f, 0f);
            Vector3 p2 = new Vector3(0.05f, 0f, 0.51f);
            Vector3 p3 = new Vector3(0.0f, 0f, 0.0f);

            yield return PlayModeTestUtilities.ShowHand(Handedness.Right, inputSimulationService);
            yield return PlayModeTestUtilities.MoveHandFromTo(p1, p2, numSteps, ArticulatedHandPose.GestureId.Poke, Handedness.Right, inputSimulationService);

            yield return CheckButtonTranslation(targetStartPosition, translateTargetObject);

            // Move the hand back
            yield return PlayModeTestUtilities.MoveHandFromTo(p2, p3, numSteps, ArticulatedHandPose.GestureId.Poke, Handedness.Right, inputSimulationService);
            yield return PlayModeTestUtilities.HideHand(Handedness.Right, inputSimulationService);
            yield return new WaitForSeconds(ButtonReleaseAnimationDelay);

            Assert.True(wasClicked, "Interactable was not clicked.");
        }

        /// <summary>
        /// Instantiates a push button prefab and uses simulated global input events to press it.
        /// </summary>
        [UnityTest]
        public IEnumerator TestSelectGlobalInput()
        {
            // Face the camera in the opposite direction so we don't focus on button
            MixedRealityPlayspace.PerformTransformation(p =>
            {
                p.position = Vector3.zero;
                p.LookAt(Vector3.back);
            });

            // Load interactable prefab
            Interactable interactable;
            Transform translateTargetObject;

            // Place out of the way of any pointers
            InstantiatePressButtonPrefab(
                new Vector3(10f, 0.0f, 0.5f),
                new Vector3(-90f, 0f, 0f),
                out interactable,
                out translateTargetObject);

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });

            // Set interactable to global
            interactable.IsGlobal = true;

            Vector3 targetStartPosition = translateTargetObject.localPosition;

            yield return null;

            // Find an input source to associate with the input event (doesn't matter which one)
            IMixedRealityInputSource defaultInputSource = CoreServices.InputSystem.DetectedInputSources.FirstOrDefault();
            Assert.NotNull(defaultInputSource, "At least one input source must be present for this test to work.");

            // Add interactable as a global listener
            // This is only necessary if IsGlobal is being set manually. If it's set in the inspector, interactable will register itself in OnEnable automatically.
            CoreServices.InputSystem.PushModalInputHandler(interactable.gameObject);

            // Raise a select down input event, then wait for transition to take place
            CoreServices.InputSystem.RaiseOnInputDown(defaultInputSource, Handedness.None, interactable.InputAction);
            // Wait for at least one frame explicitly to ensure the input goes through
            yield return new WaitForFixedUpdate();

            yield return CheckButtonTranslation(targetStartPosition, translateTargetObject);

            // Raise a select up input event, then wait for transition to take place
            CoreServices.InputSystem.RaiseOnInputUp(defaultInputSource, Handedness.Right, interactable.InputAction);
            // Wait for at button release animation to finish
            yield return new WaitForSeconds(ButtonReleaseAnimationDelay);

            Assert.True(wasClicked, "Interactable was not clicked.");
            Assert.False(interactable.HasFocus, "Interactable had focus");

            // Remove as global listener
            CoreServices.InputSystem.PopModalInputHandler();
        }

        /// <summary>
        /// Assembles a push button from primitives and uses simulated hand input to press it.
        /// </summary>
        [UnityTest]
        public IEnumerator TestHandInputOnRuntimeAssembled()
        {
            // Load interactable
            Interactable interactable;
            Transform translateTargetObject;

            AssembleInteractableButton(
                out interactable,
                out translateTargetObject);

            interactable.transform.position = new Vector3(0.025f, 0.05f, 0.65f);
            interactable.transform.eulerAngles = new Vector3(-90f, 0f, 0f);

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });

            Vector3 targetStartPosition = translateTargetObject.transform.localPosition;

            yield return null;

            // Add a touchable and configure for touch events
            NearInteractionTouchable touchable = interactable.gameObject.AddComponent<NearInteractionTouchable>();
            touchable.EventsToReceive = TouchableEventType.Touch;
            touchable.SetBounds(Vector2.one);
            touchable.SetLocalForward(Vector3.up);
            touchable.SetLocalUp(Vector3.forward);
            touchable.SetLocalCenter(Vector3.up * 2.75f);

            // Add a touch handler and link touch started / touch completed events
            TouchHandler touchHandler = interactable.gameObject.AddComponent<TouchHandler>();
            touchHandler.OnTouchStarted.AddListener((HandTrackingInputEventData e) => interactable.SetInputDown());
            touchHandler.OnTouchCompleted.AddListener((HandTrackingInputEventData e) => interactable.SetInputUp());

            // Move the hand forward to intersect the interactable
            var inputSimulationService = PlayModeTestUtilities.GetInputSimulationService();
            int numSteps = 32;
            Vector3 p1 = new Vector3(0.0f, 0f, 0f);
            Vector3 p2 = new Vector3(0.05f, 0f, 0.51f);
            Vector3 p3 = new Vector3(0.0f, 0f, 0.0f);

            yield return PlayModeTestUtilities.ShowHand(Handedness.Right, inputSimulationService);
            yield return PlayModeTestUtilities.MoveHandFromTo(p1, p2, numSteps, ArticulatedHandPose.GestureId.Poke, Handedness.Right, inputSimulationService);

            yield return CheckButtonTranslation(targetStartPosition, translateTargetObject);

            // Move the hand back
            yield return PlayModeTestUtilities.MoveHandFromTo(p2, p3, numSteps, ArticulatedHandPose.GestureId.Poke, Handedness.Right, inputSimulationService);
            yield return PlayModeTestUtilities.HideHand(Handedness.Right, inputSimulationService);
            yield return new WaitForSeconds(ButtonReleaseAnimationDelay);

            Assert.True(wasClicked, "Interactable was not clicked.");
        }

        /// <summary>
        /// Assembles a push button from primitives and uses simulated input events to press it.
        /// </summary>
        [UnityTest]
        public IEnumerator TestInputActionSelectInput()
        {
            // Load interactable
            Interactable interactable;
            Transform translateTargetObject;

            AssembleInteractableButton(
                out interactable,
                out translateTargetObject);

            interactable.transform.position = new Vector3(0.0f, 0.0f, 0.5f);
            interactable.transform.eulerAngles = new Vector3(-90f, 0f, 0f);

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });

            Vector3 targetStartPosition = translateTargetObject.localPosition;

            yield return null;

            // Find an input source to associate with the input event (doesn't matter which one)
            IMixedRealityInputSource defaultInputSource = CoreServices.InputSystem.DetectedInputSources.FirstOrDefault();
            Assert.NotNull(defaultInputSource, "At least one input source must be present for this test to work.");

            // Raise an input down event, then wait for transition to take place
            CoreServices.InputSystem.RaiseOnInputDown(defaultInputSource, Handedness.None, interactable.InputAction);
            // Wait for at least one frame explicitly to ensure the input goes through
            yield return new WaitForFixedUpdate();

            yield return CheckButtonTranslation(targetStartPosition, translateTargetObject);

            // Raise an input up event, then wait for transition to take place
            CoreServices.InputSystem.RaiseOnInputUp(defaultInputSource, Handedness.None, interactable.InputAction);
            // Wait for at least one frame explicitly to ensure the input goes through
            yield return new WaitForSeconds(ButtonReleaseAnimationDelay);

            Assert.True(wasClicked, "Interactable was not clicked.");
            Assert.AreEqual(targetStartPosition, translateTargetObject.localPosition, "Transform target object was not translated back by action.");
        }

        /// <summary>
        /// Tests that radial buttons can be selected and deselected, and that a radial button
        /// set allows just one button to be selected at a time
        /// </summary>
        [UnityTest]
        public IEnumerator TestRadialSetPrefab()
        {
            var radialSet = InstantiateInteractableFromPath(Vector3.forward, Vector3.zero, RadialSetPrefabAssetPath);
            var firstRadialButton = radialSet.transform.Find("Radial (1)");
            var secondRadialButton = radialSet.transform.Find("Radial (2)");
            var thirdRadialButton = radialSet.transform.Find("Radial (3)");
            var testHand = new TestHand(Handedness.Right);
            yield return testHand.Show(Vector3.zero);

            Assert.IsTrue(firstRadialButton.GetComponent<Interactable>().IsToggled);
            Assert.IsFalse(secondRadialButton.GetComponent<Interactable>().IsToggled);
            Assert.IsFalse(thirdRadialButton.GetComponent<Interactable>().IsToggled);

            yield return testHand.Show(Vector3.zero);

            var aBitBack = Vector3.forward * -0.2f;
            yield return testHand.MoveTo(firstRadialButton.position);
            yield return testHand.Move(aBitBack);

            yield return testHand.MoveTo(secondRadialButton.transform.position);
            yield return testHand.Move(aBitBack);

            Assert.IsFalse(firstRadialButton.GetComponent<Interactable>().IsToggled);
            Assert.IsTrue(secondRadialButton.GetComponent<Interactable>().IsToggled);
            Assert.IsFalse(thirdRadialButton.GetComponent<Interactable>().IsToggled);
        }

        /// <summary>
        /// Instantiates a push button prefab and uses simulated input events to press it.
        /// </summary>
        [UnityTest]
        public IEnumerator TestInputActionMenuInput()
        {
            // Load interactable prefab
            Interactable interactable;
            Transform translateTargetObject;

            InstantiatePressButtonPrefab(
                new Vector3(0.0f, 0.0f, 0.5f),
                new Vector3(-90f, 0f, 0f),
                out interactable,
                out translateTargetObject);

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });
            bool wasPressed = false;
            bool wasReleased = false;
            var pressReceiver = interactable.AddReceiver<InteractableOnPressReceiver>();
            pressReceiver.OnPress.AddListener(() => { wasPressed = true; Debug.Log("pressReciever wasPressed true"); });
            pressReceiver.OnRelease.AddListener(() => { wasReleased = true; Debug.Log("pressReciever wasReleased true"); });
            Vector3 targetStartPosition = translateTargetObject.localPosition;

            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            // Find the menu action from the input system profile
            MixedRealityInputAction menuAction = CoreServices.InputSystem.InputSystemProfile.InputActionsProfile.InputActions.Where(m => m.Description == "Menu").FirstOrDefault();
            Assert.NotNull(menuAction.Description, "Couldn't find menu input action in input system profile.");

            // Set the interactable to respond to a 'menu' input action
            interactable.InputAction = menuAction;

            // Find an input source to associate with the input event (doesn't matter which one)
            IMixedRealityInputSource defaultInputSource = CoreServices.InputSystem.DetectedInputSources.FirstOrDefault();
            Assert.NotNull(defaultInputSource, "At least one input source must be present for this test to work.");
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            // Raise a menu down input event, then wait for transition to take place
            CoreServices.InputSystem.RaiseOnInputDown(defaultInputSource, Handedness.Right, menuAction);
            // Wait for at least one frame explicitly to ensure the input goes through
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            // Raise a menu up input event, then wait for transition to take place
            CoreServices.InputSystem.RaiseOnInputUp(defaultInputSource, Handedness.Right, menuAction);
            // Wait for at least one frame explicitly to ensure the input goes through
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();


            Assert.True(wasPressed, "interactable not pressed");
            Assert.True(wasReleased, "interactable not released");
            Assert.True(wasClicked, "Interactable was not clicked.");
        }
        /// <summary>
        /// Instantiates a push button prefab and uses simulated voice input events to press it.
        /// </summary>
        [UnityTest]
        public IEnumerator TestVoiceInputOnPrefab()
        {
            // Load interactable prefab
            Interactable interactable;
            Transform translateTargetObject;

            InstantiatePressButtonPrefab(
                new Vector3(0.0f, 0.0f, 0.5f),
                new Vector3(-90f, 0f, 0f),
                out interactable,
                out translateTargetObject);

            // Subscribe to interactable's on click so we know the click went through
            bool wasClicked = false;
            interactable.OnClick.AddListener(() => { wasClicked = true; });
            
            Vector3 targetStartPosition = translateTargetObject.localPosition;

            // Set up its voice command
            interactable.VoiceCommand = "Select";

            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();

            // Find an input source to associate with the input event (doesn't matter which one)
            IMixedRealityInputSource defaultInputSource = CoreServices.InputSystem.DetectedInputSources.FirstOrDefault();
            Assert.NotNull(defaultInputSource, "At least one input source must be present for this test to work.");

            // Raise a voice select input event, then wait for transition to take place
            SpeechCommands commands = new SpeechCommands("Select", KeyCode.None, interactable.InputAction);
            CoreServices.InputSystem.RaiseSpeechCommandRecognized(defaultInputSource, RecognitionConfidenceLevel.High, new System.TimeSpan(100), System.DateTime.Now, commands);
            // Wait for at least one frame explicitly to ensure the input goes through
            yield return PlayModeTestUtilities.WaitForInputSystemUpdate();


            Assert.True(wasClicked, "Interactable was not clicked.");
        }

        /// <summary>
        /// Instantiates a runtime assembled Interactable and set Interactable state to disabled (not disabling the GameObject/component)
        /// </summary>
        [UnityTest]
        public IEnumerator TestDisableState()
        {
            // Load interactable
            Interactable interactable;
            Transform translateTargetObject;

            AssembleInteractableButton(
                out interactable,
                out translateTargetObject);

            CameraCache.Main.transform.LookAt(interactable.transform.position);

            yield return new WaitForSeconds(EaseDelay);
            var propBlock = InteractableThemeShaderUtils.GetPropertyBlock(translateTargetObject.gameObject);
            Assert.AreEqual(propBlock.GetColor("_Color"), FocusColor);

            interactable.Enabled = false;

            yield return new WaitForSeconds(EaseDelay);
            propBlock = InteractableThemeShaderUtils.GetPropertyBlock(translateTargetObject.gameObject);
            Assert.AreEqual(propBlock.GetColor("_Color"), DisabledColor);
            Assert.AreEqual(interactable.IsDisabled, true);
        }

        /// <summary>
        /// Instantiates a runtime assembled Interactable and destroy the Interactable component
        /// </summary>
        [UnityTest]
        public IEnumerator TestDestroy()
        {
            // Load interactable
            Interactable interactable;
            Transform translateTargetObject;

            AssembleInteractableButton(
                out interactable,
                out translateTargetObject);

            // Put GGV focus on the Interactable button
            CameraCache.Main.transform.LookAt(interactable.transform.position);

            yield return new WaitForSeconds(EaseDelay);
            var propBlock = InteractableThemeShaderUtils.GetPropertyBlock(translateTargetObject.gameObject);
            Assert.AreEqual(propBlock.GetColor("_Color"), FocusColor);

            // Destroy the interactable component
            GameObject.Destroy(interactable);

            // Remove focus
            CameraCache.Main.transform.LookAt(Vector3.zero);

            yield return null;
            propBlock = InteractableThemeShaderUtils.GetPropertyBlock(translateTargetObject.gameObject);
            Assert.AreEqual(propBlock.GetColor("_Color"), FocusColor);
        }

        #region Test Helpers

        /// <summary>
        /// Generates an interactable from primitives and assigns a select action.
        /// </summary>
        private void AssembleInteractableButton(out Interactable interactable, out Transform translateTargetObject, string selectActionDescription = "Select")
        {
            // Assemble an interactable out of a set of primitives
            // This will be the button housing
            var interactableObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            interactableObject.name = "RuntimeInteractable";
            interactableObject.transform.position = new Vector3(0.05f, 0.05f, 0.625f);
            interactableObject.transform.localScale = new Vector3(0.15f, 0.025f, 0.15f);
            interactableObject.transform.eulerAngles = new Vector3(90f, 0f, 180f);

            // This will be the part that gets scaled
            GameObject childObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            var renderer = childObject.GetComponent<Renderer>();
            renderer.material.color = DefaultColor;
            renderer.material.shader = StandardShaderUtility.MrtkStandardShader;

            childObject.transform.parent = interactableObject.transform;
            childObject.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            childObject.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            childObject.transform.localRotation = Quaternion.identity;
            // Only use a collider on the main object
            GameObject.Destroy(childObject.GetComponent<Collider>());

            translateTargetObject = childObject.transform;

            // Add an interactable
            interactable = interactableObject.AddComponent<Interactable>();

            var themeDefinition = ThemeDefinition.GetDefaultThemeDefinition<ScaleOffsetColorTheme>().Value;
            // themeDefinition.Easing.Enabled = false;
            // Set the offset state property (index = 1) to move on the Pressed state (index = 2)
            themeDefinition.StateProperties[1].Values = new List<ThemePropertyValue>()
            {
                new ThemePropertyValue() { Vector3 = Vector3.zero},
                new ThemePropertyValue() { Vector3 = Vector3.zero},
                new ThemePropertyValue() { Vector3 = new Vector3(0.0f, -0.32f, 0.0f)},
                new ThemePropertyValue() { Vector3 = Vector3.zero},
            };
            // Set the color state property (index = 2) values
            themeDefinition.StateProperties[2].Values = new List<ThemePropertyValue>()
            {
                new ThemePropertyValue() { Color = DefaultColor},
                new ThemePropertyValue() { Color = FocusColor},
                new ThemePropertyValue() { Color = Color.green},
                new ThemePropertyValue() { Color = DisabledColor},
            };

            Theme testTheme = ScriptableObject.CreateInstance<Theme>();
            testTheme.States = interactable.States;
            testTheme.Definitions = new List<ThemeDefinition>() { themeDefinition };

            interactable.Profiles = new List<InteractableProfileItem>()
            {
                new InteractableProfileItem()
                {
                    Themes = new List<Theme>() { testTheme },
                    Target = translateTargetObject.gameObject,
                },
            };

            // Set the interactable to respond to the requested input action
            MixedRealityInputAction selectAction = CoreServices.InputSystem.InputSystemProfile.InputActionsProfile.InputActions.Where(m => m.Description == selectActionDescription).FirstOrDefault();
            Assert.NotNull(selectAction.Description, "Couldn't find " + selectActionDescription + " input action in input system profile.");
            interactable.InputAction = selectAction;
        }

        private GameObject InstantiateInteractableFromPath(Vector3 position, Vector3 eulerAngles, string path)
        {
            // Load interactable prefab
            Object interactablePrefab = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            GameObject result = Object.Instantiate(interactablePrefab) as GameObject;
            Assert.IsNotNull(result);

            // Move the object into position
            result.transform.position = position;
            result.transform.eulerAngles = eulerAngles;
            return result;
        }

        /// <summary>
        /// Instantiates the default interactable button.
        /// </summary>
        private void InstantiatePressButtonPrefab(Vector3 position, Vector3 rotation, out Interactable interactable, out Transform pressButtonCylinder)
        {
            // Load interactable prefab
            var interactableObject = InstantiateInteractableFromPath(position, rotation, DefaultInteractablePrefabAssetPath);
            interactable = interactableObject.GetComponent<Interactable>();
            Assert.IsNotNull(interactable);

            // Find the target object for the interactable transformation
            pressButtonCylinder = interactableObject.transform.Find("Cylinder");
            Assert.IsNotNull(pressButtonCylinder, "Object 'Cylinder' could not be found under example object Model_PushButton.");

            // Move the object into position
            interactableObject.transform.position = position;
            interactableObject.transform.eulerAngles = rotation;
        }

        private IEnumerator CheckButtonTranslation(Vector3 targetStartPosition, Transform translateTarget)
        {
            bool wasTranslated = false;
            float pressEndTime = Time.time + ButtonPressAnimationDelay;
            while (Time.time < pressEndTime)
            {   // If the transform is moved at any point during this interval, we were successful
                yield return new WaitForFixedUpdate();
                wasTranslated |= targetStartPosition != translateTarget.localPosition;
            }

            Assert.True(wasTranslated, "Transform target object was not translated by action.");
        }

        #endregion
    }
}
#endif
