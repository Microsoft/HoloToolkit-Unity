﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using MixedRealityToolkit.InputModule.Utilities.Interations;
using MixedRealityToolkit.UX.BoundingBoxes;
using MixedRealityToolkit.Examples.InputModule;
using MixedRealityToolkit.UX.Buttons;
using MixedRealityToolkit.UX.AppBarControl;
using UnityEngine;

namespace MixedRealityToolkit.UX.BoundingBoxes
{
    public class BoundingBoxRig : MonoBehaviour
    {
        [Header("Flattening")]
        [SerializeField]
        [Tooltip("Choose this option if Rig is to be applied to a 2D object.")]
        private BoundingBox.FlattenModeEnum flattenedAxis;

        [Header("Customization Settings")]
        [SerializeField]
        private Material scaleHandleMaterial;
        [SerializeField]
        private Material rotateHandleMaterial;
        [SerializeField]
        private Material interactingMaterial;

        [Header("Preset Components")]
        [SerializeField]
        [Tooltip("To visualize the object bounding box, drop the MixedRealityToolkit/UX/Prefabs/BoundingBoxes/BoundingBoxBasic.prefab here.")]
        public BoundingBox BoundingBoxPrefab;
        [SerializeField]
        [Tooltip("AppBar prefab.")]
        private AppBar appBarPrefab;

        private BoundingBox boxInstance;
        private GameObject objectToBound;
        private AppBar appBarInstance;
        private GameObject[] rotateHandles;
        private GameObject[] cornerHandles;
        private List<Vector3> handleCentroids;
        private GameObject transformRig;
        private bool showRig = false;
        private Vector3 scaleHandleSize = new Vector3(0.04f, 0.04f, 0.04f);
        private Vector3 rotateHandleSize = new Vector3(0.04f, 0.04f, 0.04f);
        private bool destroying = false;

        public Material ScaleHandleMaterial
        {
            get
            {
                return scaleHandleMaterial;
            }

            set
            {
                scaleHandleMaterial = value;
            }
        }

        public Material RotateHandleMaterial
        {
            get
            {
                return rotateHandleMaterial;
            }

            set
            {
                rotateHandleMaterial = value;
            }
        }

        public Material InteractingMaterial
        {
            get
            {
                return interactingMaterial;
            }

            set
            {
                interactingMaterial = value;
            }
        }

        public void Activate()
        {
            ShowRig = true;
        }

        public void Deactivate()
        {
            ShowRig = false;
        }

        public void FocusOnHandle(GameObject handle)
        {
            if (handle != null)
            {
                for (int i = 0; i < rotateHandles.Length; ++i)
                {
                    rotateHandles[i].SetActive(rotateHandles[i].gameObject == handle);
                }
                for (int i = 0; i < cornerHandles.Length; ++i)
                {
                    cornerHandles[i].SetActive(cornerHandles[i].gameObject == handle);
                }
            }
            else
            {
                for (int i = 0; i < rotateHandles.Length; ++i)
                {
                    rotateHandles[i].SetActive(true);
                }
                for (int i = 0; i < cornerHandles.Length; ++i)
                {
                    cornerHandles[i].SetActive(true);
                }
            }
        }

        private void Start()
        {
            objectToBound = this.gameObject;

            boxInstance = Instantiate(BoundingBoxPrefab) as BoundingBox;
            boxInstance.Target = objectToBound;
            boxInstance.FlattenPreference = flattenedAxis;

            BuildRig();

            appBarInstance = Instantiate(appBarPrefab) as AppBar;
            appBarInstance.BoundingBox = boxInstance;

            boxInstance.IsVisible = false;
        }
        private void Update()
        {
            if (destroying == false && ShowRig)
            {
                UpdateBoundsPoints();
                UpdateHandles();
            }
        }

        private void OnDestroy()
        {
            destroying = true;
            ShowRig = false;
            ClearHandles();
        }

        private void UpdateBoundsPoints()
        {
            handleCentroids = GetBounds();
        }

