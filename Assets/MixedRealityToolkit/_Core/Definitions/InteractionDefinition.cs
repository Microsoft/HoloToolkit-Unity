﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Definitions
{
    /// <summary>
    /// A InteractionDefinition maps the capabilities of controllers, one definition should exist for each interaction profile.<para/>
    /// <remarks>Interactions can be any input the controller supports such as buttons, triggers, joysticks, dpads, and more.</remarks>
    /// </summary>
    public struct InteractionDefinition
    {
        public InteractionDefinition(uint id, AxisType axisType, InputType inputType) : this()
        {
            Id = id;
            AxisType = axisType;
            InputType = inputType;
        }

        #region Interaction Properties

        /// <summary>
        /// The Id assigned to the Interaction.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// The axis type of the button, e.g. Analogue, Digital, etc.
        /// </summary>
        public AxisType AxisType { get; }

        /// <summary>
        /// The primary action of the input as defined by the controller SDK.
        /// </summary>
        public InputType InputType { get; }

        /// <summary>
        /// Has the value changed since the last reading.
        /// </summary>
        public bool Changed { get; private set; }

        #endregion Interaction Properties

        #region Definition Data items

        /// <summary>
        /// The data storage for a Raw / None Axis type.
        /// </summary>
        private object RawData { get; set; }

        /// <summary>
        /// The data storage for a Digital Axis type.
        /// </summary>
        private bool BoolData { get; set; }

        /// <summary>
        /// The data storage for a Single Axis type.
        /// </summary>
        private float FloatData { get; set; }

        /// <summary>
        /// The data storage for a Dual Axis type.
        /// </summary>
        private Vector2 Vector2Data { get; set; }

        /// <summary>
        /// The position data storage for a 3DoF or 6DoF Axis type.
        /// </summary>
        private Vector3 PositionData { get; set; }

        /// <summary>
        /// The rotation data storage for a 3DoF or 6DoF Axis type.
        /// </summary>
        private Quaternion RotationData { get; set; }

        private Tuple<Vector3, Quaternion> TransformData { get; set; }

        #endregion Definition Data items

        #region Get Operators

        public object GetRaw()
        {
            return RawData;
        }

        public bool GetBool()
        {
            return BoolData;
        }

        public float GetFloat()
        {
            return FloatData;
        }

        public Vector2 GetVector2()
        {
            return Vector2Data;
        }

        public Vector3 GetPosition()
        {
            return PositionData;
        }

        public Quaternion GetRotation()
        {
            return RotationData;
        }

        public Tuple<Vector3, Quaternion> GetTransform()
        {
            return TransformData;
        }

        #endregion Get Operators

        #region Set Operators

        public void SetValue(object newValue)
        {
            if (AxisType == AxisType.Raw)
            {
                Changed = newValue == RawData;
                RawData = newValue;
            }
        }

        public void SetValue(bool newValue)
        {
            if (AxisType == AxisType.Digital)
            {
                Changed = newValue == BoolData;
                BoolData = newValue;
            }
        }

        public void SetValue(float newValue)
        {
            if (AxisType == AxisType.SingleAxis)
            {
                Changed = newValue.Equals(FloatData);
                FloatData = newValue;
            }
        }

        public void SetValue(Vector2 newValue)
        {
            if (AxisType == AxisType.DualAxis)
            {
                Changed = newValue == Vector2Data;
                Vector2Data = newValue;
            }
        }

        public void SetValue(Vector3 newValue)
        {
            if (AxisType == AxisType.ThreeDoFPosition)
            {
                Changed = newValue == PositionData;
                PositionData = newValue;
            }
        }

        public void SetValue(Quaternion newValue)
        {
            if (AxisType == AxisType.ThreeDoFRotation)
            {
                Changed = newValue == RotationData;
                RotationData = newValue;
            }
        }

        public void SetValue(Tuple<Vector3, Quaternion> newValue)
        {
            if (AxisType == AxisType.SixDoF)
            {
                Changed = newValue.Item1 == PositionData && newValue.Item2 == RotationData;
                PositionData = newValue.Item1;
                RotationData = newValue.Item2;
            }
        }

        #endregion Set Operators
    }
}