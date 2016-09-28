﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HoloToolkit.Unity
{
    /// <summary>
    /// Use this class for easy access to OnTap, OnGazeEnder, and OnGazeExit events.
    /// Be sure to override each of these methods with your own implimentation.
    /// Examples of code in comments below.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            GestureManager.Instance.OnTap += OnTap;
            GazeManager.Instance.OnGazeExit += OnGazeExit;
            GazeManager.Instance.OnGazeEnter += OnGazeEnter;
        }

        protected virtual void OnDisable()
        {
            GestureManager.Instance.OnTap -= OnTap;
            GazeManager.Instance.OnGazeExit -= OnGazeExit;
            GazeManager.Instance.OnGazeEnter -= OnGazeEnter;
        }

        /// <summary>
        /// Called when a user has tapped any gameObject.
        /// </summary>
        /// <param name="tappedGameObject">GameObject user has tapped.</param>
        protected virtual void OnTap(GameObject tappedGameObject)
        {
            //if (tappedGameObject == gameObject)
            //{
            //    // Do something if our gameObject has been tapped
            //}
            //else
            //{
            //    // Do something if we've tapped any object except for our own
            //}
        }

        /// <summary>
        /// Called when a user's gaze enters any gameObject.
        /// </summary>
        /// <param name="focusedObject">The GameObject that the users gaze has entered.</param>
        protected virtual void OnGazeEnter(GameObject focusedObject)
        {
            //if (focusedObject == gameObject)
            //{
            //    // Do something if our gaze has entered this gameObject
            //}
            //else
            //{
            //    // Do something if our gaze has entered another gameObject
            //}
        }

        /// <summary>
        /// Called when a user's gaze exits any gameObject.
        /// </summary>
        /// <param name="focusedObject">The GameObject that the users gaze has left.</param>
        protected virtual void OnGazeExit(GameObject focusedObject)
        {
            //if(focusedObject == gameObject)
            //{
            //    // Do something if our gaze has left this gameObject
            //}
            //else
            //{
            //    // Do something if our gaze has left another gameObject
            //}
        }
    }
}