﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if !WINDOWS_UWP
// When the .NET scripting backend is enabled and C# projects are built
// The assembly that this file is part of is still built for the player,
// even though the assembly itself is marked as a test assembly (this is not
// expected because test assemblies should not be included in player builds).
// Because the .NET backend is deprecated in 2018 and removed in 2019 and this
// issue will likely persist for 2018, this issue is worked around by wrapping all
// play mode tests in this check.

using Microsoft.MixedReality.Toolkit.Boundary;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Microsoft.MixedReality.Toolkit.Tests
{
    // Tests to verify that changing the active profile at runtime results in
    // the MRTK being correctly reconfigured.
    public class ChangeActiveProfileTests
    {
        [TearDown]
        public void TearDown()
        {
            TestUtilities.ShutdownMixedRealityToolkit();
        }

        private const string DefaultHoloLens2Profile = "Assets/MixedRealityToolkit.SDK/Profiles/HoloLens2/DefaultHoloLens2ConfigurationProfile.asset";
        private const string BoundaryOnlyProfile = "Assets/MixedRealityToolkit.Tests/PlayModeTests/Core/TestProfiles/BoundaryOnly.asset";

        /// <summary>
        /// Test to verify that switching profiles results in appropriate service counts.
        /// </summary>
        [UnityTest]
        public IEnumerator VerifyServiceCount()
        {
            MixedRealityToolkitConfigurationProfile profile1 = TestUtilities.GetDefaultMixedRealityProfile<MixedRealityToolkitConfigurationProfile>();
            yield return null;

            // Initialize the test case with profile 1
            InitializeTest(profile1);

            // Get count of registered services
            IReadOnlyList<IMixedRealityService> services = MixedRealityServiceRegistry.GetAllServices();
            int count1 = services.Count;
            yield return null;

            // Switch to profile 2
            MixedRealityToolkitConfigurationProfile profile2 = LoadTestProfile(DefaultHoloLens2Profile);
            ChangeProfile(profile2);
            yield return null;

            // Get count of registered services
            services = MixedRealityServiceRegistry.GetAllServices();
            int count2 = services.Count;

            // We specifically selected the test profiles to ensure that they load a different number of services.
            Assert.IsTrue(count1 != count2);
        }

        /// <summary>
        /// Test to verify that switching profiles results the expected service state.
        /// </summary>
        [UnityTest]
        public IEnumerator VerifyServiceState()
        {
            MixedRealityToolkitConfigurationProfile profile1 = TestUtilities.GetDefaultMixedRealityProfile<MixedRealityToolkitConfigurationProfile>();
            yield return null;

            // Initialize the test case with profile 1
            InitializeTest(profile1);

            // Cache the interesting settings read from the initial boundary system instance
            IMixedRealityBoundarySystem boundarySystem1 = CoreServices.BoundarySystem;
            yield return null;

            MixedRealityToolkitConfigurationProfile profile2 = LoadTestProfile(BoundaryOnlyProfile);

            // Switch to profile 2
            ChangeProfile(profile2);
            yield return null;

            // The custom boundary profile has been configured to match the default with the following fields being different
            // * Boundary height
            // * Floor plane physics layer
            // * Show tracked area
            // * Show boundary ceiling
            IMixedRealityBoundarySystem boundarySystem2 = CoreServices.BoundarySystem;
            yield return null;

            // Check service settings to ensure it has properly reset
            Assert.IsTrue(boundarySystem1.BoundaryHeight != boundarySystem2.BoundaryHeight);
            Assert.IsTrue(boundarySystem1.FloorPhysicsLayer != boundarySystem2.FloorPhysicsLayer);
            Assert.IsTrue(boundarySystem1.ShowTrackedArea != boundarySystem2.ShowTrackedArea);
            Assert.IsTrue(boundarySystem1.ShowBoundaryCeiling != boundarySystem2.ShowBoundaryCeiling);
        }

        /// <summary>
        /// Test to verify that switching profiles results in the expected playspace children.
        /// </summary>
        [UnityTest]
        public IEnumerator VerifyPlayspaceChildren()
        {
            MixedRealityToolkitConfigurationProfile profile1 = TestUtilities.GetDefaultMixedRealityProfile<MixedRealityToolkitConfigurationProfile>();
            yield return null;

            // Initialize the test case with profile 1
            InitializeTest(profile1);

            // Switch between the profiles a few times.
            MixedRealityToolkitConfigurationProfile profile2 = LoadTestProfile(DefaultHoloLens2Profile);
            ChangeProfile(profile2);
            yield return null;
            ChangeProfile(profile1);
            yield return null;
            ChangeProfile(profile2);
            yield return null;

            int uiRaycastCameraCount = 0;
            // Confirm that we have one UIRaycastCamera.
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            foreach (Camera camera in cameras)
            {
                if ("UIRaycastCamera" == camera.name)
                {
                    uiRaycastCameraCount++;
                }
            }
            Assert.AreEqual(1, uiRaycastCameraCount);

            // Confirm that we have only one instance of the default cursor.
            int defaultCursorCount = 0;
            foreach (Transform child in MixedRealityPlayspace.Transform.GetComponentsInChildren<Transform>())
            {
                if ("DefaultCursor(Clone)" == child.name)
                {
                    defaultCursorCount++;
                }
            }
            Assert.AreEqual(1, defaultCursorCount);
        }

        private void InitializeTest(MixedRealityToolkitConfigurationProfile profile)
        {
            TestUtilities.InitializeMixedRealityToolkit(profile);
            TestUtilities.PlayspaceToOriginLookingForward();
        }

        private void ChangeProfile(MixedRealityToolkitConfigurationProfile newProfile)
        {
            MixedRealityToolkitConfigurationProfile oldProfile = MixedRealityToolkit.Instance.ActiveProfile;
            Debug.Log($"Switching active profile from {oldProfile.name} to {newProfile.name}");
            MixedRealityToolkit.Instance.ActiveProfile = newProfile;
        }

        private MixedRealityToolkitConfigurationProfile LoadTestProfile(string file)
        {
            return AssetDatabase.LoadAssetAtPath<MixedRealityToolkitConfigurationProfile>(file);
        }
    }
}

#endif // !WINDOWS_UWP