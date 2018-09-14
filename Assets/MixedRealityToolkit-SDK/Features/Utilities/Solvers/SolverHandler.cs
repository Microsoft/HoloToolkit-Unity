// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Core.Utilities;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Devices;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.Utilities.Solvers
{
    /// <summary>
    /// This class handles the solver components that are attached to this <see cref="GameObject"/>
    /// </summary>
    [DisallowMultipleComponent]
    public class SolverHandler : MonoBehaviour
    {
        private IMixedRealityControllerPoseSynchronizer poseSynchronizer;

        /// <summary>
        /// Tracked object to calculate position and orientation from. If you want to manually override and use a scene object, use the TransformTarget field.
        /// </summary>
        public TrackedObjectType TrackedObjectType
        {
            get
            {
                if (poseSynchronizer == null)
                {
                    return transformTarget == CameraCache.Main.transform
                        ? TrackedObjectType.Head
                        : TrackedObjectType.Other;
                }

                return poseSynchronizer.Handedness == Handedness.Left
                    ? TrackedObjectType.MotionControllerLeft
                    : TrackedObjectType.MotionControllerRight;
            }
        }

        [SerializeField]
        [Tooltip("Add an additional offset of the tracked object to base the solver on. Useful for tracking something like a halo position above your head or off the side of a controller.")]
        private Vector3 additionalOffset;

        /// <summary>
        /// Add an additional offset of the tracked object to base the solver on. Useful for tracking something like a halo position above your head or off the side of a controller.
        /// </summary>
        public Vector3 AdditionalOffset
        {
            get { return additionalOffset; }
            set
            {
                additionalOffset = value;
                transformTarget = MakeOffsetTransform(transformTarget);
            }
        }

        [SerializeField]
        [Tooltip("Add an additional rotation on top of the tracked object. Useful for tracking what is essentially the up or right/left vectors.")]
        private Vector3 additionalRotation;

        /// <summary>
        /// Add an additional rotation on top of the tracked object. Useful for tracking what is essentially the up or right/left vectors.
        /// </summary>
        public Vector3 AdditionalRotation
        {
            get { return additionalRotation; }
            set
            {
                additionalRotation = value;
                transformTarget = MakeOffsetTransform(transformTarget);
            }
        }

        [SerializeField]
        [Tooltip("Manual override for TrackedObjectToReference if you want to use a scene object. Leave empty if you want to use head or motion-tracked controllers.")]
        private Transform transformTarget;

        /// <summary>
        /// The target transform that the solvers will act upon.
        /// </summary>
        public Transform TransformTarget
        {
            get { return transformTarget; }
            set
            {
                transformTarget = value;
                TrackTransform(transformTarget != null ? transformTarget : transform);
            }
        }

        [SerializeField]
        [Tooltip("Whether or not this SolverHandler calls SolverUpdate() every frame. Only one SolverHandler should manage SolverUpdate(). This setting does not affect whether the Target Transform of this SolverHandler gets updated or not.")]
        private bool updateSolvers = true;

        /// <summary>
        /// Whether or not this SolverHandler calls SolverUpdate() every frame. Only one SolverHandler should manage SolverUpdate(). This setting does not affect whether the Target Transform of this SolverHandler gets updated or not.
        /// </summary>
        public bool UpdateSolvers
        {
            get { return updateSolvers; }
            set { updateSolvers = value; }
        }

        /// <summary>
        /// The position the solver is trying to move to.
        /// </summary>
        public Vector3 GoalPosition { get; set; }

        /// <summary>
        /// The rotation the solver is trying to rotate to.
        /// </summary>
        public Quaternion GoalRotation { get; set; }

        /// <summary>
        /// The scale the solver is trying to scale to.
        /// </summary>
        public Vector3 GoalScale { get; set; }

        /// <summary>
        /// Alternate scale.
        /// </summary>
        public Vector3Smoothed AltScale { get; set; }

        /// <summary>
        /// The timestamp the solvers will use to calculate with.
        /// </summary>
        public float DeltaTime { get; set; }

        private bool RequiresOffset => !AdditionalOffset.sqrMagnitude.Equals(0) || AdditionalRotation.sqrMagnitude.Equals(0);

        protected readonly List<Solver> Solvers = new List<Solver>();

        private float lastUpdateTime;
        private GameObject transformWithOffset;

        #region MonoBehaviour Implementation

        private void Awake()
        {
            GoalScale = Vector3.one;
            AltScale = new Vector3Smoothed(Vector3.one, 0.1f);
            DeltaTime = 0.0f;

            Solvers.AddRange(GetComponents<Solver>());

            poseSynchronizer = GetComponent<IMixedRealityControllerPoseSynchronizer>();

            if (poseSynchronizer != null)
            {
                TransformTarget = transform;
            }
        }

        private void Update()
        {
            DeltaTime = Time.realtimeSinceStartup - lastUpdateTime;
            lastUpdateTime = Time.realtimeSinceStartup;
        }

        private void LateUpdate()
        {
            if (UpdateSolvers)
            {
                for (int i = 0; i < Solvers.Count; ++i)
                {
                    Solver solver = Solvers[i];

                    if (solver.enabled)
                    {
                        solver.SolverUpdate();
                    }
                }
            }
        }

        protected void OnDestroy()
        {
            if (transformWithOffset != null)
            {
                Destroy(transformWithOffset);
            }
        }

        #endregion MonoBehaviour Implementation

        private void TrackTransform(Transform newTrackedTransform)
        {
            TransformTarget = RequiresOffset ? MakeOffsetTransform(newTrackedTransform) : newTrackedTransform;
        }

        private Transform MakeOffsetTransform(Transform parentTransform)
        {
            if (transformWithOffset == null)
            {
                transformWithOffset = new GameObject();
                transformWithOffset.transform.parent = parentTransform;
            }

            transformWithOffset.transform.localPosition = AdditionalOffset;
            transformWithOffset.transform.localRotation = Quaternion.Euler(AdditionalRotation);
            transformWithOffset.name = $"{gameObject.name} on {TrackedObjectType.ToString()} with offset {AdditionalOffset}, {AdditionalRotation}";
            return transformWithOffset.transform;
        }
    }
}