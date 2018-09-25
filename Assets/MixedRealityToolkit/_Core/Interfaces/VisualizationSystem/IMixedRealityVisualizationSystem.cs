﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Interfaces.Devices;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Events;
using System.Collections.Generic;

namespace Microsoft.MixedReality.Toolkit.Core.Interfaces.VisualizationSystem
{
    /// <summary>
    /// Manager interface for the Visualization system in the Mixed Reality Toolkit
    /// All replacement systems for providing Visualization functionality should derive from this interface
    /// </summary>
    public interface IMixedRealityVisualizationSystem : IMixedRealityEventSystem
    {

        /// <summary>
        /// List of <see cref="IMixedRealityController"/>s currently detected by the input manager.
        /// </summary>
        /// <remarks>
        /// This property is similar to <see cref="InputSystem.IMixedRealityInputSystem.DetectedInputSources"/>, as this is a subset of those <see cref="InputSystem.IMixedRealityInputSource"/>s in that list.
        /// </remarks>
        HashSet<IMixedRealityVisualizer> DetectedVisualizers { get; }

        /// <summary>
        /// Manager registration function for controllers, to add a new controller to present in the scene.
        /// </summary>
        /// <param name="controller">Tracked controller to visualize</param>
        IMixedRealityVisualizer RegisterVisualizerForController(IMixedRealityController controller);
    }
}