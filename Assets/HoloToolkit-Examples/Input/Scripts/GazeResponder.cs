﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HoloToolkit.Unity.InputModule.Tests
{
    /// <summary>
    /// This class implements IFocusHandler to respond to gaze changes.
    /// It highlights the object being gazed at.
    /// </summary>
    public class GazeResponder : FocusTarget
    {
        private Material[] defaultMaterials;

        private void Start()
        {
            defaultMaterials = GetComponent<Renderer>().materials;
        }

        private void OnDestroy()
        {
            foreach (var material in defaultMaterials)
            {
                Destroy(material);
            }
        }

        public override void OnFocusEnter(FocusEventData eventData)
        {
            base.OnFocusEnter(eventData);

            for (int i = 0; i < defaultMaterials.Length; i++)
            {
                // Highlight the material when gaze enters using the shader property.
                defaultMaterials[i].SetFloat("_Gloss", 10.0f);
            }
        }

        public override void OnFocusExit(FocusEventData eventData)
        {
            base.OnFocusExit(eventData);

            for (int i = 0; i < defaultMaterials.Length; i++)
            {
                // Remove highlight on material when gaze exits.
                defaultMaterials[i].SetFloat("_Gloss", 1.0f);
            }
        }
    }
}