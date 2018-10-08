﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// Theme scriptableObject for loading theme settings
    /// </summary>
    
    [CreateAssetMenu(fileName = "Theme", menuName = "Interactable/Theme", order = 1)]
    public class Theme : ScriptableObject
    {
        public string Name;
        public List<ThemePropertySettings> Settings;
        public List<ThemePropertyValue> CustomSettings;
        public States States;

        public State[] GetStates()
        {
            if (States != null)
            {
                return States.GetStates();
            }

            return new State[0];
        }
    }
}
