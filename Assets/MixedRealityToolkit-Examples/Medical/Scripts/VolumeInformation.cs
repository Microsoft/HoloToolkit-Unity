﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MixedRealityToolkit.Common;
using UnityEngine;

namespace MixedRealityToolkit
{
    /// <summary>
    /// Holds data about a volume asset
    /// </summary>
    [CreateAssetMenu]
    public class VolumeInformation : ScriptableObject
    {
        [OpenLocalFolder] public string ImageSourceFolder;
        [HideInInspector] public Texture3D BakedTexture;

        public bool InferAlpha = true;

        public bool AutoSizeOnBake = true;
        public Int3 Size;
    }
}