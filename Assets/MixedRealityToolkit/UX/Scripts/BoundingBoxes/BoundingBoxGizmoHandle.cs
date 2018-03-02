﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MixedRealityToolkit.InputModule.EventData;
using MixedRealityToolkit.InputModule.InputHandlers;
using UnityEngine;

#if UNITY_WSA && UNITY_2017_2_OR_NEWER
using UnityEngine.XR.WSA;
#endif

namespace MixedRealityToolkit.UX.BoundingBoxes
{
    public class BoundingBoxGizmoHandle : MonoBehaviour, IInputHandler, ISourceStateHandler
    {
        public enum TransformType
        {
            Rotation,
            Scale
        };
        public enum AxisToAffect
        {
            X,
            Y,
            Z
        };
        public BoundingBoxRig Rig;

        private Transform transformToAffect;
        private TransformType affineType;
        private AxisToAffect axis;
        private Vector3 initialHandPosition;
        private Vector3 initialScale;
        private Vector3 initialPosition;
        private Vector3 initialOrientation;
        private Quaternion initialHandOrientation;
        private Vector3 initialScaleOrigin;
        private InputEventData inputDownEventData;
        private bool isHandRotationAvailable;
        private bool isLeftHandedRotation = false;
        private Vector3 rotationFromPositionScale = new Vector3(-300.0f, -300.0f, -300.0f);
        private float minimumScaleNav = 0.001f;
        
        public TransformType AffineType
        {
            get
            {
                return affineType;
            }

            set
            {
                affineType = value;
            }
        }
        public AxisToAffect Axis
        {
            get
            {
                return axis;
            }

            set
            {
                axis = value;
            }
        }
        public bool IsLeftHandedRotation
        {
            get
            {
                return isLeftHandedRotation;
            }

            set
            {
                isLeftHandedRotation = value;
            }
        }
        public Transform TransformToAffect
        {
            get
            {
                return transformToAffect;
            }

            set
            {
                transformToAffect = value;
            }
        }

        private void Start()
        {
            isHandRotationAvailable = true;

#if UNITY_WSA && UNITY_2017_2_OR_NEWER
            if (HolographicSettings.IsDisplayOpaque == false)
            {
                isHandRotationAvailable = false;
            }
#endif
        }
        private void Update()
        {
            if (inputDownEventData != null)
            {
                Vector3 currentHandPosition = Vector3.zero;
                Quaternion currentHandOrientation = Quaternion.identity;

                //set values from hand
                currentHandPosition = GetHandPosition(inputDownEventData.SourceId);
                if (isHandRotationAvailable)
                {
                    currentHandOrientation = GetHandOrientation(inputDownEventData.SourceId);
                }

                //calculate affines
                if (this.AffineType == TransformType.Scale)
                {
                    ApplyScale(currentHandPosition);
                }
                else if (this.AffineType == TransformType.Rotation)
                {
                    if (isHandRotationAvailable)
                    {
                        ApplyRotation(currentHandOrientation);
                    }
                    else
                    {
                        ApplyRotation(currentHandPosition);
                    }
                }
            }
        }

