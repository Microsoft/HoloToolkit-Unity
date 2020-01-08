﻿using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Utilities.Editor
{
    public static class HandednessInspectorGUI
    {
        private static readonly GUIContent[] HandednessSelections =
        {
            new GUIContent("Left Hand"),
            new GUIContent("Right Hand"),
            new GUIContent("Any Hand"),
        };

        /// <summary>
        /// Responsible for drawing the ControllerHandedness as a dropdown. Resets to None in case it was set to something other than left, right or both
        /// </summary>
        /// <param name="controllerHandedness">The SerializedProperty for the the controllerHandedness</param>
        public static void DrawControllerHandednessDropdown(SerializedProperty controllerHandedness)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                using (var c = new EditorGUI.ChangeCheckScope())
                {
                    var handednessValue = controllerHandedness.intValue - 1;

                    // Reset in case it was set to something other than left, right or both.
                    if (handednessValue < 0 || handednessValue > 2) { handednessValue = 0; }

                    handednessValue = EditorGUILayout.IntPopup(new GUIContent(controllerHandedness.displayName, controllerHandedness.tooltip), handednessValue, HandednessSelections, null);

                    if (c.changed)
                    {
                        controllerHandedness.intValue = handednessValue + 1;
                    }
                }
            }
        }
    }
}
