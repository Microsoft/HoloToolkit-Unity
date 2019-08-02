﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using Microsoft.MixedReality.Toolkit.Physics;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Utilities.Editor.Solvers
{
    [CustomEditor(typeof(SurfaceMagnetism))]
    [CanEditMultipleObjects]
    public class SurfaceMagnetismInspector : SolverInspector
    {
        private SerializedProperty magneticSurfacesProperty;
        private SerializedProperty maxDistanceProperty;
        private SerializedProperty closestDistanceProperty;
        private SerializedProperty surfaceNormalOffsetProperty;
        private SerializedProperty surfaceRayOffsetProperty;
        private SerializedProperty raycastModeProperty;
        private SerializedProperty boxRaysPerEdgeProperty;
        private SerializedProperty orthographicBoxCastProperty;
        private SerializedProperty maximumNormalVarianceProperty;

        private SerializedProperty sphereSizeProperty;
        private SerializedProperty volumeCastSizeOverrideProperty;
        private SerializedProperty useLinkedAltScaleOverrideProperty;
        private SerializedProperty currentRaycastDirectionModeProperty;
        private SerializedProperty orientationModeProperty;
        private SerializedProperty orientationBlendProperty;
        private SerializedProperty debugEnabledProperty;

        private SurfaceMagnetism surfaceMagnetism;

        protected override void OnEnable()
        {
            base.OnEnable();

            magneticSurfacesProperty = serializedObject.FindProperty("magneticSurfaces");
            maxDistanceProperty = serializedObject.FindProperty("maxRaycastDistance");
            closestDistanceProperty = serializedObject.FindProperty("closestDistance");
            surfaceNormalOffsetProperty = serializedObject.FindProperty("surfaceNormalOffset");
            surfaceRayOffsetProperty = serializedObject.FindProperty("surfaceRayOffset");
            currentRaycastDirectionModeProperty = serializedObject.FindProperty("currentRaycastDirectionMode");
            raycastModeProperty = serializedObject.FindProperty("raycastMode");
            boxRaysPerEdgeProperty = serializedObject.FindProperty("boxRaysPerEdge");
            orthographicBoxCastProperty = serializedObject.FindProperty("orthographicBoxCast");
            maximumNormalVarianceProperty = serializedObject.FindProperty("maximumNormalVariance");
            sphereSizeProperty = serializedObject.FindProperty("sphereSize");
            volumeCastSizeOverrideProperty = serializedObject.FindProperty("volumeCastSizeOverride");
            useLinkedAltScaleOverrideProperty = serializedObject.FindProperty("useLinkedAltScaleOverride");
            currentRaycastDirectionModeProperty = serializedObject.FindProperty("currentRaycastDirectionMode");
            orientationModeProperty = serializedObject.FindProperty("orientationMode");
            orientationBlendProperty = serializedObject.FindProperty("orientationBlend");
            debugEnabledProperty = serializedObject.FindProperty("debugEnabled");

            surfaceMagnetism = target as SurfaceMagnetism;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            // General Properties
            EditorGUILayout.LabelField("General Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(surfaceNormalOffsetProperty);
            EditorGUILayout.PropertyField(surfaceRayOffsetProperty);
            EditorGUILayout.PropertyField(orientationModeProperty);
            EditorGUILayout.PropertyField(orientationBlendProperty);

            // Raycast properties
            EditorGUILayout.LabelField("Raycast Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(magneticSurfacesProperty, true);
            EditorGUILayout.PropertyField(closestDistanceProperty);
            EditorGUILayout.PropertyField(maxDistanceProperty);
            EditorGUILayout.PropertyField(currentRaycastDirectionModeProperty);
            EditorGUILayout.PropertyField(raycastModeProperty);

            // Draw properties dependent on type of raycast direction mode selected
            switch (raycastModeProperty.enumValueIndex)
            {
                case (int)SceneQueryType.BoxRaycast:
                    EditorGUILayout.PropertyField(boxRaysPerEdgeProperty);
                    EditorGUILayout.PropertyField(orthographicBoxCastProperty);
                    EditorGUILayout.PropertyField(maximumNormalVarianceProperty);
                    break;
                case (int)SceneQueryType.SphereCast:
                    EditorGUILayout.PropertyField(sphereSizeProperty);
                    break;
                case (int)SceneQueryType.SphereOverlap:
                    InspectorUIUtility.DrawWarning("SurfaceMagnetism does not support SphereOverlap raycast mode");
                    break;
            }

            if (raycastModeProperty.enumValueIndex != (int)SceneQueryType.SimpleRaycast &&
                raycastModeProperty.enumValueIndex != (int)SceneQueryType.SphereOverlap)
            {
                EditorGUILayout.PropertyField(volumeCastSizeOverrideProperty);
            }

            // Other properties
            EditorGUILayout.LabelField("Other Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(useLinkedAltScaleOverrideProperty);
            EditorGUILayout.PropertyField(debugEnabledProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