        private Vector3 GetHandPosition(uint sourceId)
        {
            Vector3 handPosition = new Vector3(0, 0, 0);
            inputDownEventData.InputSource.TryGetGripPosition(sourceId, out handPosition);
            return handPosition;
        }
        private Quaternion GetHandOrientation(uint sourceId)
        {
            Quaternion handOrientation = Quaternion.identity;
            inputDownEventData.InputSource.TryGetGripRotation(sourceId, out handOrientation);
            return handOrientation;
        }
        private void ApplyScale(Vector3 currentHandPosition)
        {
            if ((transformToAffect.position - initialHandPosition).magnitude > minimumScaleNav)
            {
                float scaleScalar = (currentHandPosition - transformToAffect.position).magnitude / (transformToAffect.position - initialHandPosition).magnitude;
                Vector3 newScale = new Vector3(scaleScalar, scaleScalar, scaleScalar);
                newScale.Scale(initialScale);

                //scale from object center
                transformToAffect.localScale = newScale;

                //now handle offset
                Vector3 currentScaleOrigin = initialScaleOrigin;
                currentScaleOrigin.Scale(new Vector3(scaleScalar, scaleScalar, scaleScalar));
                Vector3 postScaleOffset = currentScaleOrigin - initialScaleOrigin;

                //translate so that scale is effectively from opposite corner
                transformToAffect.position = initialPosition - postScaleOffset;
            }
        }
        private void ApplyRotation(Quaternion currentHandOrientation)
        {
            Matrix4x4 m = Matrix4x4.Rotate(initialHandOrientation);
            Vector3 initRay = new Vector3(0, 0, 1);
            initRay = m.MultiplyPoint(initRay);
            initRay.Normalize();

            m  = Matrix4x4.Rotate(currentHandOrientation);
            Vector3 currentRay = new Vector3(0, 0, 1);
            currentRay = m.MultiplyPoint(currentRay);
            currentRay.Normalize();

            float dot = Vector3.Dot(initRay, currentRay);
            dot = Mathf.Acos(dot) * Mathf.Rad2Deg;

            if (Mathf.Abs(initRay.y - currentRay.y) < Mathf.Abs(initRay.x - currentRay.x))
            {
                if (Vector3.Cross(initRay, currentRay).y > 0)
                {
                    dot = -dot;
                }
            }
            else
            {
                if (Vector3.Cross(initRay, currentRay).x > 0)
                {
                    dot = -dot;
                }
            }

            Vector3 newEulers = new Vector3(0,0,0);
            if (Axis == AxisToAffect.X)
            {
                newEulers = new Vector3(dot, 0, 0);
            }
            else if (Axis == AxisToAffect.Y)
            {
                newEulers = new Vector3(0, dot, 0);
            }
            else if (Axis == AxisToAffect.Z)
            {
                newEulers = new Vector3(0, 0, dot);
            }
            newEulers += initialOrientation;


            transformToAffect.rotation = Quaternion.Euler(newEulers);
        }
        private void ApplyRotation(Vector3 currentHandPosition)
        {
            Vector3 initialRay = initialHandPosition - transformToAffect.position;
            initialRay.Normalize();

            Vector3 currentRay = currentHandPosition - transformToAffect.position;
            currentRay.Normalize();

            Vector3 delta = currentRay - initialRay;
            delta.Scale(rotationFromPositionScale);

            Vector3 newEulers = new Vector3(0, 0, 0);
            if (Axis == AxisToAffect.X)
            {
                newEulers = new Vector3(-delta.y, 0, 0);
            }
            else if (Axis == AxisToAffect.Y)
            {
                newEulers = new Vector3(0, delta.x, 0);
            }
            else if (Axis == AxisToAffect.Z)
            {
                newEulers = new Vector3(0, 0, delta.y);
            }

            if (IsLeftHandedRotation)
            {
                newEulers.Scale(new Vector3(-1.0f, -1.0f, -1.0f));
            }

            newEulers += initialOrientation;

            transformToAffect.rotation = Quaternion.Euler(newEulers);
        }

        public void OnInputDown(InputEventData eventData)
        {
            inputDownEventData = eventData;

            initialHandPosition     = GetHandPosition(eventData.SourceId);
            initialScale            = transformToAffect.localScale;
            initialPosition         = transformToAffect.position;
            initialOrientation      = transformToAffect.rotation.eulerAngles;
            initialHandOrientation  = GetHandOrientation(eventData.SourceId);
            initialScaleOrigin      = transformToAffect.position - this.transform.position;

            MixedRealityToolkit.InputModule.InputManager.Instance.PushModalInputHandler(gameObject);

            this.gameObject.GetComponent<Renderer>().material = Rig.InteractingMaterial;
            Rig.FocusOnHandle(this.gameObject);
            eventData.Use();
        }
        public void OnInputUp(InputEventData eventData)
        {
            inputDownEventData = null;

            if (this.AffineType == TransformType.Scale)
            {
                this.gameObject.GetComponent<Renderer>().material = Rig.ScaleHandleMaterial;
            }
            else
            {
                this.gameObject.GetComponent<Renderer>().material = Rig.RotateHandleMaterial;
            }

            MixedRealityToolkit.InputModule.InputManager.Instance.PopModalInputHandler();
            Rig.FocusOnHandle(null);

            if (eventData != null)
            {
                eventData.Use();
            }
        }
        public void OnSourceDetected(SourceStateEventData eventData)
        {
        }
        public void OnSourceLost(SourceStateEventData eventData)
        {
            if (eventData.SourceId == inputDownEventData.SourceId)
            {
                OnInputUp(null);
            }
            eventData.Use();
        }
    }
}










