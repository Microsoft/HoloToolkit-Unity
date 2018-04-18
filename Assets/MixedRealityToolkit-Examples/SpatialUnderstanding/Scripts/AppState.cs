﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MixedRealityToolkit.Common;
using MixedRealityToolkit.InputModule;
using MixedRealityToolkit.InputModule.EventData;
using MixedRealityToolkit.InputModule.InputHandlers;
using MixedRealityToolkit.InputModule.Utilities;
using MixedRealityToolkit.SpatialMapping;
using MixedRealityToolkit.SpatialUnderstanding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_WSA || UNITY_STANDALONE_WIN
using UnityEngine.Windows.Speech;
#endif

namespace MixedRealityToolkit.Examples.SpatialUnderstanding
{
    public class AppState : Singleton<AppState>, ISourceStateHandler, IPointerHandler
    {
        // Consts
        public float kMinAreaForStats = 5.0f;
        public float kMinAreaForComplete = 50.0f;
        public float kMinHorizAreaForComplete = 25.0f;
        public float kMinWallAreaForComplete = 10.0f;

        // Config
        public TextMesh DebugDisplay;
        public TextMesh DebugSubDisplay;
        public Transform Parent_Scene;
        public SpatialMappingObserver MappingObserver;
        public SpatialUnderstandingCursor AppCursor;

        // Properties
        public string SpaceQueryDescription
        {
            get
            {
                return spaceQueryDescription;
            }
            set
            {
                spaceQueryDescription = value;
                objectPlacementDescription = "";
            }
        }

        public string ObjectPlacementDescription
        {
            get
            {
                return objectPlacementDescription;
            }
            set
            {
                objectPlacementDescription = value;
                spaceQueryDescription = "";
            }
        }

        public bool DoesScanMeetMinBarForCompletion
        {
            get
            {
                // Only allow this when we are actually scanning
                if ((SpatialUnderstandingManager.Instance.ScanState != SpatialUnderstandingManager.ScanStates.Scanning) ||
                   (!SpatialUnderstandingManager.Instance.AllowSpatialUnderstanding))
                {
                    return false;
                }

                // Query the current playspace stats
                IntPtr statsPtr = SpatialUnderstandingManager.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                {
                    return false;
                }
                SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstandingManager.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

                // Check our preset requirements
                if ((stats.TotalSurfaceArea > kMinAreaForComplete) ||
                    (stats.HorizSurfaceArea > kMinHorizAreaForComplete) ||
                    (stats.WallSurfaceArea > kMinWallAreaForComplete))
                {
                    return true;
                }
                return false;
            }
        }

        public string PrimaryText
        {
            get
            {
                // Display the space and object query results (has priority)
                if (!string.IsNullOrEmpty(SpaceQueryDescription))
                {
                    return SpaceQueryDescription;
                }
                else if (!string.IsNullOrEmpty(ObjectPlacementDescription))
                {
                    return ObjectPlacementDescription;
                }

                // Scan state
                if (SpatialUnderstandingManager.Instance.AllowSpatialUnderstanding)
                {
                    switch (SpatialUnderstandingManager.Instance.ScanState)
                    {
                        case SpatialUnderstandingManager.ScanStates.Scanning:
                            // Get the scan stats
                            IntPtr statsPtr = SpatialUnderstandingManager.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                            if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                            {
                                return "playspace stats query failed";
                            }

                            // The stats tell us if we could potentially finish
                            if (DoesScanMeetMinBarForCompletion)
                            {
                                return "When ready, air tap to finalize your playspace";
                            }
                            return "Walk around and scan in your playspace";
                        case SpatialUnderstandingManager.ScanStates.Finishing:
                            return "Finalizing scan (please wait)";
                        case SpatialUnderstandingManager.ScanStates.Done:
                            return "Scan complete - Use the menu to run queries";
                        default:
                            return "ScanState = " + SpatialUnderstandingManager.Instance.ScanState.ToString();
                    }
                }
                return "";
            }
        }

        public Color PrimaryColor
        {
            get
            {
                if (SpatialUnderstandingManager.Instance.ScanState == SpatialUnderstandingManager.ScanStates.Scanning)
                {
                    if (trackedHandsCount > 0)
                    {
                        return DoesScanMeetMinBarForCompletion ? Color.green : Color.red;
                    }
                    return DoesScanMeetMinBarForCompletion ? Color.yellow : Color.white;
                }

                // If we're looking at the menu, fade it out
                Vector3 hitPos, hitNormal;
                UnityEngine.UI.Button hitButton;
                float alpha = AppCursor.RayCastUI(out hitPos, out hitNormal, out hitButton) ? 0.15f : 1.0f;

                // Special case processing & 
                return (!string.IsNullOrEmpty(SpaceQueryDescription) || !string.IsNullOrEmpty(ObjectPlacementDescription)) ?
                    (PrimaryText.Contains("processing") ? new Color(1.0f, 0.0f, 0.0f, 1.0f) : new Color(1.0f, 0.7f, 0.1f, alpha)) :
                    new Color(1.0f, 1.0f, 1.0f, alpha);
            }
        }

