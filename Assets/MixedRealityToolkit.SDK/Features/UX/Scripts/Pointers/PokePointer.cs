﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Physics;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// A near interaction pointer that generates touch events based on touchables in close proximity.
    /// </summary>
    /// <remarks>
    /// _Reachable Objects_ are objects with a both a [BaseNearInteractionTouchable](xref:Microsoft.MixedReality.Toolkit.Input.BaseNearInteractionTouchable) and a collider within [TouchableDistance](xref:Microsoft.MixedReality.Toolkit.Input.PokePointer.TouchableDistance) from the poke pointer (based on [OverlapSphere](https://docs.unity3d.com/ScriptReference/Physics.OverlapSphere.html)).
    ///
    /// If a poke pointer has no [CurrentTouchableObjectDown](xref:Microsoft.MixedReality.Toolkit.Input.PokePointer.CurrentTouchableObjectDown), then it will try to select one from the Reachable Objects based on:
    /// 1. Layer mask priority: Lower-priority layer masks will only be considered if higher-priority layers don't contain any Reachable Objects.
    /// 1. Distance: the closest object in the highest priority layers is selected based on [DistanceToTouchable](xref:Microsoft.MixedReality.Toolkit.Input.BaseNearInteractionTouchable.DistanceToTouchable*).
    /// 1. Poke Threshold: The object becomes the [CurrentTouchableObjectDown](xref:Microsoft.MixedReality.Toolkit.Input.PokePointer.CurrentTouchableObjectDown) once it crosses the [PokeThreshold](xref:Microsoft.MixedReality.Toolkit.Input.BaseNearInteractionTouchable.PokeThreshold) distance (behind the surface). At this point the [OnTouchStarted](xref:Microsoft.MixedReality.Toolkit.Input.IMixedRealityTouchHandler.OnTouchStarted*) or [OnPointerDown](xref:Microsoft.MixedReality.Toolkit.Input.IMixedRealityPointerHandler.OnPointerDown*) event is raised.
    ///
    /// If a poke pointer _does_  have a [CurrentTouchableObjectDown](xref:Microsoft.MixedReality.Toolkit.Input.PokePointer.CurrentTouchableObjectDown) it will not consider any other object, until the [DistanceToTouchable](xref:Microsoft.MixedReality.Toolkit.Input.BaseNearInteractionTouchable.DistanceToTouchable*) exceeds the [DebounceThreshold](xref:Microsoft.MixedReality.Toolkit.Input.BaseNearInteractionTouchable.DebounceThreshold) (in front of the surface). At this point the active object is cleared and the [OnTouchCompleted](xref:Microsoft.MixedReality.Toolkit.Input.IMixedRealityTouchHandler.OnTouchCompleted*) or [OnPointerUp](xref:Microsoft.MixedReality.Toolkit.Input.IMixedRealityPointerHandler.OnPointerUp*) event is raised.
    /// </remarks>
    public class PokePointer : BaseControllerPointer, IMixedRealityNearPointer
    {
        [SerializeField]
        protected LineRenderer line;

        [SerializeField]
        protected GameObject visuals;

        [SerializeField]
        [Tooltip("Maximum distance a which a touchable surface can be interacted with.")]
        protected float touchableDistance = 0.2f;
        /// <summary>
        /// Maximum distance a which a touchable surface can be interacted with.
        /// </summary>
        public float TouchableDistance => touchableDistance;

        [SerializeField]
        [Tooltip("Maximum number of colliders that can be detected in a scene query.")]
        [Min(1)]
        private int sceneQueryBufferSize = 64;
        /// <summary>
        /// Maximum number of colliders that can be detected in a scene query.
        /// </summary>
        public int SceneQueryBufferSize => sceneQueryBufferSize;

        [SerializeField]
        [Tooltip("The LayerMasks, in prioritized order, that are used to determine the touchable objects.")]
        private LayerMask[] pokeLayerMasks = { UnityEngine.Physics.DefaultRaycastLayers };
        /// <summary>
        /// The LayerMasks, in prioritized order, that are used to determine the touchable objects.
        /// </summary>
        /// <remarks>
        /// Only [BaseNearInteractionTouchables](xref:Microsoft.MixedReality.Toolkit.Input.BaseNearInteractionTouchable) in one of the LayerMasks will raise touch events.
        /// </remarks>
        public LayerMask[] PokeLayerMasks => pokeLayerMasks;

        [SerializeField]
        [Tooltip("Specify whether queries for touchable surfaces hit triggers.")]
        protected QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal;
        /// <summary>
        /// Specify whether queries for touchable surfaces hit triggers.
        /// </summary>
        public QueryTriggerInteraction TriggerInteraction => triggerInteraction;

        private Collider[] queryBuffer;

        private float closestDistance = 0.0f;

        private Vector3 closestNormal = Vector3.forward;
        // previous frame pointer position
        public Vector3 PreviousPosition { get; private set; } = Vector3.zero;

        private BaseNearInteractionTouchable closestProximityTouchable = null;
        /// <summary>
        /// The closest touchable component that has been detected.
        /// </summary>
        /// <remarks>
        /// The closest touchable component limits the set of objects which are currently touchable.
        /// These are all the game objects in the subtree of the closest touchable component's owner object.
        /// </remarks>
        public BaseNearInteractionTouchable ClosestProximityTouchable => closestProximityTouchable;

        private GameObject currentTouchableObjectDown = null;
        /// <summary>
        /// The current object that is being touched.
        /// </summary>
        /// We need to make sure to consistently fire
        /// poke-down / poke-up events for this object. This is also the case when the object within
        /// the same current closest touchable component's changes (e.g. Unity UI control elements).
        public GameObject CurrentTouchableObjectDown => currentTouchableObjectDown;

        protected override void Start()
        {
            base.Start();

            queryBuffer = new Collider[sceneQueryBufferSize];
        }

        protected void OnValidate()
        {
            Debug.Assert(line != null, this);
            Debug.Assert(visuals != null, this);
            touchableDistance = Mathf.Max(touchableDistance, 0);
            sceneQueryBufferSize = Mathf.Max(sceneQueryBufferSize, 1);
        }

        /// <inheritdoc />
        public bool IsNearObject
        {
            get { return (closestProximityTouchable != null); }
        }

        /// <inheritdoc />
        public override bool IsInteractionEnabled => base.IsInteractionEnabled && IsNearObject;

        public override void OnPreSceneQuery()
        {
            if (Rays == null)
            {
                Rays = new RayStep[1];
            }

            closestNormal = Rotation * Vector3.forward;

            var layerMasks = PrioritizedLayerMasksOverride ?? PokeLayerMasks;

            // Find closest touchable
            BaseNearInteractionTouchable newClosestTouchable = null;
            foreach (var layerMask in layerMasks)
            {
                if (FindClosestTouchableForLayerMask(layerMask, out newClosestTouchable, out closestDistance, out closestNormal))
                {
                    break;
                }
            }

            if (newClosestTouchable != null)
            {
                // Build ray (poke from in front to the back of the pointer position)
                Vector3 start = Position - newClosestTouchable.PokeThreshold * -closestNormal;
                Vector3 end = Position + touchableDistance * -closestNormal;
                Rays[0].UpdateRayStep(ref start, ref end);

                line.SetPosition(0, Position);
                line.SetPosition(1, end);
            }

            // Check if the currently touched object is still part of the new touchable.
            if (currentTouchableObjectDown != null)
            {
                if (!IsObjectPartOfTouchable(currentTouchableObjectDown, newClosestTouchable))
                {
                    TryRaisePokeUp();
                }
            }

            // Set new touchable only now: If we have to raise a poke-up event for the previous touchable object,
            // we need to do so using the previous touchable in TryRaisePokeUp().
            closestProximityTouchable = newClosestTouchable;

            visuals.SetActive(IsActive);
        }

        private bool FindClosestTouchableForLayerMask(LayerMask layerMask, out BaseNearInteractionTouchable closest, out float closestDistance, out Vector3 closestNormal)
        {
            closest = null;
            closestDistance = float.PositiveInfinity;
            closestNormal = Vector3.zero;

            int numColliders = UnityEngine.Physics.OverlapSphereNonAlloc(Position, touchableDistance, queryBuffer, layerMask, triggerInteraction);
            if (numColliders == queryBuffer.Length)
            {
                Debug.LogWarning($"Maximum number of {numColliders} colliders found in PokePointer overlap query. Consider increasing the query buffer size in the pointer profile.");
            }

            for (int i = 0; i < numColliders; ++i)
            {
                var touchable = queryBuffer[i].GetComponent<ColliderNearInteractionTouchable>();
                if (touchable)
                {
                    float distance = touchable.DistanceToTouchable(Position, out Vector3 normal);
                    if (distance < closestDistance)
                    {
                        closest = touchable;
                        closestDistance = distance;
                        closestNormal = normal;
                    }
                }
            }

            // Unity UI does not provide an equivalent broad-phase test to Physics.OverlapSphere,
            // so we have to use a static instances list to test all NearInteractionTouchableUnityUI
            for (int i = 0; i < NearInteractionTouchableUnityUI.Instances.Count; i++)
            {
                NearInteractionTouchableUnityUI touchable = NearInteractionTouchableUnityUI.Instances[i];
                float distance = touchable.DistanceToTouchable(Position, out Vector3 normal);
                if (distance <= touchableDistance && distance < closestDistance)
                {
                    closest = touchable;
                    closestDistance = distance;
                    closestNormal = normal;
                }
            }

            return closest != null;
        }

        public override void OnPostSceneQuery()
        {
            base.OnPostSceneQuery();

            if (!IsActive)
            {
                return;
            }

            if (Result?.CurrentPointerTarget != null && closestProximityTouchable != null)
            {
                // Start position of the ray is offset by PokeThreshold, subtract to get distance between surface and pointer position.
                float distToFront = Vector3.Distance(Result.StartPoint, Result.Details.Point) - closestProximityTouchable.PokeThreshold;
                bool newIsDown = (distToFront < closestProximityTouchable.PokeThreshold);
                bool newIsUp = (distToFront > closestProximityTouchable.PokeThreshold + closestProximityTouchable.DebounceThreshold);

                if (newIsDown)
                {
                    TryRaisePokeDown();
                }
                else if (currentTouchableObjectDown != null)
                {
                    if (newIsUp)
                    {
                        TryRaisePokeUp();
                    }
                    else
                    {
                        TryRaisePokeDown();
                    }
                }
            }

            if (!IsNearObject)
            {
                line.endColor = line.startColor = new Color(1, 1, 1, 0.25f);
            }
            else if (currentTouchableObjectDown == null)
            {
                line.endColor = line.startColor = new Color(1, 1, 1, 0.75f);
            }
            else
            {
                line.endColor = line.startColor = new Color(0, 0, 1, 0.75f);
            }

            PreviousPosition = Position;
        }

        public override void OnPreCurrentPointerTargetChange()
        {
            // We need to raise the event now, since the pointer's focused object or touchable will change
            // after we leave this function. This will make sure the same object that received the Down event
            // will also receive the Up event.
            TryRaisePokeUp();
        }

        private void TryRaisePokeDown()
        {
            GameObject targetObject = Result.CurrentPointerTarget;

            if (currentTouchableObjectDown == null)
            {
                // In order to get reliable up/down event behavior, only allow the closest touchable to be touched.
                if (IsObjectPartOfTouchable(targetObject, closestProximityTouchable))
                {
                    currentTouchableObjectDown = targetObject;

                    if (closestProximityTouchable.EventsToReceive == TouchableEventType.Pointer)
                    {
                        InputSystem?.RaisePointerDown(this, pointerAction, Handedness);
                    }
                    else if (closestProximityTouchable.EventsToReceive == TouchableEventType.Touch)
                    {
                        InputSystem?.RaiseOnTouchStarted(InputSourceParent, Controller, Handedness, Position);
                    }
                }
            }
            else
            {
                RaiseTouchUpdated(targetObject, Position);
            }
        }

        private void TryRaisePokeUp()
        {
            if (currentTouchableObjectDown != null)
            {
                Debug.Assert(Result.CurrentPointerTarget == currentTouchableObjectDown, "PokeUp will not be raised for correct object.");

                if (closestProximityTouchable.EventsToReceive == TouchableEventType.Pointer)
                {
                    InputSystem.RaisePointerClicked(this, pointerAction, 0, Handedness);
                    InputSystem?.RaisePointerUp(this, pointerAction, Handedness);
                }
                else if (closestProximityTouchable.EventsToReceive == TouchableEventType.Touch)
                {
                    InputSystem?.RaiseOnTouchCompleted(InputSourceParent, Controller, Handedness, Position);
                }

                currentTouchableObjectDown = null;
            }
        }

        private void RaiseTouchUpdated(GameObject targetObject, Vector3 touchPosition)
        {
            if (currentTouchableObjectDown != null)
            {
                Debug.Assert(Result?.CurrentPointerTarget == currentTouchableObjectDown);

                if (closestProximityTouchable.EventsToReceive == TouchableEventType.Touch)
                {
                    InputSystem?.RaiseOnTouchUpdated(InputSourceParent, Controller, Handedness, touchPosition);
                }
            }
        }

        private static bool IsObjectPartOfTouchable(GameObject targetObject, BaseNearInteractionTouchable touchable)
        {
            return targetObject != null && touchable != null &&
                (targetObject == touchable.gameObject ||
                // Descendant game objects are touchable as well. In particular, this is needed to be able to send
                // touch events to Unity UI control elements.
                (targetObject.transform != null && touchable.gameObject.transform != null &&
                targetObject.transform.IsChildOf(touchable.gameObject.transform)));
        }

        /// <inheritdoc />
        bool IMixedRealityNearPointer.TryGetNearGraspPoint(out Vector3 position)
        {
            position = Vector3.zero;
            return false;
        }

        /// <inheritdoc />
        bool IMixedRealityNearPointer.TryGetDistanceToNearestSurface(out float distance)
        {
            distance = closestDistance;
            return true;
        }

        /// <inheritdoc />
        bool IMixedRealityNearPointer.TryGetNormalToNearestSurface(out Vector3 normal)
        {
            normal = closestNormal;
            return true;
        }

        /// <inheritdoc />
        public override void OnSourceLost(SourceStateEventData eventData)
        {
            TryRaisePokeUp();

            base.OnSourceLost(eventData);
        }

        public override void OnSourceDetected(SourceStateEventData eventData)
        {
            base.OnSourceDetected(eventData);
            PreviousPosition = Position;
        }

        public override void OnInputDown(InputEventData eventData)
        {
            // Poke pointer should not respond when a button is pressed or hand is pinched
            // It should only dispatch events based on collision with touchables.
        }

        public override void OnInputUp(InputEventData eventData)
        {
            // Poke pointer should not respond when a button is released or hand is un-pinched
            // It should only dispatch events based on collision with touchables.
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Debug.Assert(line != null, "No line renderer found in PokePointer.");
            Debug.Assert(visuals != null, "No visuals object found in PokePointer.");
        }
    }
}
