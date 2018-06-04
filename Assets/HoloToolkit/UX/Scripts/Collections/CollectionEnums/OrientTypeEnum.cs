﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace HoloToolkit.Unity.Collections
{
    /// <summary>
    /// Orientation type enum for collections
    /// </summary>
    public enum OrientTypeEnum
    {
        None,                   // Don't rotate at all
        FaceOrigin,             // Rotate towards the origin
        FaceOriginReversed,     // Rotate towards the origin + 180 degrees
        FaceParentFoward,       // Parent Relative Forwards, this used to be called FaceForward
        FaceParentBack,         // Parent Relative Backwards, this used to be called FaceForwardReversed
        [Obsolete("Please use FaceParentFoward")]
        FaceFoward = FaceParentFoward,             // Zero rotation
        [Obsolete("Please use FaceParentBack")]
        FaceForwardReversed = FaceParentBack,    // Zero rotation + 180 degrees
        FaceParentUp,           // Parent Relative Up
        FaceParentDown,         // Parent Relative Down
		FaceCenterAxis,         // Lay flat on the surface, facing in
        FaceCenterAxisReversed, // Lay flat on the surface, facing out

    }
}