        public string DetailsText
        {
            get
            {
                if (SpatialUnderstandingManager.Instance.ScanState == SpatialUnderstandingManager.ScanStates.None)
                {
                    return "";
                }

                // Scanning stats get second priority
                if ((SpatialUnderstandingManager.Instance.ScanState == SpatialUnderstandingManager.ScanStates.Scanning) &&
                    (SpatialUnderstandingManager.Instance.AllowSpatialUnderstanding))
                {
                    IntPtr statsPtr = SpatialUnderstandingManager.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                    if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                    {
                        return "Playspace stats query failed";
                    }
                    SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstandingManager.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

                    // Start showing the stats when they are no longer zero
                    if (stats.TotalSurfaceArea > kMinAreaForStats)
                    {
                        string subDisplayText =
                            $"totalArea={stats.TotalSurfaceArea:0.0}, horiz={stats.HorizSurfaceArea:0.0}, wall={stats.WallSurfaceArea:0.0}";
                        subDisplayText +=
                            $"\nnumFloorCells={stats.NumFloor}, numCeilingCells={stats.NumCeiling}, numPlatformCells={stats.NumPlatform}";
                        subDisplayText +=
                            $"\npaintMode={stats.CellCount_IsPaintMode}, seenCells={stats.CellCount_IsSeenQualtiy_Seen + stats.CellCount_IsSeenQualtiy_Good}, notSeen={stats.CellCount_IsSeenQualtiy_None}";
                        return subDisplayText;
                    }
                    return "";
                }
                return "";
            }
        }

        // Privates
        private string spaceQueryDescription;
        private string objectPlacementDescription;
        private uint trackedHandsCount = 0;
#if UNITY_WSA || UNITY_STANDALONE_WIN
        private KeywordRecognizer keywordRecognizer;

        // Functions
        private void Start()
        {
            // Default the scene & the HoloToolkit objects to the camera
            Vector3 sceneOrigin = CameraCache.Main.transform.position;
            Parent_Scene.transform.position = sceneOrigin;
            MappingObserver.SetObserverOrigin(sceneOrigin);
            InputManager.AddGlobalListener(gameObject);


            var keywordsToActions = new Dictionary<string, Action>
            {
                { "Toggle Scanned Mesh", ToggleScannedMesh },
                { "Toggle Processed Mesh", ToggleProcessedMesh },
            };

            keywordRecognizer = new KeywordRecognizer(keywordsToActions.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += args => keywordsToActions[args.text].Invoke();
            keywordRecognizer.Start();
        }
#endif

        protected override void OnDestroy()
        {
            InputManager.RemoveGlobalListener(gameObject);
        }

        private void Update_DebugDisplay()
        {
            // Basic checks
            if (DebugDisplay == null)
            {
                return;
            }

            // Update display text
            DebugDisplay.text = PrimaryText;
            DebugDisplay.color = PrimaryColor;
            DebugSubDisplay.text = DetailsText;
        }

        private void Update_KeyboardInput(float deltaTime)
        {
            // Toggle SurfaceMapping & CustomUnderstandingMesh visibility
            if (Input.GetKeyDown(KeyCode.BackQuote) && (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)))
            {
                ToggleScannedMesh();
            }
            else if (Input.GetKeyDown(KeyCode.BackQuote) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                ToggleProcessedMesh();
            }
        }

        private static void ToggleScannedMesh()
        {
            SpatialMappingManager.Instance.DrawVisualMeshes = !SpatialMappingManager.Instance.DrawVisualMeshes;
            Debug.Log("SpatialUnderstanding -> SpatialMappingManager.Instance.DrawVisualMeshes=" + SpatialMappingManager.Instance.DrawVisualMeshes);
        }

        private static void ToggleProcessedMesh()
        {
            SpatialUnderstandingManager.Instance.UnderstandingCustomMesh.DrawProcessedMesh = !SpatialUnderstandingManager.Instance.UnderstandingCustomMesh.DrawProcessedMesh;
            Debug.Log("SpatialUnderstanding -> SpatialUnderstandingManager.Instance.UnderstandingCustomMesh.DrawProcessedMesh=" + SpatialUnderstandingManager.Instance.UnderstandingCustomMesh.DrawProcessedMesh);
        }

        private void Update()
        {
            Update_DebugDisplay();
            Update_KeyboardInput(Time.deltaTime);
        }

        void ISourceStateHandler.OnSourceDetected(SourceStateEventData eventData)
        {
            // If the source has positional info and there is currently no visible source
            if (eventData.InputSource.SupportsInputInfo(SupportedInputInfo.GripPosition))
            {
                trackedHandsCount++;
            }
        }

        void ISourceStateHandler.OnSourceLost(SourceStateEventData eventData)
        {
            if (eventData.InputSource.SupportsInputInfo(SupportedInputInfo.GripPosition))
            {
                trackedHandsCount--;
            }
        }

        void ISourceStateHandler.OnSourcePositionChanged(SourcePositionEventData eventData) { }

        void ISourceStateHandler.OnSourceRotationChanged(SourceRotationEventData eventData) { }

        void IPointerHandler.OnPointerUp(ClickEventData eventData) { }

        void IPointerHandler.OnPointerDown(ClickEventData eventData) { }

        void IPointerHandler.OnPointerClicked(ClickEventData eventData)
        {
            if ((SpatialUnderstandingManager.Instance.ScanState == SpatialUnderstandingManager.ScanStates.Scanning) &&
                !SpatialUnderstandingManager.Instance.ScanStatsReportStillWorking)
            {
                SpatialUnderstandingManager.Instance.RequestFinishScan();
            }
        }
    }
}