﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

#if UNITY_WSA
using UnityEngine.XR.WSA.Input;
#endif

namespace Microsoft.MixedReality.Toolkit.InputSystem.Utilities
{
    /// <summary>
    /// Waits for a controller to be instantiated, then attaches itself to a specified element
    /// </summary>
    public class AttachToController : ControllerFinder
    {
        [SerializeField]
        private bool setChildrenInactiveWhenDetached = true;

        public bool SetChildrenInactiveWhenDetached
        {
            get { return setChildrenInactiveWhenDetached; }
            set { setChildrenInactiveWhenDetached = value; }
        }

        [SerializeField]
        protected Vector3 PositionOffset = Vector3.zero;

        [SerializeField]
        protected Vector3 RotationOffset = Vector3.zero;

        [SerializeField]
        protected Vector3 ScaleOffset = Vector3.one;

        [SerializeField]
        protected bool SetScaleOnAttach = false;

        public bool IsAttached { get; private set; }

        protected virtual void OnAttachToController() { }

        protected virtual void OnDetachFromController() { }

        protected override void OnEnable()
        {
            SetChildrenActive(false);

#if UNITY_WSA
            // Look if the controller has loaded.
            if (TryGetControllerModel((InteractionSourceHandedness)Handedness, out ControllerInfo))
            {
                AddControllerTransform(ControllerInfo);
            }

            OnControllerModelLoaded += AddControllerTransform;
            OnControllerModelUnloaded += RemoveControllerTransform;
#endif 
        }

        protected override void AddControllerTransform(MotionControllerInfo newController)
        {
#if UNITY_WSA
            if (!IsAttached && newController.Handedness == (InteractionSourceHandedness)Handedness)
            {
                base.AddControllerTransform(newController);

                SetChildrenActive(true);

                // Parent ourselves under the element and set our offsets
                transform.parent = ElementTransform;
                transform.localPosition = PositionOffset;
                transform.localEulerAngles = RotationOffset;

                if (SetScaleOnAttach)
                {
                    transform.localScale = ScaleOffset;
                }

                // Announce that we're attached
                OnAttachToController();

                IsAttached = true;
            }
#endif
        }

        protected override void RemoveControllerTransform(MotionControllerInfo oldController)
        {
#if UNITY_WSA
            if (IsAttached && oldController.Handedness == (InteractionSourceHandedness)Handedness)
            {
                base.RemoveControllerTransform(oldController);

                OnDetachFromController();

                transform.parent = null;

                SetChildrenActive(false);

                IsAttached = false;
            }
#endif
        }

        private void SetChildrenActive(bool isActive)
        {
            if (SetChildrenInactiveWhenDetached)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(isActive);
                }
            }
        }
    }
}