﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Microsoft.MixedReality.Toolkit.Experimental.Utilities
{
    /// <summary>
    /// A set of child objects organized in a series of Rows/Columns that can scroll in either the X or Y direction
    /// </summary>
    public class ScrollingObjectCollection : BaseObjectCollection, IMixedRealityPointerHandler, IMixedRealityTouchHandler, IMixedRealitySourceStateHandler
    {

        public enum VelocityType
        {
            FalloffPerFrame = 0,
            FalloffPerItem,
            NoVeloctySnapToItem,
            None
        }

        public enum ScrollDirectionType
        {
            UpAndDown = 0,
            LeftAndRight,
        }

        /// <summary>
        /// Automatically set up scroller at runtime 
        /// </summary>
        [SerializeField]
        [Tooltip("Automatically set up scroller at runtime")]
        private bool setUpAtRuntime = true;

        public bool SetUpAtRuntime
        {
            get => setUpAtRuntime;
            set => setUpAtRuntime = value;
        }

        /// <summary>
        /// Number of lines visible in scroller. orthagonal to <see cref="tiers"/>
        /// </summary>
        [SerializeField]
        [Tooltip("Number of lines visible in scroller, orthagonal to tiers")]
        private int viewableArea = 4;

        public int ViewableArea
        {
            get => (viewableArea > 0) ? viewableArea : 1;
            set => viewableArea = value;
        }

        /// <summary>
        /// The amount of movement the user's pointer can make before its considerd a drag gesture
        /// </summary>
        [SerializeField]
        [Tooltip("The amount of movement the user's pointer can make before its considerd a drag gesture")]
        [Range(0.0f, 2.0f)]
        private float handDeltaMagThreshold = 0.1f;

        public float HandDeltaMagThreshold
        {
            get => handDeltaMagThreshold;
            set => handDeltaMagThreshold = value;
        }

        /// <summary>
        /// The amount of time the user's hand can intersect a controller item before its considerd a drag gesture
        /// </summary>
        [SerializeField]
        [Tooltip("The amount of time the user's hand can intersect a controller item before its considerd a drag gesture")]
        [Range(0.0f, 2.0f)]
        private float dragTimeThreshold = 0.25f;

        public float DragTimeThreshold
        {
            get => dragTimeThreshold;
            set => dragTimeThreshold = value;
        }

        /// <summary>
        /// The direction in which content should scroll 
        /// </summary>
        [SerializeField]
        [Tooltip("The direction in which content should scroll")]
        private ScrollDirectionType scrollDirection;

        public ScrollDirectionType ScrollDirection
        {
            get => scrollDirection;
            set => scrollDirection = value;
        }

        /// <summary>
        /// Toggles whether the <see cref="ScrollingObjectCollection"/> will use the Camera <see cref="Camera.onPreRender"/> event to hide items in the list.
        /// </summary>
        /// <remarks>This is especially helpful if you're trying to scroll dynamically created objects that may be added to the list after <see cref"MonoBehaviour.LateUpdate()"/> such as <see cref"MonoBehaviour.OnWillRender()"/></remarks>
        [SerializeField]
        [Tooltip("Toggles whether the scrollingObjectCollection will use the Camera OnPreRender event to hide items in the list")]
        private bool useOnPreRender;

        public bool UseOnPreRender
        {
            get => useOnPreRender;
            set
            {
                if (cameraMethods == null)
                {
                    cameraMethods = CameraCache.Main.gameObject.EnsureComponent<CameraEventRouter>();
                }

                if (value)
                {
                    cameraMethods.OnCameraPreRender += OnCameraPreRender;
                }
                else
                {
                    cameraMethods.OnCameraPreRender -= OnCameraPreRender;
                }

                useOnPreRender = value;
            }
        }

        /// <summary>
        /// Amount of (extra) velocity to be applied to scroller 
        /// </summary>
        [SerializeField]
        [Tooltip("Amount of velocity to be applied to scroller")]
        [Range(0.0f, 2.0f)]
        private float velocityMultiplier = 0.8f;

        public float VelocityMultiplier
        {
            get => velocityMultiplier;
            set => velocityMultiplier = value;
        }

        /// <summary>
        /// Amount of falloff (drag) applied to velocity 
        /// </summary>
        /// <remarks>This can't be 0.0f since that won't allow ANY velocity, and it can't be 1.0f since that won't allow ANY drag.</remarks>
        [SerializeField]
        [Tooltip("Amount of falloff applied to velocity")]
        [Range(0.0001f, 0.9999f)]
        private float velocityFalloff = 0.90f;

        public float VelocityFalloff
        {
            get => velocityFalloff;
            set => velocityFalloff = value;
        }

        /// <summary>
        /// The desired type of velocity for the scroller 
        /// </summary>
        [SerializeField]
        [Tooltip("The desired type of velocity for the scroller")]
        private VelocityType typeOfVelocity;

        public VelocityType TypeOfVelocity
        {
            get => typeOfVelocity;
            set => typeOfVelocity = value;
        }

        /// <summary>
        /// Animation curve used to interpolate the pagination and movement methods
        /// </summary>
        [SerializeField]
        [Tooltip("Animation curve for pagination")]
        private AnimationCurve paginationCurve = new AnimationCurve(
                                                                    new Keyframe(0, 0),
                                                                    new Keyframe(1, 1));
        public AnimationCurve PaginationCurve
        {
            get => paginationCurve;
            set => paginationCurve = value;
        }

        /// <summary>
        /// The amount of time the <see cref="PaginationCurve"/> will take to evaulaute
        /// </summary>
        [SerializeField]
        [Tooltip("The amount of time the PaginationCurve will take to evaulaute")]
        private float animationLength = 0.25f;

        public float AnimationLength
        {
            get => (animationLength < 0) ? 0 : animationLength;
            set => animationLength = value;
        }

        /// <summary>
        /// Number of columns or rows in respect to <see cref="ViewableArea"/> and <see cref="ScrollDirection"/>
        /// </summary>
        [Tooltip("Number of columns or rows in respect to ViewableArea and ScrollDirection")]
        [SerializeField]
        [Range(1, 500)]
        private int tiers = 2;

        public int Tiers
        {
            get => (tiers > 0) ? tiers : 1;
            set => tiers = value;
        }

        /// <summary>
        /// Manual offset adjust the scale calculation of the <see cref="ClippingBox"/>
        /// </summary>
        [SerializeField]
        [Tooltip("Manual offset adjust the scale calculation of the ClippingBox")]
        private Vector3 occlusionScalePadding = new Vector3(0.0f, 0.0f, 0.001f);

        public Vector3 OcclusionScalePadding
        {
            get => occlusionScalePadding;
            set => occlusionScalePadding = value;
        }

        /// <summary>
        /// Manual offset adjust the position calculation of the <see cref="ClippingBox"/>
        /// </summary>
        [SerializeField]
        [Tooltip("Manual offset adjust the position calculation of the ClippingBox")]
        private Vector3 occlusionPositionPadding = Vector3.zero;

        public Vector3 OcclusionPositionPadding
        {
            get => occlusionPositionPadding;
            set => occlusionPositionPadding = value;
        }

        /// <summary>
        /// Width of the cell per object in the collection.
        /// </summary>
        [Tooltip("Width of cell per object")]
        [SerializeField]
        [Range(0.00001f, 100.0f)]
        private float cellWidth = 0.25f;

        public float CellWidth
        {
            get => (cellWidth > 0) ? cellWidth : 0.00001f;
            set => cellWidth = value;
        }

        /// <summary>
        /// Height of the cell per object in the collection.
        /// </summary>
        [Tooltip("Height of cell per object")]
        [SerializeField]
        [Range(0.00001f, 100.0f)]
        private float cellHeight = 0.25f;

        public float CellHeight
        {
            get => (cellHeight > 0) ? cellHeight : 0.00001f;
            set => cellHeight = value;
        }

        /// <summary>
        /// Forward vector for calculating the press plane of the <see cref="ScrollingObjectCollection"/>.
        /// </summary>
        /// <remarks>
        /// When using <see cref="PressableButton"/> or <see cref="NearInteractionTouchable"/> ensure this matches <see cref="NearInteractionTouchable.LocalForward"/>.
        /// </remarks>
        [SerializeField]
        [Tooltip("Forward vector for calculating the press plane of the ScrollingObjectCollection")]
        private AxisOrientationType collectionForward = AxisOrientationType.NegativeZ;

        public AxisOrientationType CollectionForward
        {
            get => collectionForward;
            set => collectionForward = value;
        }

        /// <summary>
        /// Multiplier to add more bounce to the overscroll of a list when using <see cref="VelocityType.FalloffPerFrame"/> or <see cref="VelocityType.FalloffPerItem"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("Multiplier to add more bounce to the overscroll of a list when using VelocityType.FalloffPerFrame or VelocityType.FalloffPerItem")]
        private float bounceMultiplier = 0.1f;

        public float BounceMultiplier
        {
            get => bounceMultiplier;
            set => bounceMultiplier = value;
        }

        /// <summary>
        /// The UnityEvent type the ScrollingObjectCollection sends 
        /// </summary>
        [System.Serializable]
        public class ScrollEvent : UnityEvent<GameObject> { }

        /// <summary>
        /// Event that is fired on the target object when the ScrollingObjectCollection deems event as a Click.
        /// </summary>
        [Tooltip("Event that is fired on the target object when the ScrollingObjectCollection deems event as a Click.")]
        public ScrollEvent ClickEvent = new ScrollEvent();

        /// <summary>
        /// Event that is fired on the target object when the ScrollingObjectCollection is touched.
        /// </summary>
        [Tooltip("Event that is fired on the target object when the ScrollingObjectCollection is touched.")]
        public ScrollEvent TouchStarted = new ScrollEvent();

        /// <summary>
        /// Event that is fired on the target object when the ScrollingObjectCollection is no longer touched.
        /// </summary>
        [Tooltip("Event that is fired on the target object when the ScrollingObjectCollection is no longer touched.")]
        public ScrollEvent TouchEnded = new ScrollEvent();

        /// <summary>
        /// Event that is fired on the target object when the ScrollingObjectCollection is no longer in motion from velocity
        /// </summary>
        [Tooltip("Event that is fired on the target object when the ScrollingObjectCollection is no longer in motion from velocity.")]
        public UnityEvent ListMomentumEnded = new UnityEvent();

        /// <summary>
        /// First item (visible) in the <see cref="ViewableArea"/>. 
        /// </summary>
        public int FirstItemInView
        {
            get
            {
                if (scrollDirection == ScrollDirectionType.UpAndDown)
                {
                    return (int)(workingScrollerPos.y / CellHeight) - ((int)(workingScrollerPos.y / CellHeight) % Tiers);
                }
                else
                {
                    return (int)(workingScrollerPos.x / CellWidth) - ((int)(workingScrollerPos.x / CellWidth) % Tiers);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private CameraEventRouter cameraMethods;

        //Half of a cell
        private Vector2 HalfCell;

        //Maximum amount the scroller can travel (vertically)
        private float maxY => NodeList.Count != 0 ? ((StepMultiplier(NodeList.Count, Tiers) + ModuloCheck(NodeList.Count, Tiers) - ViewableArea) * CellHeight) : 0.0f;

        //Minimum amount the scroller can travel (vertically) - this will always be zero. Here for readability
        private readonly float minY = 0.0f;

        //Maximum amount the scroller can travel (horizontally) - this will always be zero. Here for readability
        private readonly float maxX = 0.0f;

        //Minimum amount the scroller can travel (horizontally)
        private float minX => NodeList.Count != 0 ? -((StepMultiplier(NodeList.Count, Tiers) + ModuloCheck(NodeList.Count, Tiers) - ViewableArea) * CellWidth) : 0.0f;

        //item index for items that should be visible
        private int numItemsPrevView
        {
            get
            {
                if (scrollDirection == ScrollDirectionType.UpAndDown)
                {
                    return (int)Mathf.Ceil(scrollContainer.transform.localPosition.y / CellHeight) * Tiers;
                }
                else
                {
                    return -((int)Mathf.Ceil(scrollContainer.transform.localPosition.x / CellWidth) * Tiers);
                }
            }
        }

        //Take the previous view and then subtract the column remainder and we add the viewable area as a multiplier (minus 1 since the index is zero based).
        //The first item not viewable in the list
        private int numItemsPostView
        {
            get
            {
                if (scrollDirection == ScrollDirectionType.UpAndDown)
                {
                    return ((int)Mathf.Floor((scrollContainer.transform.localPosition.y + occlusionPositionPadding.y) / CellHeight) * Tiers) + (ViewableArea * Tiers) - 1;
                }
                else
                {
                    return ((int)Mathf.Floor((-(scrollContainer.transform.localPosition.x) + occlusionPositionPadding.x) / CellWidth) * Tiers) + (ViewableArea * Tiers) - 1;
                }

            }
        }

        //The empty game object that contains our nodes and be scrolled
        [SerializeField]
        [HideInInspector]
        private GameObject scrollContainer;

        //The empty game object that contains the ClipppingBox
        [SerializeField]
        [HideInInspector]
        private GameObject clippingObject;

        /// <summary>
        /// The empty <see cref="GameObject"/> containing the <see cref="ScrollingObjectCollection"/>'s <see cref="ClippingBox"/>.
        /// </summary>
        public GameObject ClippingObject => clippingObject;

        //A reference to the ClippingBox on the clippingObject
        [SerializeField]
        [HideInInspector]
        private ClippingBox clipBox;

        /// <summary>
        /// The <see cref="ScrollingObjectCollection"/>'s <see cref="ClippingBox"/> that is used for clipping items in and out of the list.
        /// </summary>
        public ClippingBox ClipBox => clipBox;

        //The bounds of the clipping object, this is to make helper math easier later, it doesn't matter that its AABB since we're really not using it for bounds operations
        [SerializeField]
        [HideInInspector]
        private Bounds clippingBounds;

        // Tracks whether an item in the list is being interacted with
        private bool isEngaged = false;

        // Tracks whether a movement action resulted in dragging the list  
        private bool isDragging = false;

        // we need to know if the pointer was a touch so we can do the threshold test (dot product test)
        private bool isTouched = false;

        //The postion of the scollContainer before we do any updating to it
        private Vector3 initialScrollerPos;

        //The new of the scollContainer before we've set the postion / finished the updateloop
        private Vector3 workingScrollerPos;

        //The node in the list currently being interacted with
        private GameObject focusedObject;

        //The Touchable in the list currently being interacted with
        private IMixedRealityTouchHandler scrollChild;

        //A list of new child nodes that have new child renderers that need to be added to the clippingBox
        private List<ObjectCollectionNode> nodesToClip = new List<ObjectCollectionNode>();

        //A list of new child nodes that have new child renderers that need to be removed to the clippingBox
        private List<ObjectCollectionNode> nodesToUnclip = new List<ObjectCollectionNode>();

        #region scroll state variables

        // The currently tracked finger/pointer objects
        private Transform currentPointerTransform;

        private Handedness currentHand;

        private IMixedRealityPointer currentPointer;

        //The point where the original PointerDown occured
        private Vector3 pointerHitPoint;

        //Helper to get the current (actual) position of the pointer
        private Vector3 currentPointerPos
        {
            get
            {
                if (currentPointer == null) { return Vector3.zero; } //bail

                if (currentPointer.GetType() == typeof(PokePointer))
                {
                    //this is a hand pointer
                    return UpdateFingerPosition(TrackedHandJoint.IndexTip, currentHand);
                }
                else
                {
                    return GetTrackedPoint(currentPointerTransform.position, pointerHitPoint, currentPointer.Position + currentPointer.Rotation * Vector3.forward, AxisOrientationType.PositiveY);
                }
            }
        }

        #endregion scroll state variables

        #region drag position calculation variables

        //The simple value scroll movement delta
        private Vector3 handDelta;

        //Absolute value scroll axis delta
        private float absAxisHandDelta;

        //Hand position when starting a motion
        private Vector3 initialHandPos;

        //Hand position previous frame
        private Vector3 lastHandPos;

        //the amount for finalOffset to travel
        [SerializeField]
        [HideInInspector]
        private float thresholdOffset;

        //A point along the collectionForward of the scroll container to calculate the threshold point
        [SerializeField]
        [HideInInspector]
        private Vector3 finalOffset = new Vector3();

        //A point in front of the scroller to use for dot product comparison
        [SerializeField]
        [HideInInspector]
        private Vector3 thresholdPoint = new Vector3();

        //Current time at initial press
        private float initialPressTime;

        #endregion drag position calculation variables

        #region velocity calculation variables

        //simple velocity of the scroller: current - last / timeDelta
        private float scrollVelocity = 0.0f;

        //Filtered weight of scroll velocity
        private float avgVelocity = 0.0f;

        //How much we should filter the velocity - yes this is a magic number. Its been tuned so lets leave it.
        private readonly float velocityFilterWeight = 0.97f;

        //Simple state enum to handle velocity logic
        private enum VelocityState
        {
            None = 0,
            Resolving,
            Calculating,
            Bouncing
        }

        //Internal enum for tracking the velocity state of the list
        private VelocityState velocityState = VelocityState.None;

        //Pre calculated destination with velocity and falloff when using per item snapping
        private Vector3 velocityDestinationPos;

        //velocity container for storing previous filtered velocity
        private float velocitySnapshot;

        #endregion velocity calculation variables

        //Whether a Animation CoRoutine is running / animating the scrollContainer
        private bool animatingToPosition = false;

        //the Animation CoRoutine
        private IEnumerator AnimateScroller;

        #region ObjectCollection methods

        /// <inheritdoc/>
        public override void UpdateCollection()
        {
            //Generate our scroll specific objects

            //scrollContainer empty game object null check - ensure its set up properly
            if (scrollContainer == null)
            {
                Transform oldContainer = transform.Find("Container");

                if (oldContainer != null)
                {
                    scrollContainer = oldContainer.gameObject;
                    Debug.LogWarning("Scrolling Object Collection found an existing Container object, using it for the list");
                }
                else
                {
                    scrollContainer = new GameObject();
                }

                scrollContainer.name = "Container";
                scrollContainer.transform.parent = transform;
                scrollContainer.transform.localPosition = Vector3.zero;
                scrollContainer.transform.localRotation = Quaternion.identity;
            }

            //ClippingObject empty game object null check - ensure its set up properly
            if (clippingObject == null)
            {
                Transform oldClippingObj = transform.Find("Clipping Bounds");

                if (oldClippingObj != null)
                {
                    clippingObject = oldClippingObj.gameObject;
                    Debug.LogWarning("Scrolling Object Collection found an existing Clipping object, using it for the list");
                }
                else
                {
                    clippingObject = new GameObject();
                }

                clippingObject.name = "Clipping Bounds";
                clippingObject.transform.parent = transform;
                clippingObject.transform.localRotation = Quaternion.identity;
                clippingObject.transform.localPosition = Vector3.zero;
            }

            //ClippingBox  component null check - ensure its set up properly
            if (clipBox == null)
            {
                clipBox = clippingObject.GetComponent<ClippingBox>();

                if (clipBox == null)
                {
                    clipBox = clippingObject.AddComponent<ClippingBox>();
                }

                clipBox.ClippingSide = ClippingPrimitive.Side.Outside;

                if (useOnPreRender)
                {
                    clipBox.UseOnPreRender = true;

                    //Subscribe to the preRender callback on the main camera so we can intercept it and make sure we catch
                    //any dynamically created children in our list
                    cameraMethods = CameraCache.Main.gameObject.EnsureComponent<CameraEventRouter>();
                    cameraMethods.OnCameraPreRender += OnCameraPreRender;
                }
            }

            //stash our children in a list so the count doesn't change or reverse the order if we were to count down
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child != scrollContainer.transform && child != clippingObject.transform)
                {
                    children.Add(child);
                }
            }

            //move any objects to the scrollContainer
            for (int i = 0; i < children.Count; i++)
            {
                children[i].parent = scrollContainer.transform;
            }

            // Check for empty nodes and remove them
            List<ObjectCollectionNode> emptyNodes = new List<ObjectCollectionNode>();

            for (int i = 0; i < NodeList.Count; i++)
            {
                //Make sure we respect our special scroll objects
                if (NodeList[i].Transform == null
                    || (IgnoreInactiveTransforms && !NodeList[i].Transform.gameObject.activeSelf)
                    || NodeList[i].Transform.parent == null
                    || !(NodeList[i].Transform.parent.gameObject == scrollContainer))
                {
                    emptyNodes.Add(NodeList[i]);
                }
            }

            // Now delete the empty nodes
            for (int i = 0; i < emptyNodes.Count; i++)
            {
                NodeList.Remove(emptyNodes[i]);
            }

            emptyNodes.Clear();

            // Check when children change and adjust
            for (int i = 0; i < scrollContainer.transform.childCount; i++)
            {
                Transform child = scrollContainer.transform.GetChild(i);

                if (!ContainsNode(child) && (child.gameObject.activeSelf || !IgnoreInactiveTransforms))
                {
                    NodeList.Add(new ObjectCollectionNode { Name = child.name, Transform = child, GameObject = child.gameObject, Colliders = child.GetComponentsInChildren<Collider>() });
                }
            }
            /*
            //TODO: Use a passthrough for objects that use input
            //Check for Pressable buttons and Interactables, then set passthrough mode
            //eventually we should be adding Passthrough to the IMixedRealityTouchHandler (maybe even all Input events)
            foreach (ObjectCollectionNode node in NodeList)
            {
                PressableButton pb = node.GameObject.GetComponentInChildren<PressableButton>();
                if (pb != null)
                {
                    pb.PassThroughMode = true;
                }

                Interactable ixble = node.GameObject.GetComponentInChildren<Interactable>();
                if (ixble != null)
                {
                    ixble.PassThroughMode = true;
                }
            }
            */
            if (NodeList.Count <= 0)
            {
                Debug.LogWarning(gameObject.name + " ScrollingObjectCollection needs a NodeList greater than zero");
                return;
            }

            switch (SortType)
            {
                case CollationOrder.ChildOrder:
                    NodeList.Sort((c1, c2) => (c1.Transform.GetSiblingIndex().CompareTo(c2.Transform.GetSiblingIndex())));
                    break;

                case CollationOrder.Alphabetical:
                    NodeList.Sort((c1, c2) => (string.CompareOrdinal(c1.Name, c2.Name)));
                    break;

                case CollationOrder.AlphabeticalReversed:
                    NodeList.Sort((c1, c2) => (string.CompareOrdinal(c1.Name, c2.Name)));
                    NodeList.Reverse();
                    break;

                case CollationOrder.ChildOrderReversed:
                    NodeList.Sort((c1, c2) => (c1.Transform.GetSiblingIndex().CompareTo(c2.Transform.GetSiblingIndex())));
                    NodeList.Reverse();
                    break;
            }

            LayoutChildren();

            OnCollectionUpdated?.Invoke(this);
        }

        /// <summary>
        /// Arranges our child objects in the scrollContainer per our set up instructions
        /// The layout method uses modulo with Columns / Rows
        /// </summary>
        protected override void LayoutChildren()
        {
            HalfCell = new Vector2(CellWidth * 0.5f, CellHeight * 0.5f);

            if (NodeList.Count <= 0)
            {
                Debug.LogWarning(gameObject.name + " ScrollingObjectCollection needs a NodeList greater than zero");
                return;
            }

            for (int i = 0; i < NodeList.Count; i++)
            {
                ObjectCollectionNode node = NodeList[i];

                Vector3 newPos;

                if (scrollDirection == ScrollDirectionType.UpAndDown)
                {
                    newPos.x = (ModuloCheck(i, Tiers) != 0) ? (ModuloCheck(i, Tiers) * CellWidth) + HalfCell.x : HalfCell.x;
                    newPos.y = ((StepMultiplier(i, Tiers) * CellHeight) + HalfCell.y) * -1;
                    newPos.z = 0.0f;

                }
                else //left or right
                {
                    newPos.x = (StepMultiplier(i, Tiers) * CellWidth) + HalfCell.x;
                    newPos.y = ((ModuloCheck(i, Tiers) != 0) ? (ModuloCheck(i, Tiers) * CellHeight) + HalfCell.y : HalfCell.y) * -1;
                    newPos.z = 0.0f;
                }
                node.Transform.localPosition = newPos;
                NodeList[i] = node;
            }

            ResolveLayout();
            HideItems();
        }

        /// <summary>
        /// Sets up the initial position of the scroller object as well as the configuration for the clippingBox
        /// </summary>
        private void ResolveLayout()
        {
            if (NodeList.Count <= 0)
            {
                Debug.LogWarning(gameObject.name + " ScrollingObjectCollection needs a NodeList greater than zero");
                return;
            }

            //temporarily turn on the first item in the list if its inactive
            bool resetActiveState = false;
            if (!NodeList[FirstItemInView].Transform.gameObject.activeSelf)
            {
                resetActiveState = true;
                NodeList[FirstItemInView].Transform.gameObject.SetActive(true);
            }

            //create the offset for our thesholdCalculation -- grab the first item in the list
            TryGetObjectAlignedBoundsSize(NodeList[FirstItemInView].Transform, collectionForward, out thresholdOffset);

            //get a point in front of the scrollContainer to use for the dot product check
            finalOffset = (AxisOrientationToTransformDirection(collectionForward) * -1.0f) * thresholdOffset;
            thresholdPoint = transform.InverseTransformPoint(transform.localPosition - finalOffset);

            //Use the first element for collection bounds -> for occluder positioning
            //temporarily zero out the rotation so we can get an accurate bounds
            Quaternion origRot = NodeList[FirstItemInView].Transform.rotation;
            NodeList[FirstItemInView].Transform.rotation = Quaternion.identity;

            clippingBounds.size = Vector3.zero;
            
            List<Vector3> boundsPoints = new List<Vector3>();
            BoundsExtensions.GetColliderBoundsPoints(NodeList[FirstItemInView].GameObject, boundsPoints, 0);

            clippingBounds.center = boundsPoints[0];

            foreach (Vector3 point in boundsPoints)
            {
                clippingBounds.Encapsulate(point);
            }

            // lets check whether the collection cell dimensions are a better fit
            // this prevents negative offset from ruining the scroll effect
            Vector3 tempClippingSize = clippingBounds.size;

            //put the rotation back
            NodeList[FirstItemInView].Transform.rotation = origRot;

            tempClippingSize.x = (tempClippingSize.x > CellWidth) ? tempClippingSize.x : CellWidth;
            tempClippingSize.y = (tempClippingSize.y > CellHeight) ? tempClippingSize.y : CellHeight;

            clippingBounds.size = tempClippingSize;
            clippingBounds.center = clippingBounds.size * 0.5f;

            //set the first item back to its original state
            if (resetActiveState)
            {
                NodeList[FirstItemInView].Transform.gameObject.SetActive(false);
            }

            Vector3 viewableCenter = new Vector3();

            //Adjust scale and position of our clipping box
            switch (scrollDirection)
            {
                case ScrollDirectionType.UpAndDown:
                default:

                    //apply the viewable area and column/row multiplier
                    //use a dummy bounds of one to get the local scale to match;
                    clippingBounds.size = new Vector3((clippingBounds.size.x * Tiers), (clippingBounds.size.y * ViewableArea), clippingBounds.size.z);
                    clipBox.transform.localScale = CalculateScale(new Bounds(Vector3.zero, Vector3.one), clippingBounds, OcclusionScalePadding);

                    //adjust where the center of the clipping box is
                    viewableCenter.x = (clipBox.transform.localScale.x * 0.5f) - (OcclusionScalePadding.x * 0.5f) + OcclusionPositionPadding.x;
                    viewableCenter.y = (((clipBox.transform.localScale.y * 0.5f) - (OcclusionScalePadding.y * 0.5f)) * -1) + OcclusionPositionPadding.y;
                    viewableCenter.z = OcclusionPositionPadding.z;
                    break;

                case ScrollDirectionType.LeftAndRight:

                    //Same as above for L <-> R
                    clippingBounds.size = new Vector3(clippingBounds.size.x * ViewableArea, clippingBounds.size.y * Tiers, clippingBounds.size.z);
                    clipBox.transform.localScale = CalculateScale(new Bounds(Vector3.zero, Vector3.one), clippingBounds) + OcclusionScalePadding;

                    //Same as above for L <-> R
                    viewableCenter.x = (clipBox.transform.localScale.x * 0.5f) - (OcclusionScalePadding.x * 0.5f) + OcclusionPositionPadding.x;
                    viewableCenter.y = ((clipBox.transform.localScale.y * 0.5f) - (OcclusionScalePadding.y * 0.5f) + OcclusionPositionPadding.y) * -1.0f;
                    viewableCenter.z = OcclusionPositionPadding.z;
                    break;
            }

            //Apply our new values
            clipBox.transform.localPosition = viewableCenter;

            //add our objects to the clippingBox queue
            AddAllItemsToClippingObject();
        }

        #endregion ObjectCollection methods

        #region Monobehavior Implementation

        private void OnEnable()
        {
            if (useOnPreRender)
            {
                if (clippingObject == null || clipBox == null)
                {
                    UpdateCollection();
                }

                clipBox.UseOnPreRender = true;

                //Subscribe to the preRender callback on the main camera so we can intercept it and make sure we catch
                //any dynamically created children in our list
                cameraMethods = CameraCache.Main.gameObject.EnsureComponent<CameraEventRouter>();
                cameraMethods.OnCameraPreRender += OnCameraPreRender;
            }
        }

        private void Start()
        {
            if (setUpAtRuntime)
            {
                UpdateCollection();
            }

            if (useOnPreRender && clipBox != null)
            {
                clipBox.UseOnPreRender = true;

                //Subscribe to the preRender callback on the main camera so we can intercept it and make sure we catch
                //any dynamically created children in our list
                cameraMethods = CameraCache.Main.gameObject.EnsureComponent<CameraEventRouter>();
                cameraMethods.OnCameraPreRender += OnCameraPreRender;
            }
        }

        private void Update()
        {
            //early bail, the component isn't set up properly
            if (scrollContainer == null) { return; }

            bool nodeLengthCheck = NodeList.Count > (ViewableArea * Tiers);

            // Force the position if the total number of items in the list is less than the scrollable area
            if (!nodeLengthCheck)
            {
                workingScrollerPos = Vector3.zero;
                CalculateDragMove(workingScrollerPos);
            }

            //The scroller has detected input and has a valid pointer
            if (isEngaged)
            {
                handDelta = initialHandPos - currentPointerPos;

                //Lets see if this is gonna be a click or a drag
                //Check the var's state to prevent resetting calculation
                if (!isDragging && nodeLengthCheck)
                {
                    //grab the delta value we care about
                    absAxisHandDelta = (scrollDirection == ScrollDirectionType.UpAndDown) ? Mathf.Abs(handDelta.y) : Mathf.Abs(handDelta.x);

                    //Catch an intentional finger in scroller to stop momentum, this isn't a drag its definitely a stop
                    if (absAxisHandDelta > (handDeltaMagThreshold * 0.1f) || TimeTest(initialPressTime, Time.time, dragTimeThreshold))
                    {
                        scrollVelocity = 0.0f;
                        avgVelocity = 0.0f;

                        isDragging = true;
                        velocityState = VelocityState.None;

                        //reset initialHandPos to prevent the scroller from jumping
                        initialScrollerPos = scrollContainer.transform.localPosition;
                        initialHandPos = currentPointerPos;
                    }
                }

                //get a point in front of the scrollContainer to use for the dot product check
                finalOffset = (AxisOrientationToTransformDirection(collectionForward) * -1.0f) * thresholdOffset;
                thresholdPoint = transform.InverseTransformPoint(transform.localPosition - finalOffset);

                //Make sure we're actually (near) touched and not a pointer event, do a dot product check
                if (isTouched && DetectScrollRelease((AxisOrientationToTransformDirection(collectionForward) * 1), thresholdPoint, currentPointerPos, clippingObject.transform, transform.worldToLocalMatrix, scrollDirection))
                {
                    //We're on the other side of the original touch position. This is a release.
                    if (!isDragging)
                    {
                        //Its a click release
                        Collider[] c = focusedObject.GetComponentsInChildren<Collider>();
                        bool isColliderActive = false;

                        foreach (Collider col in c)
                        {
                            if (col.enabled)
                            {
                                isColliderActive = true;
                                break;
                            }
                        }

                        if (isColliderActive)
                        {
                            //Fire the UnityEvent
                            //TODO: Use existing InputInterfaces to make a call back to the child object
                            ClickEvent?.Invoke(focusedObject);
                        }
                    }
                    else
                    {
                        //Its a drag release
                        initialScrollerPos = workingScrollerPos;
                        velocityState = VelocityState.Calculating;
                    }

                    //Release the pointer
                    if (currentPointer.GetType() != typeof(PokePointer))
                    {
                        currentPointer.IsFocusLocked = false;
                        currentPointer = null;
                    }

                    //Let everyone know the scroller is no longer engaged
                    TouchEnded?.Invoke(focusedObject);

                    focusedObject = null;
                    scrollChild = null;

                    //Clear our states
                    isTouched = false;
                    isEngaged = false;
                    isDragging = false;

                }
                else if (isDragging)
                {

                    if (scrollDirection == ScrollDirectionType.UpAndDown)
                    {
                        //Lock X, clamp Y
                        workingScrollerPos.y = SoftClamp(initialScrollerPos.y - handDelta.y, minY, maxY, 0.5f);
                        workingScrollerPos.x = 0.0f;
                    }
                    else
                    {
                        //Lock Y, clamp X
                        workingScrollerPos.x = SoftClamp(initialScrollerPos.x - handDelta.x, minX, maxX, 0.5f);
                        workingScrollerPos.y = 0.0f;
                    }

                    //Update the scrollContainer Position
                    CalculateDragMove(workingScrollerPos);

                    CalculateVelocity();

                    //Update the prev val for velocity
                    lastHandPos = currentPointerPos;
                }
            }
            else if (!animatingToPosition && nodeLengthCheck)//Prevent the Animation coroutine from being overridden
            {
                //We're not engaged, so handle any not touching behavior
                HandleVeloctyFalloff();

                //Apply our position
                CalculateDragMove(workingScrollerPos);
            }
        }

        private void LateUpdate()
        {
            //Hide the items not in view
            HideItems();
        }

        private void OnDisable()
        {
            if (useOnPreRender && cameraMethods != null)
            {
                CameraEventRouter cameraMethods = CameraCache.Main.gameObject.EnsureComponent<CameraEventRouter>();
                cameraMethods.OnCameraPreRender -= OnCameraPreRender;
            }
        }

        #endregion Monobehavior Implementation

        #region private methods

        /// <summary>
        /// When <see cref="UseOnPreRender"/>, the <see cref="ScrollingObjectCollection"/> subscribes to the <see cref="CameraEventRouter"/> call back for OnCameraPreRender
        /// </summary>
        /// <param name="router">The active <see cref="CameraEventRouter"/> on the camera.</param>

        private void OnCameraPreRender(CameraEventRouter router)
        {
            //clip any new items that may have shown up
            if (nodesToClip.Count > 0)
            {
                AddItemsToClippingObject(nodesToClip);

                //reset the list for next time
                nodesToClip.Clear();
            }

            //unclip any new items that may have shown up
            if (nodesToUnclip.Count > 0)
            {
                RemoveItemsFromClippingObject(nodesToUnclip);

                //reset the list for next time
                nodesToUnclip.Clear();
            }

        }

        /// <summary>
        /// Calculates our <see cref="VelocityType"/> falloff
        /// </summary>
        private void HandleVeloctyFalloff()
        {
            switch (typeOfVelocity)
            {
                case VelocityType.NoVeloctySnapToItem:
                    velocityState = VelocityState.None;

                    if (Mathf.Abs(avgVelocity) > Mathf.Epsilon)
                    {
                        avgVelocity = 0.0f;

                        //Round to the nearest list item
                        if (scrollDirection == ScrollDirectionType.UpAndDown)
                        {
                            workingScrollerPos.y = ((int)(scrollContainer.transform.localPosition.y / CellHeight)) * CellHeight;
                        }
                        else
                        {
                            workingScrollerPos.x = ((int)(scrollContainer.transform.localPosition.x / CellWidth)) * CellWidth;
                        }

                        initialScrollerPos = workingScrollerPos;
                        ListMomentumEnded?.Invoke();
                    }
                    break;

                case VelocityType.None:

                    velocityState = VelocityState.None;

                    if (Mathf.Abs(avgVelocity) > Mathf.Epsilon)
                    {
                        //apply no velocity
                        avgVelocity = 0.0f;
                        ListMomentumEnded?.Invoke();
                    }
                    break;

                case VelocityType.FalloffPerItem:

                    switch (velocityState)
                    {
                        case VelocityState.Calculating:

                            // The logarithmic formula to get the number of steps from "average velocity" with drag to "zero" -> can't figure out how to get the proper distance though.
                            // Saving for later, use IterateFalloff() for now...
                            //float numSteps = (Mathf.Log(0.00001f) - Mathf.Log(Mathf.Abs(avgVelocity))) / Mathf.Log(velocityFalloff);

                            int numSteps;
                            float newPosAfterVelocity;
                            if (scrollDirection == ScrollDirectionType.UpAndDown)
                            {
                                if (avgVelocity == 0.0f)
                                {
                                    //velocity was cleared out so we should just snap
                                    newPosAfterVelocity = scrollContainer.transform.localPosition.y;
                                }
                                else
                                {
                                    //calculate velocity one more time to prevent any sort of edge case where we didn't have enough frames to calculate properly
                                    CalculateVelocity();

                                    //Precalculate where the velocity falloff WOULD land our scrollContainer, then round it to the nearest item so it feels "natural"
                                    velocitySnapshot = IterateFalloff(avgVelocity, out numSteps);
                                    newPosAfterVelocity = initialScrollerPos.y + velocitySnapshot;
                                }

                                velocityDestinationPos.y = ((int)(newPosAfterVelocity / CellHeight)) * CellHeight;

                                velocityState = VelocityState.Resolving;
                            }
                            else
                            {
                                if (avgVelocity == 0.0f)
                                {
                                    //velocity was cleared out so we should just snap
                                    newPosAfterVelocity = scrollContainer.transform.localPosition.x;
                                }
                                else
                                {
                                    //calculate velocity one more time to prevent any sort of edge case where we didn't have enough frames to calculate properly
                                    CalculateVelocity();

                                    //Precalculate where the velocity falloff WOULD land our scrollContainer, then round it to the nearest item so it feels "natural"
                                    velocitySnapshot = IterateFalloff(avgVelocity, out numSteps);
                                    newPosAfterVelocity = initialScrollerPos.x + velocitySnapshot;
                                }

                                velocityDestinationPos.x = ((int)(newPosAfterVelocity / CellWidth)) * CellWidth;

                                velocityState = VelocityState.Resolving;
                            }

                            workingScrollerPos = SmoothTo(scrollContainer.transform.localPosition, velocityDestinationPos, Time.deltaTime, 0.9275f);
                            //Clear the velocity now that we've applied a new position
                            avgVelocity = 0.0f;
                            break;

                        case VelocityState.Resolving:

                            if (scrollDirection == ScrollDirectionType.UpAndDown)
                            {
                                if (scrollContainer.transform.localPosition.y > maxY + (thresholdOffset * bounceMultiplier)
                                    || scrollContainer.transform.localPosition.y < minY - (thresholdOffset * bounceMultiplier))
                                {
                                    velocityState = VelocityState.Bouncing;
                                    velocitySnapshot = 0.0f;
                                    break;
                                }
                                else
                                {
                                    workingScrollerPos = SmoothTo(scrollContainer.transform.localPosition, velocityDestinationPos, Time.deltaTime, 0.9275f);

                                    if (Vector3.Distance(scrollContainer.transform.localPosition, workingScrollerPos) < 0.00001f)
                                    {
                                        //Ensure we've actually snapped the position to prevent an extreme in-between state
                                        workingScrollerPos.y = ((int)(scrollContainer.transform.localPosition.y / CellHeight)) * CellHeight;

                                        velocityState = VelocityState.None;

                                        ListMomentumEnded?.Invoke();

                                        // clean up our position for next frame
                                        initialScrollerPos = workingScrollerPos;
                                    }
                                }
                            }
                            else
                            {
                                if (scrollContainer.transform.localPosition.x > maxX + (thresholdOffset * bounceMultiplier)
                                    || scrollContainer.transform.localPosition.x < minX - (thresholdOffset * bounceMultiplier))
                                {
                                    velocityState = VelocityState.Bouncing;
                                    velocitySnapshot = 0.0f;
                                    break;
                                }
                                else
                                {
                                    workingScrollerPos = SmoothTo(scrollContainer.transform.localPosition, velocityDestinationPos, Time.deltaTime, 0.9275f);

                                    if (Vector3.Distance(scrollContainer.transform.localPosition, workingScrollerPos) < 0.00001f)
                                    {
                                        //Ensure we've actually snapped the position to prevent an extreme in-between state
                                        workingScrollerPos.y = ((int)(scrollContainer.transform.localPosition.x / CellWidth)) * CellWidth;

                                        velocityState = VelocityState.None;

                                        ListMomentumEnded?.Invoke();

                                        // clean up our position for next frame
                                        initialScrollerPos = workingScrollerPos;
                                    }
                                }
                            }
                            break;

                        case VelocityState.Bouncing:
                            bool smooth = false;
                            if (Vector3.Distance(scrollContainer.transform.localPosition, workingScrollerPos) < 0.00001f)
                            {
                                smooth = true;
                            }
                            if (scrollDirection == ScrollDirectionType.UpAndDown
                                && (scrollContainer.transform.localPosition.y - minY > -0.00001
                                && scrollContainer.transform.localPosition.y - maxY < 0.00001f))
                            {
                                velocityState = VelocityState.None;

                                ListMomentumEnded?.Invoke();

                                // clean up our position for next frame
                                initialScrollerPos = workingScrollerPos;
                            }
                            else if (scrollDirection == ScrollDirectionType.LeftAndRight
                                     && (scrollContainer.transform.localPosition.x + minX > -0.00001
                                     && scrollContainer.transform.localPosition.x - maxX < 0.00001f))
                            {
                                velocityState = VelocityState.None;

                                ListMomentumEnded?.Invoke();

                                // clean up our position for next frame
                                initialScrollerPos = workingScrollerPos;
                            }
                            else
                            {
                                smooth = true;
                            }

                            if (smooth)
                            {
                                //Debug.Log("bounce calc");
                                Vector3 clampedDest = new Vector3(Mathf.Clamp(scrollContainer.transform.localPosition.x, minX, maxX), Mathf.Clamp(scrollContainer.transform.localPosition.y, minY, maxY), 0.0f);
                                workingScrollerPos.y = SmoothTo(scrollContainer.transform.localPosition, clampedDest, Time.deltaTime, 0.2f).y;
                                workingScrollerPos.x = SmoothTo(scrollContainer.transform.localPosition, clampedDest, Time.deltaTime, 0.2f).x;
                            }
                            break;

                        case VelocityState.None:
                        default:
                            // clean up our position for next frame
                            initialScrollerPos = workingScrollerPos;
                            break;
                    }
                    break;

                case VelocityType.FalloffPerFrame:
                default:

                    switch (velocityState)
                    {
                        case VelocityState.Calculating:

                            if (scrollDirection == ScrollDirectionType.UpAndDown)
                            {
                                workingScrollerPos.y = initialScrollerPos.y + avgVelocity;
                            }
                            else
                            {
                                workingScrollerPos.x = initialScrollerPos.x + avgVelocity;
                            }

                            velocityState = VelocityState.Resolving;

                            // clean up our position for next frame
                            initialScrollerPos = workingScrollerPos;
                            break;

                        case VelocityState.Resolving:

                            if (scrollDirection == ScrollDirectionType.UpAndDown)
                            {
                                if (scrollContainer.transform.localPosition.y > maxY + (thresholdOffset * bounceMultiplier)
                                    || scrollContainer.transform.localPosition.y < minY - (thresholdOffset * bounceMultiplier))
                                {
                                    velocityState = VelocityState.Bouncing;
                                    avgVelocity = 0.0f;
                                    break;
                                }
                                else
                                {
                                    avgVelocity *= velocityFalloff;
                                    workingScrollerPos.y = initialScrollerPos.y + avgVelocity;

                                    if (Vector3.Distance(scrollContainer.transform.localPosition, workingScrollerPos) < 0.00001f)
                                    {
                                        velocityState = VelocityState.None;
                                        avgVelocity = 0.0f;

                                        ListMomentumEnded?.Invoke();
                                    }
                                }
                            }
                            else
                            {
                                if (scrollContainer.transform.localPosition.x > maxX + (thresholdOffset * bounceMultiplier)
                                    || scrollContainer.transform.localPosition.x < minX - (thresholdOffset * bounceMultiplier))
                                {
                                    velocityState = VelocityState.Bouncing;
                                    avgVelocity = 0.0f;
                                    break;
                                }
                                else
                                {

                                    avgVelocity *= velocityFalloff;
                                    workingScrollerPos.x = initialScrollerPos.x + avgVelocity;

                                    if (Vector3.Distance(scrollContainer.transform.localPosition, workingScrollerPos) < 0.00001f)
                                    {
                                        velocityState = VelocityState.None;
                                        avgVelocity = 0.0f;

                                        ListMomentumEnded?.Invoke();
                                    }
                                }
                            }

                            // clean up our position for next frame
                            initialScrollerPos = workingScrollerPos;

                            break;

                        case VelocityState.Bouncing:

                            bool smooth = false;

                            if (Vector3.Distance(scrollContainer.transform.localPosition, workingScrollerPos) < 0.00001f)
                            {
                                smooth = true;
                            }
                            if (scrollDirection == ScrollDirectionType.UpAndDown
                            && (scrollContainer.transform.localPosition.y - minY > -0.00001
                            && scrollContainer.transform.localPosition.y - maxY < 0.00001f))
                            {
                                velocityState = VelocityState.None;

                                ListMomentumEnded?.Invoke();

                                // clean up our position for next frame
                                initialScrollerPos = workingScrollerPos;
                            }
                            else if (scrollDirection == ScrollDirectionType.LeftAndRight
                                     && (scrollContainer.transform.localPosition.x + minX > -0.00001
                                     && scrollContainer.transform.localPosition.x - maxX < 0.00001f))
                            {
                                velocityState = VelocityState.None;

                                ListMomentumEnded?.Invoke();

                                // clean up our position for next frame
                                initialScrollerPos = workingScrollerPos;
                            }
                            else
                            {
                                smooth = true;
                            }

                            if (smooth)
                            {
                                Vector3 clampedDest = new Vector3(Mathf.Clamp(scrollContainer.transform.localPosition.x, minX, maxX), Mathf.Clamp(scrollContainer.transform.localPosition.y, minY, maxY), 0.0f);
                                workingScrollerPos.y = SmoothTo(scrollContainer.transform.localPosition, clampedDest, Time.deltaTime, 0.2f).y;
                                workingScrollerPos.x = SmoothTo(scrollContainer.transform.localPosition, clampedDest, Time.deltaTime, 0.2f).x;
                            }
                            break;
                    }

                    break;
            }

            if (velocityState == VelocityState.None)
            {
                workingScrollerPos.y = Mathf.Clamp(workingScrollerPos.y, minY, maxY);
                workingScrollerPos.x = Mathf.Clamp(workingScrollerPos.x, minX, maxX);
            }
        }

        /// <summary>
        /// Clamps via a lerp for a "soft" clamp effect
        /// </summary>
        /// <param name="pos">number to clamp</param>
        /// <param name="min">if <see cref="pos"/> is less, clamps to this value</param>
        /// <param name="max">if <see cref="pos"/> is more, clamps to this value</param>
        /// <param name="clampFactor"> Range from 0.0f to 1.0f of how close to snap to <see cref="min"/> and <see cref="max"/></param>
        /// <returns>A soft clamped value</returns>
        private float SoftClamp(float pos, float min, float max, float clampFactor)
        {
            clampFactor = Mathf.Clamp(clampFactor, 0.0f, 1.0f);
            if (pos < min)
            {
                return Mathf.Lerp(pos, min, clampFactor);
            }
            else if (pos > max)
            {
                return Mathf.Lerp(pos, max, clampFactor);
            }

            return pos;
        }

        /// <summary>
        /// Wrapper for per frame velocity calculation
        /// </summary>
        private void CalculateVelocity()
        {
            //update simple velocity
            scrollVelocity = (scrollDirection == ScrollDirectionType.UpAndDown)
                             ? (currentPointerPos.y - lastHandPos.y) / Time.deltaTime * (velocityMultiplier * 0.01f)
                             : (currentPointerPos.x - lastHandPos.x) / Time.deltaTime * (velocityMultiplier * 0.01f);

            //And filter it...
            avgVelocity = (avgVelocity * (1.0f - velocityFilterWeight)) + (scrollVelocity * velocityFilterWeight);
        }

        /// <summary>
        /// The Animation Override to position our scroller based on manual movement <see cref="PageBy(int, bool)"/>, <see cref="MoveTo(int, bool)"/>,
        /// <see cref="MoveByItems(int, bool)"/>, or <see cref="MoveByTiers(int, bool)"/>
        /// </summary>
        /// <param name="initialPos">The start position of the scrollContainer</param>
        /// <param name="finalPos">Where we want the scrollContainer to end up, typically this should be <see cref="workingScrollerPos"/></param>
        /// <param name="curve"><see cref="AnimationCurve"/> representing the easing desired</param>
        /// <param name="time">Time for animation, in seconds</param>
        /// <param name="callback">Optional callback action to be invoked after animation coroutine has finished</param>
        /// <returns></returns>
        private IEnumerator AnimateTo(Vector3 initialPos, Vector3 finalPos, AnimationCurve curve = null, float? time = null, System.Action callback = null)
        {
            animatingToPosition = true;
            velocityState = VelocityState.None;

            if (curve == null)
            {
                curve = paginationCurve;
            }

            if (time == null)
            {
                time = animationLength;
            }

            float counter = 0.0f;
            while (counter <= time)
            {
                workingScrollerPos = Vector3.Lerp(initialPos, finalPos, curve.Evaluate(counter / (float)time));
                scrollContainer.transform.localPosition = workingScrollerPos;

                counter += Time.deltaTime;
                yield return null;
            }

            //update our values so they stick

            if (scrollDirection == ScrollDirectionType.UpAndDown)
            {
                workingScrollerPos.y = initialScrollerPos.y = ((int)(scrollContainer.transform.localPosition.y / CellHeight)) * CellHeight;
            }
            else
            {
                workingScrollerPos.x = initialScrollerPos.x = ((int)(scrollContainer.transform.localPosition.x / CellWidth)) * CellWidth;
            }

            animatingToPosition = false;

            if (callback != null)
            {
                callback?.Invoke();
            }
        }

        /// <summary>
        /// Checks to see if the engaged joint is a release
        /// </summary>
        /// <returns><see cref="true"/> if released</returns>
        private static bool DetectScrollRelease(Vector3 initialDirection, Vector3 initialPosition, Vector3 pointToCompare, Transform clippingObj, Matrix4x4 transformMatrix, ScrollDirectionType direction)
        {
            //true if finger is on the other side (Z) of the initial contact point of the collection
            if (TouchPassedThreshold(initialDirection, initialPosition, pointToCompare))
            {
                return true;
            }

            bool hasPassedBoundry;
            Vector3 posToClip = clippingObj.localPosition - transformMatrix.MultiplyPoint3x4(pointToCompare);
            Vector3 halfScale = clippingObj.localScale * 0.5f;

            if (direction == ScrollDirectionType.UpAndDown)
            {
                hasPassedBoundry = (posToClip.y > halfScale.y || posToClip.y < -halfScale.y) ? true : false;
            }
            else
            {
                hasPassedBoundry = (posToClip.x > halfScale.x || posToClip.x < -halfScale.x) ? true : false;
            }

            return hasPassedBoundry;
        }

        /// <summary>
        /// Grab all child renderers in each node from NodeList and add them to the ClippingBox
        /// </summary>
        private void AddAllItemsToClippingObject()
        {
            //Register all of the renderers to be clipped by the clippingBox
            foreach (ObjectCollectionNode node in NodeList)
            {
                Renderer[] childRends = node.Transform.gameObject.transform.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < childRends.Length; i++)
                {
                    clipBox.AddRenderer(childRends[i]);
                }
            }
        }

        /// <summary>
        /// Grab the child renderers from a specified node from NodeList and add them to the ClippingBox
        /// </summary>
        private void AddItemsToClippingObject(ObjectCollectionNode node)
        {
            //Register all of the renderers to be clipped by the clippingBox
            Renderer[] childRends = node.Transform.gameObject.transform.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < childRends.Length; i++)
            {
                clipBox.AddRenderer(childRends[i]);
            }
        }

        /// <summary>
        /// Grab all child renderers in a list of nodes from NodeList and add them to the ClippingBox
        /// </summary>
        private void AddItemsToClippingObject(List<ObjectCollectionNode> nodes)
        {
            foreach (ObjectCollectionNode node in nodes)
            {
                //Register all of the renderers to be clipped by the clippingBox
                Renderer[] childRends = node.Transform.gameObject.transform.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < childRends.Length; i++)
                {
                    clipBox.AddRenderer(childRends[i]);
                }
            }
        }

        /// <summary>
        /// Grab all child renderers in each node from NodeList and remove them to the ClippingBox
        /// </summary>
        private void RemoveAllItemsFromClippingObject()
        {
            //Register all of the renderers to be clipped by the clippingBox
            foreach (ObjectCollectionNode node in NodeList)
            {
                Renderer[] childRends = node.Transform.gameObject.transform.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < childRends.Length; i++)
                {
                    clipBox.RemoveRenderer(childRends[i]);
                }
            }
        }

        /// <summary>
        /// Grab the child renderers from a specified node from NodeList and remove them to the ClippingBox
        /// </summary>
        private void RemoveItemsFromClippingObject(ObjectCollectionNode node)
        {
            //Register all of the renderers to be clipped by the clippingBox
            Renderer[] childRends = node.Transform.gameObject.transform.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < childRends.Length; i++)
            {
                clipBox.RemoveRenderer(childRends[i]);
            }
        }

        /// <summary>
        /// Grab all child renderers in a list of nodes from NodeList and remove them to the ClippingBox
        /// </summary>
        private void RemoveItemsFromClippingObject(List<ObjectCollectionNode> nodes)
        {
            foreach (ObjectCollectionNode node in nodes)
            {
                //Register all of the renderers to be clipped by the clippingBox
                Renderer[] childRends = node.Transform.gameObject.transform.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < childRends.Length; i++)
                {
                    clipBox.RemoveRenderer(childRends[i]);
                }
            }
        }

        /// <summary>
        /// Helper to get the remainder from an itemindex in the list in relation to rows/columns
        /// </summary>
        /// <param name="itemIndex">Index of node item in <see cref="BaseObjectCollection.NodeList"/> to be compared</param>
        /// <param name="divisor">Rows / Columns</param>
        /// <returns>The remainder from the divisor</returns>
        public static int ModuloCheck(int itemIndex, int divisor)
        {
            //prevent divide by 0
            return (divisor > 0) ? itemIndex % divisor : 0;
        }

        /// <summary>
        /// Helper to get the number of rows / columns deep the item is
        /// </summary>
        /// <param name="itemIndex">Index of node item in <see cref="BaseObjectCollection.NodeList"/> to be compared</param>
        /// <param name="divisor">Rows / Columns</param>
        /// <returns>The multiplier to get the row / column index the item is in</returns>
        public static int StepMultiplier(int itemIndex, int divisor)
        {
            //prevent divide by 0
            return (divisor != 0) ? itemIndex / divisor : 0;
        }

        /// <summary>
        /// Iterates the <see cref="BaseObjectCollection.NodeList"/> to determine which <see cref="ObjectCollectionNode"/>s needs to be
        /// disabled (<see cref="GameObject.SetActive(bool)"/>) and their <see cref="Collider"/> disabled.
        /// </summary>
        /// <remarks>When <see cref="useOnPreRender"/> is set to <see cref="true"/>, <see cref="HideItems"/> will populate a list of <see cref="ObjectCollectionNode"/>
        /// to be added to the <see cref="ClipBox"/>.</remarks>
        private void HideItems()
        {
            //Early Bail - our list is empty
            if (NodeList.Count == 0) { return; }

            //Stash the values from numItems to cut down on redundant calculations
            int prevItems = numItemsPrevView;
            int postItems = numItemsPostView;

            int listLength = NodeList.Count;
            int col = Tiers;

            for (int i = 0; i < listLength; i++)
            {
                ObjectCollectionNode node = NodeList[i];
                //hide the items that have no chance of being seen
                if (i < prevItems - col || i > postItems + col)
                {
                    //quick check to cut down on the redundant calls
                    if (node.GameObject.activeSelf)
                    {
                        node.GameObject.SetActive(false);
                    }

                    //the node is inactive, and has been previously clipped, make sure we remove it from the rendering list, this keeps the calls in ClippingBox as small as possible
                    if (node.isClipped)
                    {
                        nodesToUnclip.Add(NodeList[i]);
                        node.isClipped = false;
                    }
                }
                else
                {
                    //Disable colliders on items that will be scrolling in and out of view
                    if (i < prevItems || i > postItems)
                    {
                        foreach (Collider c in node.Colliders)
                        {
                            c.enabled = false;
                        }
                    }
                    else
                    {
                        foreach (Collider c in node.Colliders)
                        {
                            c.enabled = true;
                        }
                    }

                    //Otherwise show the item
                    if (!node.GameObject.activeSelf)
                    {
                        node.GameObject.SetActive(true);
                    }

                    //the node has a new item, send it to the clipping box and see if its a renderer
                    if (!node.isClipped)
                    {
                        nodesToClip.Add(node);
                        node.isClipped = true;
                    }
                }
            }
        }

        /// <summary>
        /// Precalculates the total amount of travel given the scroller's current average velocity and drag.
        /// </summary>
        /// <param name="steps"><see cref="out"/> Number of steps to get our <see cref="avgVelocity"/> to effectively "zero" (0.00001).</param>
        /// <returns>The total distance the <see cref="avgVelocity"/> with <see cref="velocityFalloff"/> as drag would travel.</returns>
        private float IterateFalloff(float vel, out int steps)
        {
            //float numSteps = (Mathf.Log(0.00001f)  - Mathf.Log(Mathf.Abs(avgVelocity))) / Mathf.Log(velocityFalloff);

            float newVal = 0.0f;
            float v = vel;
            steps = 0;

            while (Mathf.Abs(v) > 0.00001)
            {
                v *= velocityFalloff;
                newVal += v;
                steps++;
            }

            return newVal;
        }

        /// <summary>
        /// Applies <paramref name="workingPos"/> to the <see cref="Transform.localPosition"/> of our <see cref="scrollContainer"/>
        /// </summary>
        /// <param name="workingPos">The new desired position for <see cref="scrollContainer"/> in local space</param>
        private void CalculateDragMove(Vector3 workingPos)
        {
            Vector3 newScrollPos;

            switch (scrollDirection)
            {
                case ScrollDirectionType.UpAndDown:
                default:

                    newScrollPos = new Vector3(scrollContainer.transform.localPosition.x, workingPos.y, 0.0f);
                    break;

                case ScrollDirectionType.LeftAndRight:

                    newScrollPos = new Vector3(workingPos.x, scrollContainer.transform.localPosition.y, 0.0f);

                    break;
            }
            scrollContainer.transform.localPosition = newScrollPos;
        }

        /// <summary>
        /// Iterates over a list of <see cref="IMixedRealityPointer"/> to find a <see cref="PokePointer"/>
        /// </summary>
        /// <param name="pointers"> List of <see cref="IMixedRealityPointer"/> to iterate</param>
        /// <param name="pokePointer"><see cref="out"/> valid <see cref="PokePointer"/> as <see cref="IMixedRealityPointer"/></param>
        /// <returns><see cref="true"/> when a <see cref="PokePointer"/> is found</returns>
        private bool TryGetPokePointer(IMixedRealityPointer[] pointers, out IMixedRealityPointer pokePointer)
        {
            pokePointer = null;

            for (int i = 0; i < pointers.Length; i++)
            {
                if (pointers[i].GetType() == typeof(PokePointer))
                {
                    pokePointer = pointers[i];
                }
            }

            return (pokePointer != null) ? true : false;
        }

        /// <summary>
        /// Helper get near hand joint in world space
        /// </summary>
        /// <param name="joint">Joint to get</param>
        /// <param name="handedness">Desired hand of <see cref="TrackedHandJoint"/></param>
        /// <returns></returns>
        private Vector3 UpdateFingerPosition(TrackedHandJoint joint, Handedness handedness)
        {
            if (HandJointUtils.TryGetJointPose(joint, handedness, out MixedRealityPose handJoint))
            {
                return handJoint.Position;
            }
            return Vector3.zero;
        }

        #endregion private methods

        #region public methods

        /// <summary>
        /// Checks whether the given item is visible in the list
        /// </summary>
        /// <param name="indexOfItem">the index of the item in the list</param>
        /// <returns><see cref="true"/> when item is visible</returns>
        public bool IsItemVisible(int indexOfItem)
        {
            bool itemLoc = true;

            if (indexOfItem < numItemsPrevView)
            {
                //its above the visible area
                itemLoc = false;
            }
            else if (indexOfItem > numItemsPostView)
            {
                //its below the visable area
                itemLoc = false;
            }
            return itemLoc;
        }

        /// <summary>
        /// Moves scroller by a multiplier of <see cref="ViewableArea"/>
        /// </summary>
        /// <param name="numOfPages">number of <see cref="ViewableArea"/> to move scroller by</param>
        /// <param name="animateToPage">if <see cref="true"/>, scroller will animate to new position</param>
        /// <param name="callback"> An optional action to pass in to get notified that the <see cref="ScrollingObjectCollection"/> is finished moving</param>
        public void PageBy(int numOfPages, bool animateToPage = true, System.Action callback = null)
        {
            Debug.Log(animateToPage);

            StopAllCoroutines();
            animatingToPosition = false;
            velocityState = VelocityState.None;

            if (scrollDirection == ScrollDirectionType.UpAndDown)
            {
                workingScrollerPos.y = Mathf.Clamp((FirstItemInView + (numOfPages * ViewableArea)) * CellHeight, minY, maxY);
                workingScrollerPos = workingScrollerPos.Mul(Vector3.up);
            }
            else
            {
                workingScrollerPos.x = Mathf.Clamp((FirstItemInView + (numOfPages * ViewableArea)) * CellWidth, minX, maxX);
                workingScrollerPos = workingScrollerPos.Mul(Vector3.right);
            }

            if (animateToPage)
            {
                AnimateScroller = AnimateTo(scrollContainer.transform.localPosition, workingScrollerPos, paginationCurve, animationLength, callback);
                StartCoroutine(AnimateScroller);
            }
            else
            {
                initialScrollerPos = workingScrollerPos;
                if (callback != null)
                {
                    callback?.Invoke();
                }
            }
        }

        /// <summary>
        /// Moves scroller a relative number of items
        /// </summary>
        /// <param name="numberOfItemsToMove">number of items to move by</param>
        /// <param name="animateToPosition">if <see cref="true"/>, scroller will animate to new position</param>
        /// <param name="callback"> An optional action to pass in to get notified that the <see cref="ScrollingObjectCollection"/> is finished moving</param>
        public void MoveByItems(int numberOfItemsToMove, bool animateToPosition = true, System.Action callback = null)
        {
            StopAllCoroutines();
            animatingToPosition = false;
            velocityState = VelocityState.None;

            if (scrollDirection == ScrollDirectionType.UpAndDown)
            {
                Debug.Log("FIIV: " + FirstItemInView);
                Debug.Log("offset: " + (StepMultiplier(numberOfItemsToMove, Tiers) * CellHeight));

                workingScrollerPos.y = FirstItemInView + (StepMultiplier(numberOfItemsToMove, Tiers) * CellHeight);
                //clamp the working pos since we already have calculated it
                workingScrollerPos.y = Mathf.Clamp(workingScrollerPos.y, minY, maxY);

                //zero out the other axes
                workingScrollerPos = workingScrollerPos.Mul(Vector3.up);
            }
            else
            {
                workingScrollerPos.x = FirstItemInView + (StepMultiplier(numberOfItemsToMove, Tiers) * CellWidth);
                //clamp the working pos since we already have calculated it
                workingScrollerPos.x = Mathf.Clamp(workingScrollerPos.x, minX, maxX);

                //zero out the other axes
                workingScrollerPos = workingScrollerPos.Mul(Vector3.right);
            }

            if (animateToPosition)
            {
                AnimateScroller = AnimateTo(scrollContainer.transform.localPosition, workingScrollerPos, paginationCurve, animationLength);
                StartCoroutine(AnimateScroller);
            }
            else
            {
                initialScrollerPos = workingScrollerPos;
                if (callback != null)
                {
                    callback?.Invoke();
                }
            }
        }

        /// <summary>
        /// Moves scroller a relative number of lines of items.
        /// </summary>
        /// <param name="numberOfLinesToMove">number of lines to move by</param>
        /// <param name="animateToPosition">if <see cref="true"/>, scroller will animate to new position</param>
        /// <param name="callback"> An optional action to pass in to get notified that the <see cref="ScrollingObjectCollection"/> is finished moving</param>
        public void MoveByLines(int numberOfLinesToMove, bool animateToPosition = true, System.Action callback = null)
        {
            StopAllCoroutines();
            animatingToPosition = false;
            velocityState = VelocityState.None;

            if (scrollDirection == ScrollDirectionType.UpAndDown)
            {
                workingScrollerPos.y = (FirstItemInView + (numberOfLinesToMove * Tiers)) * CellHeight;

                //clamp the working pos since we already have calculated it
                workingScrollerPos.y = Mathf.Clamp(workingScrollerPos.y, minY, maxY);

                //zero out the other axes
                workingScrollerPos = workingScrollerPos.Mul(Vector3.up);
            }
            else
            {
                workingScrollerPos.x = (FirstItemInView + (numberOfLinesToMove * Tiers)) * CellWidth;

                //clamp the working pos since we already have calculated it
                workingScrollerPos.x = Mathf.Clamp(workingScrollerPos.x, minX, maxX);

                //zero out the other axes
                workingScrollerPos = workingScrollerPos.Mul(Vector3.right);
            }

            if (animateToPosition)
            {
                AnimateScroller = AnimateTo(scrollContainer.transform.localPosition, workingScrollerPos, paginationCurve, animationLength, callback);
                StartCoroutine(AnimateScroller);
            }
            else
            {
                initialScrollerPos = workingScrollerPos;
                if (callback != null)
                {
                    callback?.Invoke();
                }
            }
        }

        /// <summary>
        /// Moves scroller to an absolute position where <param name"indexOfItem"/> is in the first column of the viewable area
        /// </summary>
        /// <param name="indexOfItem">item to move to, will be first (or closest to in respect to scroll maxiumum) in viewable area</param>
        /// <param name="animateToPosition">if <see cref="true"/>, scroller will animate to new position</param>
        /// <param name="callback"> An optional action to pass in to get notified that the <see cref="ScrollingObjectCollection"/> is finished moving</param>
        public void MoveTo(int indexOfItem, bool animateToPosition = true, System.Action callback = null)
        {
            StopAllCoroutines();
            animatingToPosition = false;
            velocityState = VelocityState.None;

            indexOfItem = (indexOfItem < 0) ? 0 : indexOfItem;

            if (scrollDirection == ScrollDirectionType.UpAndDown)
            {
                workingScrollerPos.y = StepMultiplier(indexOfItem, Tiers) * CellHeight;

                //clamp the working pos since we already have calculated it
                workingScrollerPos.y = Mathf.Clamp(workingScrollerPos.y, minY, maxY);

                //zero out the other axes
                workingScrollerPos = workingScrollerPos.Mul(Vector3.up);
            }
            else
            {
                workingScrollerPos.x = StepMultiplier(indexOfItem, Tiers) * CellWidth;

                //clamp the working pos since we already have calculated it
                workingScrollerPos.x = Mathf.Clamp(workingScrollerPos.x, minX, maxX);

                //zero out the other axes
                workingScrollerPos = workingScrollerPos.Mul(Vector3.right);
            }

            if (animateToPosition)
            {
                AnimateScroller = AnimateTo(scrollContainer.transform.localPosition, workingScrollerPos, paginationCurve, animationLength, callback);
                StartCoroutine(AnimateScroller);
            }
            else
            {
                initialScrollerPos = workingScrollerPos;

                if (callback != null)
                {
                    callback?.Invoke();
                }
            }
        }

        /// <summary>
        /// Converts <see cref="AxisOrientationType"/> into a normalized <see cref="Vector3"/> direction relative to the <see cref="ScrollingObjectCollection"/>'s transform
        /// </summary>
        /// <param name="axis"> <see cref="AxisOrientationType"/> to convert</param>
        /// <returns>Normalized direction</returns>
        public Vector3 AxisOrientationToTransformDirection(AxisOrientationType axis)
        {
            switch (axis)
            {
                case AxisOrientationType.PositiveX:
                    return transform.right;

                case AxisOrientationType.NegativeX:
                    return transform.right * -1.0f;

                case AxisOrientationType.PositiveY:
                    return transform.up;

                case AxisOrientationType.NegativeY:
                    return transform.up * -1.0f;

                case AxisOrientationType.PositiveZ:
                    return transform.forward;

                case AxisOrientationType.NegativeZ:
                    return transform.forward * -1.0f;

                default:
                    return Vector3.zero;
            }
        }

        #endregion

        #region static methods

        /// <summary>
        /// Lerps Vector3 source to goal.
        /// </summary>
        /// <remarks>
        /// Handles lerpTime less than 0 and more than 1.
        /// </remarks>
        /// <param name="source">Initial position to start at</param>
        /// <param name="goal">desired position to end at</param>
        /// <param name="deltaTime">time change per frame</param>
        /// <param name="lerpTime">amount of time for smoothng to take</param>
        /// <returns></returns>
        public static Vector3 SmoothTo(Vector3 source, Vector3 goal, float deltaTime, float lerpTime)
        {
            return Vector3.Lerp(source, goal, lerpTime.Equals(0.0f) ? 1f : deltaTime / lerpTime);
        }

        /// <summary>
        /// Projects a pointer's actual direction on the same plane as the original hit point (the point that caused the event to fire)
        /// </summary>
        /// <param name="origin">Where the direction is originating from</param>
        /// <param name="hitPoint">The original point where the hit occured</param>
        /// <param name="newDir">The direction of the pointer</param>
        /// <param name="axisConstraint">The (optional) axis to project the new point to</param>
        /// <returns>><see cref="Vector3"/> representing the new point on the plane as the hit point in world space</returns>
        /// <remarks><see cref="IMixedRealityPointer"/>'s eventData doesnt provide the "current" hit point, only an origin and direction, this makes the calculation easy</remarks>
        public static Vector3 GetTrackedPoint(Vector3 origin, Vector3 hitPoint, Vector3 newDir, AxisOrientationType? axisConstraint = null)
        {
            Vector3 hitDir = hitPoint - origin;
            float mag = Vector3.Dot(hitDir, newDir);
            Vector3 trackedPoint = origin + (newDir * mag);

            if (axisConstraint != null)
            {
                trackedPoint.Scale(AxisOrientationToDirection((AxisOrientationType)axisConstraint));
            }

            return trackedPoint;
        }

        /// <summary>
        /// Simple time threshold check
        /// </summary>
        /// <param name="initTime">Initial time</param>
        /// <param name="currTime">Current time</param>
        /// <param name="pressMargin">Time threshold</param>
        /// <returns><see cref="true"/> if amount of time surpasses <paramref name="pressMargin"/></returns>
        public static bool TimeTest(float initTime, float currTime, float pressMargin)
        {
            if (currTime - initTime > pressMargin)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calculates mow much scale is required for objBounds to match otherbounds.
        /// </summary>
        /// <param name="objBounds">Object representation to be scaled</param>
        /// <param name="otherBounds">Object representation to be scaled to</param>
        /// <param name="padding">padding multitplied into otherbounds</param>
        /// <returns>scale represented as a <see cref="Vector3"/> </returns>
        public static Vector3 CalculateScale(Bounds objBounds, Bounds otherBounds, Vector3 padding = default)
        {
            //Optional padding (especially z)
            Vector3 szA = otherBounds.size + new Vector3(otherBounds.size.x * padding.x, otherBounds.size.y * padding.y, otherBounds.size.z * padding.z);
            Vector3 szB = objBounds.size;
            return new Vector3(szA.x / szB.x, szA.y / szB.y, szA.z / szB.z);
        }

        /// <summary>
        /// Calculates which side a point in space is on
        /// </summary>
        /// <param name="initialDirection">The plane normal direction. </param>
        /// <param name="initialPosition">The point representing the plane's origin</param>
        /// <param name="pointToCompare">The point compared to the normal and origin</param>
        /// <returns>true when the compared point is on the other side of the plane</returns>
        public static bool TouchPassedThreshold(Vector3 initialDirection, Vector3 initialPosition, Vector3 pointToCompare)
        {
            //Debug.Log(pointToCompare.ToString("F5") + ", " + initialPosition.ToString("F5") + ", " + initialDirection.ToString("F5"));
            Vector3 delta = pointToCompare - initialPosition;
            float dot = Vector3.Dot(delta, initialDirection);

            return (dot > 0) ? true : false;
        }

        /// <summary>
        /// Finds the object-aligned size of a <see cref="Transform"/> 
        /// </summary>
        /// <param name="obj"><see cref="Transform"/> representing the object to get offset from</param>
        /// <returns></returns>
        public static bool TryGetObjectAlignedBoundsSize(Transform obj, AxisOrientationType axis, out float offset)
        {
            Collider c = obj.GetComponentInChildren<Collider>();
            Renderer rend = obj.GetComponentInChildren<Renderer>();
            Vector3 size = Vector3.zero;
            offset = 0.0f;

            //store and clear the original rotation
            Quaternion origRot = obj.rotation;
            obj.rotation = Quaternion.identity;

            bool canGetSize = false;

            if (c != null)
            {
                if (c.GetType() == typeof(BoxCollider))
                {
                    BoxCollider bC = c as BoxCollider;
                    size = bC.bounds.size;
                    canGetSize = true;
                }
                else if (c.GetType() == typeof(SphereCollider))
                {
                    SphereCollider sC = c as SphereCollider;
                    size = new Vector3(sC.radius, sC.radius, sC.radius);
                    canGetSize = true;
                }
                else
                {
                    canGetSize = false;
                }

            }
            else if (rend != null)
            {
                size = rend.bounds.size;
                canGetSize = true;
            }
            else
            {
                canGetSize = false;
            }

            //reapply our rotation
            obj.rotation = origRot;

            if (!canGetSize)
            {
                return false;
            }

            switch (axis)
            {
                case AxisOrientationType.PositiveX:
                case AxisOrientationType.NegativeX:
                    offset = size.x * 0.5f;
                    break;

                case AxisOrientationType.PositiveY:
                case AxisOrientationType.NegativeY:
                    offset = size.y * 0.5f;
                    break;

                case AxisOrientationType.PositiveZ:
                case AxisOrientationType.NegativeZ:
                    offset = size.z * 0.5f;
                    break;

                default:
                    offset = 0.0f;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Converts <see cref="AxisOrientationType"/> into a normalized <see cref="Vector3"/> direction
        /// </summary>
        /// <param name="axis"> <see cref="AxisOrientationType"/> to convert</param>
        /// <returns>Normalized direction</returns>
        public static Vector3 AxisOrientationToDirection(AxisOrientationType axis)
        {
            switch (axis)
            {
                case AxisOrientationType.PositiveX:
                    return Vector3.right;

                case AxisOrientationType.NegativeX:
                    return Vector3.right * -1.0f;

                case AxisOrientationType.PositiveY:
                    return Vector3.up;

                case AxisOrientationType.NegativeY:
                    return Vector3.up * -1.0f;

                case AxisOrientationType.PositiveZ:
                    return Vector3.forward;

                case AxisOrientationType.NegativeZ:
                    return Vector3.forward * -1.0f;

                default:
                    return Vector3.zero;
            }
        }
        
        #endregion

        #region IMixedRealityPointerHandler implementation

        ///</inheritdoc>
        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
        {
            //we ignore this for touch events and calculate PointerUp in the Update() loop;

            if (!isTouched && isEngaged && !animatingToPosition)
            {
                //Its a drag release
                initialScrollerPos = workingScrollerPos;

                //Release the pointer
                currentPointer.IsFocusLocked = false;
                currentPointer = null;

                //Clear our states
                isTouched = false;
                isEngaged = false;
                isDragging = false;

            }
        }

        ///</inheritdoc>
        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
        {
            //Do a check to prevent a new touch event from overriding our current touch values
            if (!isEngaged && !animatingToPosition)
            {
                if (eventData.Pointer.Controller.IsPositionAvailable)
                {
                    //create the offset for our thesholdCalculation -- grab the first item in the list
                    //TryGetObjectAlignedBoundsSize(NodeList[FirstItemInView].Transform, collectionForward, out thresholdOffset);

                    currentPointer = eventData.Pointer;

                    IMixedRealityControllerVisualizer controllerViz = eventData.Pointer.Controller.Visualizer as IMixedRealityControllerVisualizer;
                    currentPointerTransform = controllerViz.GameObjectProxy.transform;

                    currentPointer.IsFocusLocked = true;

                    pointerHitPoint = currentPointer.Result.Details.Point;

                    focusedObject = eventData.selectedObject;

                    scrollVelocity = 0.0f;

                    initialHandPos = currentPointerPos;
                    initialPressTime = Time.time;
                    initialScrollerPos = scrollContainer.transform.localPosition;

                    isTouched = false;
                    isEngaged = true;
                    isDragging = false;
                }
                else
                {
                    //not sure what to do with this pointer
                    Debug.Log(gameObject.name + "'s " + name + " intercepted a pointer, " + eventData.Pointer.PointerName + ", but don't know what to do with it.");
                }
            }
        }

        ///</inheritdoc>
        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            //we ignore this event and calculate click in the Update() loop;
        }
        #endregion IMixedRealityPointerHandler implementation

        #region IMixedRealityTouchHandler implementation

        ///</inheritdoc>
        void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
        {
            if (TryGetPokePointer(eventData.InputSource.Pointers, out currentPointer))
            {

                StopAllCoroutines();
                animatingToPosition = false;

                if (focusedObject != currentPointer.Result.CurrentPointerTarget || focusedObject == null)
                {
                    //TODO: Send input back down to child with a new sender so PassThroughMode knows to handle it properly
                    /*
                    HandTrackingInputEventData newTouchData = eventData;
                    newTouchData.Sender = this;

                    //We don't know if this is a scroll or not so we're going to pass the event along to the child
                    scrollChild = currentPointer.Result.CurrentPointerTarget.GetComponentInChildren<IMixedRealityTouchHandler>();
                    if (scrollChild != null)
                    {
                        scrollChild.OnTouchStarted(newTouchData);
                    }
                    */
                }
            }

            focusedObject = currentPointer.Result.CurrentPointerTarget;

            //Let everyone know the scroller has been engaged
            TouchStarted?.Invoke(focusedObject);

            if (!isTouched && !isEngaged)
            {
                currentHand = eventData.Controller.ControllerHandedness;

                initialHandPos = UpdateFingerPosition(TrackedHandJoint.IndexTip, eventData.Controller.ControllerHandedness);
                initialPressTime = Time.time;

                initialScrollerPos = scrollContainer.transform.localPosition;

                isTouched = true;
                isEngaged = true;
                isDragging = false;
            }
        }

        ///</inheritdoc>
        void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            if (TryGetPokePointer(eventData.InputSource.Pointers, out IMixedRealityPointer p))
            {
                if (focusedObject != currentPointer.Result.CurrentPointerTarget || focusedObject == null)
                {
                    //TODO: Send input back down to child with a new sender so PassThroughMode knows to handle it properly
                    /*

                    HandTrackingInputEventData newTouchData = eventData;
                    newTouchData.Sender = this;

                    //We don't know if this is a scroll or not so we're going to pass the event along to the child
                    scrollChild = currentPointer.Result.CurrentPointerTarget.GetComponentInChildren<IMixedRealityTouchHandler>();
                    if (scrollChild != null)
                    {
                        scrollChild.OnTouchCompleted(newTouchData);
                    }
                    */
                }
            }
        }

        ///</inheritdoc>
        void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData)
        {

            if (TryGetPokePointer(eventData.InputSource.Pointers, out IMixedRealityPointer p))
            {
                if (!isDragging & p == currentPointer)
                {
                    //TODO: Send input back down to child with a new sender so PassThroughMode knows to handle it properly
                    /*

                    HandTrackingInputEventData newTouchData = eventData;
                    newTouchData.Sender = this;

                    if (focusedObject != p.Result.CurrentPointerTarget)
                    {
                        scrollChild = currentPointer.Result.CurrentPointerTarget.GetComponentInChildren<IMixedRealityTouchHandler>();

                        //the finger moved enough we have a mismatch in our focused object, we need to make sure we call touch started first
                        //and update our focusedObject as well

                        if (scrollChild != null)
                        {
                            scrollChild.OnTouchStarted(newTouchData);
                        }

                        focusedObject = currentPointer.Result.CurrentPointerTarget;
                    }
                    scrollChild = focusedObject.GetComponentInChildren<IMixedRealityTouchHandler>();

                    //now call OnTouchUpdated
                    if (scrollChild != null)
                    {
                        scrollChild.OnTouchUpdated(newTouchData);
                    }
                    */
                }
            }
        }

        #endregion IMixedRealityTouchHandler implementation

        #region IMixedRealitySourceStateHandler implementation

        void IMixedRealitySourceStateHandler.OnSourceDetected(SourceStateEventData eventData) { /* */
                }

                void IMixedRealitySourceStateHandler.OnSourceLost(SourceStateEventData eventData)
        {
            //We'll consider this a drag release
            if (isEngaged && !animatingToPosition)
            {
                if (isTouched)
                {
                    //Its a drag release
                    initialScrollerPos = workingScrollerPos;

                }

                if (isDragging)
                {
                    initialScrollerPos = workingScrollerPos;
                }

                //Release the pointer
                if (currentPointer.GetType() != typeof(PokePointer))
                {
                    currentPointer.IsFocusLocked = false;
                    currentPointer = null;
                }

                //Let everyone know the scroller is no longer engaged
                TouchEnded?.Invoke(focusedObject);

                //Clear our states
                isTouched = false;
                isEngaged = false;
                isDragging = false;

                velocityState = VelocityState.Calculating;
            }
        }

        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            //--
        }

        #endregion IMixedRealitySourceStateHandler implementation
    }
}