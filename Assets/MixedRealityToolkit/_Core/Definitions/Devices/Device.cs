﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions.Utilities;
using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Definitions.Devices
{
    /// <summary>
    /// Used to define a controller or other input device's physical buttons, and other attributes.
    /// </summary>
    [Serializable]
    public struct Device
    {
        public Device(uint id, string description, SDKType sdkType, Handedness handedness, InteractionMapping[] interactions) : this()
        {
            this.id = id;
            this.description = description;
            this.sdkType = sdkType;
            this.handedness = handedness;
            this.interactions = interactions;
        }

        /// <summary>
        /// The ID assigned to the Device.
        /// </summary>
        public uint Id => id;

        [SerializeField]
        private uint id;

        /// <summary>
        /// Description of the Device.
        /// </summary>
        public string Description => description;

        [SerializeField]
        private string description;

        /// <summary>
        /// The XR SDKs that is supported by this Device.
        /// </summary>
        public SDKType SDKType => sdkType;

        [SerializeField]
        [Tooltip("The XR SDKs that is supported by this Device.")]
        private SDKType sdkType;

        /// <summary>
        /// The designated hand that the device is managing.
        /// </summary>
        public Handedness Handedness => handedness;

        [SerializeField]
        [Tooltip("The designated hand that the device is managing.")]
        private Handedness handedness;

        /// <summary>
        /// Details the list of available buttons / interactions available from the device.
        /// </summary>
        public InteractionMapping[] Interactions => interactions;

        [SerializeField]
        [Tooltip("Details the list of available buttons / interactions available from the device.")]
        private InteractionMapping[] interactions;
    }
}