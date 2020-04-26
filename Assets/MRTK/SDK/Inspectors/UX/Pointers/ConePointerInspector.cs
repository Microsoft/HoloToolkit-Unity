﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using Microsoft.MixedReality.Toolkit.Input.Editor;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEditor;

namespace Microsoft.MixedReality.Toolkit.Input
{
    [CustomEditor(typeof(ConePointer))]
    public class ConePointerInspector : BaseControllerPointerInspector
    {
        private SerializedProperty coneTipAngle;
        private SerializedProperty sphereCastRadius;
        private SerializedProperty nearObjectMargin;
        private SerializedProperty grabLayerMasks;
        private SerializedProperty triggerInteraction;
        private SerializedProperty sceneQueryBufferSize;
        private SerializedProperty ignoreCollidersNotInFOV;


        private bool spherePointerFoldout = true;

        protected override void OnEnable()
        {
            base.OnEnable();

            coneTipAngle = serializedObject.FindProperty("coneTipAngle");
            sphereCastRadius = serializedObject.FindProperty("sphereCastRadius");
            sceneQueryBufferSize = serializedObject.FindProperty("sceneQueryBufferSize");
            nearObjectMargin = serializedObject.FindProperty("nearObjectMargin");
            grabLayerMasks = serializedObject.FindProperty("grabLayerMasks");
            triggerInteraction = serializedObject.FindProperty("triggerInteraction");
            ignoreCollidersNotInFOV = serializedObject.FindProperty("ignoreCollidersNotInFOV");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            spherePointerFoldout = EditorGUILayout.Foldout(spherePointerFoldout, "Sphere Pointer Settings", true);

            if (spherePointerFoldout)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(coneTipAngle);
                    EditorGUILayout.PropertyField(sphereCastRadius);
                    EditorGUILayout.PropertyField(sceneQueryBufferSize);
                    EditorGUILayout.PropertyField(nearObjectMargin);
                    EditorGUILayout.PropertyField(triggerInteraction);
                    EditorGUILayout.PropertyField(grabLayerMasks, true);
                    EditorGUILayout.PropertyField(ignoreCollidersNotInFOV);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}