        private void CreateHandles()
        {
            ClearHandles();
            UpdateCornerHandles();
            UpdateRotateHandles();
            ParentHandles();
            UpdateHandles();
        }

        private void UpdateCornerHandles()
        {
            handleCentroids = handleCentroids ?? GetBounds();

            if (cornerHandles == null)
            {
                cornerHandles = new GameObject[handleCentroids.Count];
                for (int i = 0; i < cornerHandles.Length; ++i)
                {
                    cornerHandles[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cornerHandles[i].GetComponent<Renderer>().material = scaleHandleMaterial;
                    cornerHandles[i].transform.localScale = scaleHandleSize;
                    cornerHandles[i].AddComponent<BoundingBoxGizmoHandle>();
                    cornerHandles[i].GetComponent<BoundingBoxGizmoHandle>().Rig = this;
                    cornerHandles[i].GetComponent<BoundingBoxGizmoHandle>().TransformToAffect = objectToBound.transform;
                    cornerHandles[i].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.Y;
                    cornerHandles[i].GetComponent<BoundingBoxGizmoHandle>().AffineType = BoundingBoxGizmoHandle.TransformType.Scale;
                    cornerHandles[i].name = "Corner " + i.ToString();
                }
            }

            for (int i = 0; i < cornerHandles.Length; ++i)
            {
                cornerHandles[i].transform.position = handleCentroids[i];
                cornerHandles[i].transform.localRotation = objectToBound.transform.rotation;
            }
        }

        private void UpdateRotateHandles()
        {
            handleCentroids = handleCentroids ?? GetBounds();

            if (rotateHandles == null)
            {
                rotateHandles = new GameObject[12];

                for (int i = 0; i < rotateHandles.Length; ++i)
                {
                    rotateHandles[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    rotateHandles[i].GetComponent<Renderer>().material = rotateHandleMaterial;
                    rotateHandles[i].transform.localScale = rotateHandleSize;
                    rotateHandles[i].AddComponent<BoundingBoxGizmoHandle>();
                    rotateHandles[i].GetComponent<BoundingBoxGizmoHandle>().Rig = this;
                    rotateHandles[i].GetComponent<BoundingBoxGizmoHandle>().TransformToAffect = objectToBound.transform;
                    rotateHandles[i].GetComponent<BoundingBoxGizmoHandle>().AffineType = BoundingBoxGizmoHandle.TransformType.Rotation;
                    rotateHandles[i].name = "Middle " + i.ToString();
                }

                //set axis to affect
                rotateHandles[0].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.Y;
                rotateHandles[1].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.Y;
                rotateHandles[2].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.Y;
                rotateHandles[3].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.Y;

                rotateHandles[4].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.Z;
                rotateHandles[5].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.Z;
                rotateHandles[6].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.Z;
                rotateHandles[7].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.Z;

                rotateHandles[8].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.X;
                rotateHandles[9].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.X;
                rotateHandles[10].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.X;
                rotateHandles[11].GetComponent<BoundingBoxGizmoHandle>().Axis = BoundingBoxGizmoHandle.AxisToAffect.X;

                //set lefthandedness
                rotateHandles[0].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = false;
                rotateHandles[1].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = false;
                rotateHandles[2].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = false;
                rotateHandles[3].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = false;

                rotateHandles[4].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = false;
                rotateHandles[5].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = false;
                rotateHandles[6].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = true;
                rotateHandles[7].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = true;

                rotateHandles[8].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = false;
                rotateHandles[9].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = true;
                rotateHandles[10].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = false;
                rotateHandles[11].GetComponent<BoundingBoxGizmoHandle>().IsLeftHandedRotation = true;
            }

            rotateHandles[0].transform.localPosition = (handleCentroids[2] + handleCentroids[0]) * 0.5f;
            rotateHandles[1].transform.localPosition = (handleCentroids[3] + handleCentroids[1]) * 0.5f;
            rotateHandles[2].transform.localPosition = (handleCentroids[6] + handleCentroids[4]) * 0.5f;
            rotateHandles[3].transform.localPosition = (handleCentroids[7] + handleCentroids[5]) * 0.5f;
            rotateHandles[4].transform.localPosition = (handleCentroids[0] + handleCentroids[1]) * 0.5f;
            rotateHandles[5].transform.localPosition = (handleCentroids[2] + handleCentroids[3]) * 0.5f;
            rotateHandles[6].transform.localPosition = (handleCentroids[4] + handleCentroids[5]) * 0.5f;
            rotateHandles[7].transform.localPosition = (handleCentroids[6] + handleCentroids[7]) * 0.5f;
            rotateHandles[8].transform.localPosition = (handleCentroids[0] + handleCentroids[4]) * 0.5f;
            rotateHandles[9].transform.localPosition = (handleCentroids[1] + handleCentroids[5]) * 0.5f;
            rotateHandles[10].transform.localPosition = (handleCentroids[2] + handleCentroids[6]) * 0.5f;
            rotateHandles[11].transform.localPosition = (handleCentroids[3] + handleCentroids[7]) * 0.5f;
        }

        private void ParentHandles()
        {
            transformRig.transform.position = boxInstance.transform.position;
            transformRig.transform.rotation = boxInstance.transform.rotation;

            Vector3 invScale = objectToBound.transform.localScale;

            transformRig.transform.localScale = new Vector3(0.5f / invScale.x, 0.5f / invScale.y, 0.5f / invScale.z);
            transformRig.transform.parent = objectToBound.transform;
        }

        private void UpdateHandles()
        {
            UpdateCornerHandles();
            UpdateRotateHandles();
        }

        private void ClearCornerHandles()
        {
            if (cornerHandles != null)
            {
                for (int i = 0; i < cornerHandles.Length; ++i)
                {
                    GameObject.Destroy(cornerHandles[i]);
                }
                cornerHandles = null;
                handleCentroids = null;
            }

            cornerHandles = null;
            handleCentroids = null;
        }

        private void ClearRotateHandles()
        {
            if (rotateHandles != null && rotateHandles.Length > 0 && rotateHandles[0] != null)
            {
                for (int i = 0; i < rotateHandles.Length; ++i)
                {
                    if (rotateHandles[i] != null)
                    {
                        Destroy(rotateHandles[i]);
                        rotateHandles[i] = null;
                    }
                }
            }

            rotateHandles = null;
        }

        private void ClearHandles()
        {
            ClearCornerHandles();
            ClearRotateHandles();
        }

        private GameObject BuildRig()
        {
            Vector3 scale = objectToBound.transform.localScale;

            GameObject rig = new GameObject();
            rig.name = "center";
            rig.transform.SetPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);
            rig.transform.localScale = new Vector3(1.0f / scale.x, 1.0f / scale.y, 1.0f / scale.z);

            GameObject upperLeftFront = new GameObject();
            upperLeftFront.name = "upperleftfront";
            upperLeftFront.transform.SetPositionAndRotation(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity);
            upperLeftFront.transform.localScale = new Vector3(1, 1, 1);
            upperLeftFront.transform.parent = rig.transform;

            GameObject upperLeftBack = new GameObject();
            upperLeftBack.name = "upperleftback";
            upperLeftBack.transform.SetPositionAndRotation(new Vector3(0.5f, 0.5f, -0.5f), Quaternion.identity);
            upperLeftBack.transform.localScale = new Vector3(1, 1, 1);
            upperLeftBack.transform.parent = rig.transform;

            GameObject lowerLeftFront = new GameObject();
            lowerLeftFront.name = "lowerleftfront";
            lowerLeftFront.transform.SetPositionAndRotation(new Vector3(0.5f, -0.5f, 0.5f), Quaternion.identity);
            lowerLeftFront.transform.localScale = new Vector3(1, 1, 1);
            lowerLeftFront.transform.parent = rig.transform;

            GameObject lowerLeftBack = new GameObject();
            lowerLeftBack.name = "lowerleftback";
            lowerLeftBack.transform.SetPositionAndRotation(new Vector3(0.5f, -0.5f, -0.5f), Quaternion.identity);
            lowerLeftBack.transform.localScale = new Vector3(1, 1, 1);
            lowerLeftBack.transform.parent = rig.transform;

            GameObject upperRightFront = new GameObject();
            upperRightFront.name = "upperrightfront";
            upperRightFront.transform.SetPositionAndRotation(new Vector3(-0.5f, 0.5f, 0.5f), Quaternion.identity);
            upperRightFront.transform.localScale = new Vector3(1, 1, 1);
            upperRightFront.transform.parent = rig.transform;

            GameObject upperRightBack = new GameObject();
            upperRightBack.name = "upperrightback";
            upperRightBack.transform.SetPositionAndRotation(new Vector3(-0.5f, 0.5f, -0.5f), Quaternion.identity);
            upperRightBack.transform.localScale = new Vector3(1, 1, 1);
            upperRightBack.transform.parent = rig.transform;

            GameObject lowerRightFront = new GameObject();
            lowerRightFront.name = "lowerrightfront";
            lowerRightFront.transform.SetPositionAndRotation(new Vector3(-0.5f, -0.5f, 0.5f), Quaternion.identity);
            lowerRightFront.transform.localScale = new Vector3(1, 1, 1);
            lowerRightFront.transform.parent = rig.transform;

            GameObject lowerRightBack = new GameObject();
            lowerRightBack.name = "lowerrightback";
            lowerRightBack.transform.SetPositionAndRotation(new Vector3(-0.5f, -0.5f, -0.5f), Quaternion.identity);
            lowerRightBack.transform.localScale = new Vector3(1, 1, 1);
            lowerRightBack.transform.parent = rig.transform;

            transformRig = rig;

            return rig;
        }

        private bool ShowRig
        {
            get
            {
                return showRig;
            }
            set
            {
                if (destroying == false)
                {
                    if (value == true)
                    {
                        UpdateBoundsPoints();
                        UpdateHandles();
                    }

                    if (boxInstance != null)
                    {
                        boxInstance.IsVisible = value;
                    }

                    if (cornerHandles != null && rotateHandles != null)
                    {
                        foreach (GameObject handle in cornerHandles)
                        {
                            handle.SetActive(value);
                        }
                        foreach (GameObject handle in rotateHandles)
                        {
                            handle.SetActive(value);
                        }
                    }

                    showRig = value;
                }
            }
        }

        private List<Vector3> GetBounds()
        {
            if (objectToBound != null)
            {
                List<Vector3> bounds = new List<Vector3>();
                LayerMask mask = new LayerMask();

                GameObject clone = GameObject.Instantiate(boxInstance.gameObject);
                clone.transform.localRotation = Quaternion.identity;
                clone.transform.position = new Vector3(0, 0, 0);
                BoundingBox.GetMeshFilterBoundsPoints(clone, bounds, mask);
                Vector3 centroid = boxInstance.TargetBoundsCenter;
                GameObject.Destroy(clone);
                Matrix4x4 m = Matrix4x4.Rotate(objectToBound.transform.rotation);
                for (int i = 0; i < bounds.Count; ++i)
                {
                    bounds[i] = m.MultiplyPoint(bounds[i]);
                    bounds[i] += boxInstance.TargetBoundsCenter;
                }

                return bounds;
            }

            return null;
        }

        private BoundingBox.FlattenModeEnum GetBestAxisToFlatten()
        {
            int index = handleCentroids.Count - 8;
            float width = (handleCentroids[index + 0] - handleCentroids[index + 4]).magnitude;
            float height = (handleCentroids[index + 0] - handleCentroids[index + 2]).magnitude;
            float depth = (handleCentroids[index + 0] - handleCentroids[index + 1]).magnitude;

            if (width < height && width < depth)
            {
                return BoundingBox.FlattenModeEnum.FlattenX;
            }
            else if (height < width && height < depth)
            {
                return BoundingBox.FlattenModeEnum.FlattenY;
            }
            else if (depth < height && depth < width)
            {
                return BoundingBox.FlattenModeEnum.FlattenZ;
            }
          
            return BoundingBox.FlattenModeEnum.DoNotFlatten;
        }
    }
}