﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Managers;
using Microsoft.MixedReality.Toolkit.Core.Utilities.Async;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.Input
{
    /// <summary>
    /// This component ensures that all input events are forwarded to this <see cref="GameObject"/> when focus or gaze is not required.
    /// </summary>
    public class InputSystemGlobalListener : MonoBehaviour
    {
        private bool lateInitialize = true;

        protected readonly WaitUntil WaitUntilInputSystemValid = new WaitUntil(() => MixedRealityManager.InputSystem != null);

        protected virtual void OnEnable()
        {
            if (MixedRealityManager.IsInitialized && MixedRealityManager.InputSystem != null && !lateInitialize)
            {
                MixedRealityManager.InputSystem.Register(gameObject);
            }
        }

        protected virtual async void Start()
        {
            if (lateInitialize)
            {
                await WaitUntilInputSystemValid;
                lateInitialize = false;
                MixedRealityManager.InputSystem.Register(gameObject);
            }
        }

        protected virtual void OnDisable()
        {
            MixedRealityManager.InputSystem?.Unregister(gameObject);
        }
    }
}
