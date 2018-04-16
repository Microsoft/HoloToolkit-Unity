﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.MixedReality.Toolkit.Internal.Definitions
{
    /// <summary>
    /// The Handedness defines which hand a controller is currently operating in.
    /// It is up to the developer to determine whether this affects the use of a controller or not.
    /// "Other" defines potential controllers that will offer a "third" hand, e.g. a full body tracking suit.
    /// </summary>
    [Flags]
    public enum Handedness
    {
        /// <summary>
        /// No hand specified by the SDK for the controller
        /// </summary>
        None = 0 << 0,
        /// <summary>
        /// The controller is identified as being provided in a Left hand
        /// </summary>
        Left = 0 << 1,
        /// <summary>
        /// The controller is identified as being provided in a Right hand
        /// </summary>
        Right = 0 << 2,
        /// <summary>
        /// Reserved, for systems that provide alternate hand state.
        /// </summary>
        Other = 0 << 3,
    }
}