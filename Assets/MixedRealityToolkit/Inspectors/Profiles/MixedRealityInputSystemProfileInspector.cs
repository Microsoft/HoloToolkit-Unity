﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using UnityEditor;
using Microsoft.MixedReality.Toolkit.Editor;

namespace Microsoft.MixedReality.Toolkit.Input.Editor
{
    [CustomEditor(typeof(MixedRealityInputSystemProfile))]
    public class MixedRealityInputSystemProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
    {
        private static bool showFocusProperties = true;
        private SerializedProperty focusProviderType;

        private static bool showPointerProperties = true;
        private SerializedProperty pointerProfile;

        private static bool showActionsProperties = true;
        private SerializedProperty inputActionsProfile;
        private SerializedProperty inputActionRulesProfile;

        private static bool showControllerProperties = true;
        private SerializedProperty enableControllerMapping;
        private SerializedProperty controllerMappingProfile;
        private SerializedProperty controllerVisualizationProfile;

        private static bool showGestureProperties = true;
        private SerializedProperty gesturesProfile;

        private static bool showSpeechCommandsProperties = true;
        private SerializedProperty speechCommandsProfile;

        private static bool showHandTrackingProperties = true;
        private SerializedProperty handTrackingProfile;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!MixedRealityInspectorUtility.CheckMixedRealityConfigured(false))
            {
                return;
            }

            focusProviderType = serializedObject.FindProperty("focusProviderType");
            inputActionsProfile = serializedObject.FindProperty("inputActionsProfile");
            inputActionRulesProfile = serializedObject.FindProperty("inputActionRulesProfile");
            pointerProfile = serializedObject.FindProperty("pointerProfile");
            gesturesProfile = serializedObject.FindProperty("gesturesProfile");
            speechCommandsProfile = serializedObject.FindProperty("speechCommandsProfile");
            controllerMappingProfile = serializedObject.FindProperty("controllerMappingProfile");
            enableControllerMapping = serializedObject.FindProperty("enableControllerMapping");
            controllerVisualizationProfile = serializedObject.FindProperty("controllerVisualizationProfile");
            handTrackingProfile = serializedObject.FindProperty("handTrackingProfile");
        }

        public override void OnInspectorGUI()
        {
            RenderMixedRealityToolkitLogo();
            if (!MixedRealityInspectorUtility.CheckMixedRealityConfigured())
            {
                return;
            }

            if (DrawBacktrackProfileButton("Back to Configuration Profile", MixedRealityToolkit.Instance.ActiveProfile))
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Input System Profile", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("The Input System Profile helps developers configure input no matter what platform you're building for.", MessageType.Info);

            CheckProfileLock(target);

            var previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 160f;

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            bool changed = false;

            EditorGUILayout.Space();
            showFocusProperties = EditorGUILayout.Foldout(showFocusProperties, "Focus Settings", true);
            if (showFocusProperties)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(focusProviderType);
                }
            }

            EditorGUILayout.Space();
            showPointerProperties = EditorGUILayout.Foldout(showPointerProperties, "Pointer Settings", true);
            if (showPointerProperties)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    changed |= RenderProfile(pointerProfile);
                }
            }

            EditorGUILayout.Space();
            showActionsProperties = EditorGUILayout.Foldout(showActionsProperties, "Action Settings", true);
            if (showActionsProperties)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    changed |= RenderProfile(inputActionsProfile);
                    changed |= RenderProfile(inputActionRulesProfile);
                }
            }

            EditorGUILayout.Space();
            showControllerProperties = EditorGUILayout.Foldout(showControllerProperties, "Controller Settings", true);
            if (showControllerProperties)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(enableControllerMapping);
                    changed |= RenderProfile(controllerMappingProfile);
                    changed |= RenderProfile(controllerVisualizationProfile, true, typeof(IMixedRealityControllerVisualizer));
                }
            }

            EditorGUILayout.Space();
            showGestureProperties = EditorGUILayout.Foldout(showGestureProperties, "Gesture Settings", true);
            if (showGestureProperties)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    changed |= RenderProfile(gesturesProfile);
                }
            }

            EditorGUILayout.Space();
            showSpeechCommandsProperties = EditorGUILayout.Foldout(showSpeechCommandsProperties, "Speech Command Settings", true);
            if (showSpeechCommandsProperties)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    changed |= RenderProfile(speechCommandsProfile);
                }
            }

            EditorGUILayout.Space();
            showHandTrackingProperties = EditorGUILayout.Foldout(showHandTrackingProperties, "Hand Tracking Settings", true);
            if (showHandTrackingProperties)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    changed |= RenderProfile(handTrackingProfile);
                }
            }

            if (!changed)
            {
                changed |= EditorGUI.EndChangeCheck();
            }

            EditorGUIUtility.labelWidth = previousLabelWidth;
            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                EditorApplication.delayCall += () => MixedRealityToolkit.Instance.ResetConfiguration(MixedRealityToolkit.Instance.ActiveProfile);
            }
        }
    }
}