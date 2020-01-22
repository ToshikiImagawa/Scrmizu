// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.ScrollRect;

namespace Scrmizu
{
    [AddComponentMenu("UI/Paged Scroll Rect", 37)]
    [SelectionBase]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class PagedScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler,
        IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        /// <summary>
        /// Reverse direction scroll.
        /// </summary>
        [SerializeField, Tooltip("Reverse direction scroll.")]
        private bool isReverse = false;

        /// <summary>
        /// Page
        /// </summary>
        [SerializeField, Tooltip("Page.")] private int page;

        [SerializeField] private RectTransform content;

        [SerializeField, Tooltip("Direction of scroll.")]
        private Direction direction = Direction.Vertical;

        [SerializeField] private MovementType movementType = MovementType.Elastic;

        [SerializeField] private float elasticity = 0.1f;

        [SerializeField] private bool inertia = true;
        [SerializeField] private float decelerationRate = 0.135f; // Only used when inertia is enabled
        [SerializeField] private float scrollSensitivity = 1.0f;
        [SerializeField] private RectTransform viewport;

        [SerializeField] private ScrollRectEvent onValueChanged = new ScrollRectEvent();
        [SerializeField] private PagedScrollRectEvent onPageChanged = new PagedScrollRectEvent();

        [NonSerialized] private bool _hasRebuiltLayout;
        [NonSerialized] private RectTransform _rect;
        [NonSerialized] private List<RectTransform> _contentChildren = new List<RectTransform>();

        // The offset from handle position to mouse down position
        private Vector2 _pointerStartLocalCursor = Vector2.zero;
        protected Vector2 ContentStartPosition = Vector2.zero;

        private RectTransform _viewRect;

        protected Bounds ContentBounds;
        private Bounds _viewBounds;

        private Vector2 _velocity;

        private bool _dragging;

        private Vector2 _prevPosition = Vector2.zero;
        private Bounds _prevContentBounds;
        private Bounds _prevViewBounds;

        private RectTransform _scrollbarRect;

        private DrivenRectTransformTracker _tracker = new DrivenRectTransformTracker();

        private static readonly List<ILayoutIgnorer> ListPool = new List<ILayoutIgnorer>();

        /// <summary>
        /// The content that can be scrolled. It should be a child of the GameObject with ScrollRect on it.
        /// </summary>
        public RectTransform Content
        {
            get => content;
            set => content = value;
        }

        protected List<RectTransform> ContentChildren => _contentChildren;

        /// <summary>
        /// Direction of scroll.
        /// </summary>
        public Direction Direction
        {
            get => direction;
            set => direction = value;
        }

        /// <summary>
        /// The behavior to use when the content moves beyond the scroll rect.
        /// </summary>
        public MovementType MovementType
        {
            get => movementType;
            set => movementType = value;
        }

        /// <summary>
        /// The amount of elasticity to use when the content moves beyond the scroll rect.
        /// </summary>
        public float Elasticity
        {
            get => elasticity;
            set => elasticity = value;
        }

        /// <summary>
        /// Should movement inertia be enabled?
        /// </summary>
        /// <remarks>
        /// Inertia means that the scrollrect content will keep scrolling for a while after being dragged. It gradually slows down according to the decelerationRate.
        /// </remarks>
        public bool Inertia
        {
            get => inertia;
            set => inertia = value;
        }

        /// <summary>
        /// The rate at which movement slows down.
        /// </summary>
        /// <remarks>
        /// The deceleration rate is the speed reduction per second. A value of 0.5 halves the speed each second. The default is 0.135. The deceleration rate is only used when inertia is enabled.
        /// </remarks>
        public float DecelerationRate
        {
            get => decelerationRate;
            set => decelerationRate = value;
        }


        /// <summary>
        /// The sensitivity to scroll wheel and track pad scroll events.
        /// </summary>
        /// <remarks>
        /// Higher values indicate higher sensitivity.
        /// </remarks>
        public float ScrollSensitivity
        {
            get => scrollSensitivity;
            set => scrollSensitivity = value;
        }

        /// <summary>
        /// Reference to the viewport RectTransform that is the parent of the content RectTransform.
        /// </summary>
        public RectTransform Viewport
        {
            get => viewport;
            set
            {
                viewport = value;
                SetDirtyCaching();
            }
        }

        /// <summary>
        /// Callback executed when the position of the child changes.
        /// </summary>
        public ScrollRectEvent OnValueChanged
        {
            get => onValueChanged;
            set => onValueChanged = value;
        }

        /// <summary>
        /// Callback executed when the page changes.
        /// </summary>
        public PagedScrollRectEvent OnPageChanged
        {
            get => onPageChanged;
            set => onPageChanged = value;
        }

        /// <summary>
        /// The current velocity of the content.
        /// </summary>
        /// <remarks>
        /// The velocity is defined in units per second.
        /// </remarks>
        public Vector2 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

        /// <summary>
        /// The current page.
        /// </summary>
        public int Page
        {
            get => page;
            set
            {
                if (page == value) return;
                page = value;
                onPageChanged.Invoke(value);
            }
        }

        /// <summary>
        /// The scroll position as a Vector2 between (0,0) and (1,1) with (0,0) being the lower left corner.
        /// </summary>
        public Vector2 NormalizedPosition
        {
            get => new Vector2(HorizontalNormalizedPosition, VerticalNormalizedPosition);
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        /// <summary>
        /// The horizontal scroll position as a value between 0 and 1, with 0 being at the left.
        /// </summary>
        public float HorizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (ContentBounds.size.x <= _viewBounds.size.x)
                    return (_viewBounds.min.x > ContentBounds.min.x) ? 1 : 0;
                return (_viewBounds.min.x - ContentBounds.min.x) / (ContentBounds.size.x - _viewBounds.size.x);
            }
            set => SetNormalizedPosition(value, 0);
        }

        /// <summary>
        /// The vertical scroll position as a value between 0 and 1, with 0 being at the bottom.
        /// </summary>

        public float VerticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (ContentBounds.size.y <= _viewBounds.size.y)
                    return (_viewBounds.min.y > ContentBounds.min.y) ? 1 : 0;

                return (_viewBounds.min.y - ContentBounds.min.y) / (ContentBounds.size.y - _viewBounds.size.y);
            }
            set => SetNormalizedPosition(value, 1);
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        /// <inheritdoc />
        public virtual float minWidth => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        /// <inheritdoc />
        public virtual float preferredWidth => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        /// <inheritdoc />
        public virtual float flexibleWidth => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        /// <inheritdoc />
        public virtual float minHeight => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        /// <inheritdoc />
        public virtual float preferredHeight => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        /// <inheritdoc />
        public virtual float flexibleHeight => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        /// <inheritdoc />
        public virtual int layoutPriority => -1;

        protected RectTransform ViewRect
        {
            get
            {
                if (_viewRect == null)
                    _viewRect = viewport;
                if (_viewRect == null)
                    _viewRect = (RectTransform) transform;
                return _viewRect;
            }
        }

        private bool Horizontal => direction == Direction.Horizontal;

        private bool Vertical => direction == Direction.Vertical;

        private bool HScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return ContentBounds.size.x > _viewBounds.size.x + 0.01f;
                return true;
            }
        }

        private bool VScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return ContentBounds.size.y > _viewBounds.size.y + 0.01f;
                return true;
            }
        }

        private RectTransform RectTransform
        {
            get
            {
                if (_rect == null)
                    _rect = GetComponent<RectTransform>();
                return _rect;
            }
        }

        private float PagedPosition
        {
            get
            {
                switch (direction)
                {
                    case Direction.Vertical:
                        return page * viewport.rect.height * (isReverse ? -1 : 1);
                    case Direction.Horizontal:
                        return page * viewport.rect.width * (isReverse ? 1 : -1);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected PagedScrollRect()
        {
        }

        public virtual void Next()
        {
            if ((page + 1) >= _contentChildren.Count) return;
            Page = page + 1;
        }

        public virtual void Back()
        {
            if ((page - 1) < 0) return;
            Page = page - 1;
        }

        public virtual void Rebuild(CanvasUpdate executing)
        {
            switch (executing)
            {
                case CanvasUpdate.PostLayout:
                    UpdateBounds();
                    UpdatePrevData();

                    _hasRebuiltLayout = true;
                    break;
                case CanvasUpdate.Prelayout:
                case CanvasUpdate.Layout:
                case CanvasUpdate.PreRender:
                case CanvasUpdate.LatePreRender:
                case CanvasUpdate.MaxUpdateValue:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(executing), executing, null);
            }
        }

        public virtual void LayoutComplete()
        {
        }

        public virtual void GraphicUpdateComplete()
        {
        }

        ///  <summary>
        ///  See member in base class.
        ///  </summary>
        /// <inheritdoc />
        public override bool IsActive()
        {
            return base.IsActive() && content != null;
        }

        /// <summary>
        /// Sets the velocity to zero on both axes so the content stops moving.
        /// </summary>
        public virtual void StopMovement()
        {
            _velocity = Vector2.zero;
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            var delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (Vertical && !Horizontal)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    delta.y = delta.x;
                delta.x = 0;
            }

            if (Horizontal && !Vertical)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    delta.x = delta.y;
                delta.y = 0;
            }

            var position = content.anchoredPosition;
            position += delta * scrollSensitivity;
            if (movementType == MovementType.Clamped)
                position += CalculateOffset(position - content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _velocity = Vector2.zero;
        }

        /// <summary>
        /// Handling for when the content is beging being dragged.
        /// </summary>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            _pointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position,
                eventData.pressEventCamera, out _pointerStartLocalCursor);
            ContentStartPosition = content.anchoredPosition;
            _dragging = true;
        }

        /// <summary>
        /// Handling for when the content has finished being dragged.
        /// </summary>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _dragging = false;
        }

        /// <summary>
        /// Handling for when the content is dragged.
        /// </summary>
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position,
                eventData.pressEventCamera, out var localCursor))
                return;

            UpdateBounds();

            var pointerDelta = localCursor - _pointerStartLocalCursor;
            var position = ContentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            var offset = CalculateOffset(position - content.anchoredPosition);
            position += offset;
            if (movementType == MovementType.Elastic)
            {
                if (Math.Abs(offset.x) > 0)
                    position.x = position.x - RubberDelta(offset.x, _viewBounds.size.x);
                if (Math.Abs(offset.y) > 0)
                    position.y = position.y - RubberDelta(offset.y, _viewBounds.size.y);
            }

            SetContentAnchoredPosition(position);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            SetDirty();
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            _hasRebuiltLayout = false;
            _tracker.Clear();
            _velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
            base.OnDisable();
        }

        /// <summary>
        /// Override to alter or add to the code that keeps the appearance of the scroll rect synced with its data.
        /// </summary>
        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        }

        /// <summary>
        /// Override to alter or add to the code that caches data to avoid repeated heavy operations.
        /// </summary>
        protected void SetDirtyCaching()
        {
            if (!IsActive())
                return;

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirtyCaching();
        }

#endif

        /// <summary>
        /// Sets the anchored position of the content.
        /// </summary>
        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (!Horizontal)
                position.x = content.anchoredPosition.x;
            if (!Vertical)
                position.y = content.anchoredPosition.y;

            if (position == content.anchoredPosition) return;
            content.anchoredPosition = position;
            if (_dragging) UpdatePage();
            UpdateBounds();
        }

        protected virtual void LateUpdate()
        {
            if (!content)
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();
            var deltaTime = Time.unscaledDeltaTime;
            var pagedOffset = CalculatePageOffset();

            if (!_dragging && (pagedOffset != Vector2.zero || _velocity != Vector2.zero))
            {
                var position = content.anchoredPosition;
                for (var axis = 0; axis < 2; axis++)
                {
                    // Apply spring physics if movement is elastic and content has an offset from the view.
                    if (Math.Abs(pagedOffset[axis]) > 0)
                    {
                        var speed = _velocity[axis];
                        var anchoredPosition = content.anchoredPosition;
                        position[axis] = Mathf.SmoothDamp(anchoredPosition[axis],
                            anchoredPosition[axis] + pagedOffset[axis], ref speed, elasticity, Mathf.Infinity,
                            deltaTime);
                        if (Mathf.Abs(speed) < 1)
                            speed = 0;
                        _velocity[axis] = speed;
                    }
                    // Else move content according to velocity with deceleration applied.
                    else if (inertia)
                    {
                        _velocity[axis] *= Mathf.Pow(decelerationRate, deltaTime);
                        if (Mathf.Abs(_velocity[axis]) < 1)
                            _velocity[axis] = 0;
                        position[axis] += _velocity[axis] * deltaTime;
                    }
                    // If we have neither elaticity or friction, there shouldn't be any velocity.
                    else
                    {
                        _velocity[axis] = 0;
                    }
                }

                SetContentAnchoredPosition(position);
            }
            else
            {
                var offset = CalculateOffset(Vector2.zero);
                if (!_dragging && (offset != Vector2.zero || _velocity != Vector2.zero))
                {
                    var position = content.anchoredPosition;
                    for (var axis = 0; axis < 2; axis++)
                    {
                        // Apply spring physics if movement is elastic and content has an offset from the view.
                        if (movementType == MovementType.Elastic && Math.Abs(offset[axis]) > 0)
                        {
                            var speed = _velocity[axis];
                            var anchoredPosition = content.anchoredPosition;
                            position[axis] = Mathf.SmoothDamp(anchoredPosition[axis],
                                anchoredPosition[axis] + offset[axis], ref speed, elasticity, Mathf.Infinity,
                                deltaTime);
                            if (Mathf.Abs(speed) < 1)
                                speed = 0;
                            _velocity[axis] = speed;
                        }
                        // Else move content according to velocity with deceleration applied.
                        else if (inertia)
                        {
                            _velocity[axis] *= Mathf.Pow(decelerationRate, deltaTime);
                            if (Mathf.Abs(_velocity[axis]) < 1)
                                _velocity[axis] = 0;
                            position[axis] += _velocity[axis] * deltaTime;
                        }
                        // If we have neither elaticity or friction, there shouldn't be any velocity.
                        else
                        {
                            _velocity[axis] = 0;
                        }
                    }

                    if (movementType == MovementType.Clamped)
                    {
                        offset = CalculateOffset(position - content.anchoredPosition);
                        position += offset;
                    }

                    SetContentAnchoredPosition(position);
                }
            }

            if (_dragging && inertia)
            {
                Vector3 newVelocity = (content.anchoredPosition - _prevPosition) / deltaTime;
                _velocity = Vector3.Lerp(_velocity, newVelocity, deltaTime * 10);
            }

            if (_viewBounds != _prevViewBounds || ContentBounds != _prevContentBounds ||
                content.anchoredPosition != _prevPosition)
            {
                UISystemProfilerApi.AddMarker("ScrollRect.value", this);
                onValueChanged.Invoke(NormalizedPosition);
                UpdatePrevData();
            }
        }

        /// <summary>
        /// Helper function to update the previous data fields on a ScrollRect. Call this before you change data in the ScrollRect.
        /// </summary>
        protected void UpdatePrevData()
        {
            _prevPosition = content == null ? Vector2.zero : content.anchoredPosition;
            _prevViewBounds = _viewBounds;
            _prevContentBounds = ContentBounds;
        }

        /// <summary>
        /// >Set the horizontal or vertical scroll position as a value between 0 and 1, with 0 being at the left or at the bottom.
        /// </summary>
        /// <param name="value">The position to set, between 0 and 1.</param>
        /// <param name="axis">The axis to set: 0 for horizontal, 1 for vertical.</param>
        protected virtual void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            UpdateBounds();
            var hiddenLength = ContentBounds.size[axis] - _viewBounds.size[axis];
            var contentBoundsMinPosition = _viewBounds.min[axis] - value * hiddenLength;
            var position = content.localPosition;
            var newLocalPosition = position[axis] + contentBoundsMinPosition - ContentBounds.min[axis];

            var localPosition = position;
            if (!(Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)) return;
            localPosition[axis] = newLocalPosition;
            content.localPosition = localPosition;
            _velocity[axis] = 0;
            UpdatePage();
            UpdateBounds();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        /// <summary>
        /// Calculate the bounds the ScrollRect should be using.
        /// </summary>
        protected void UpdateBounds()
        {
            var rect = ViewRect.rect;
            _viewBounds = new Bounds(rect.center, rect.size);
            ContentBounds = GetBounds();

            if (content == null) return;

            var contentSize = ContentBounds.size;
            var contentPos = ContentBounds.center;
            var contentPivot = content.pivot;
            AdjustBounds(ref _viewBounds, ref contentPivot, ref contentSize, ref contentPos);
            ContentBounds.size = contentSize;
            ContentBounds.center = contentPos;

            if (MovementType != MovementType.Clamped) return;
            var delta = Vector2.zero;
            if (_viewBounds.max.x > ContentBounds.max.x)
            {
                delta.x = Math.Min(_viewBounds.min.x - ContentBounds.min.x,
                    _viewBounds.max.x - ContentBounds.max.x);
            }
            else if (_viewBounds.min.x < ContentBounds.min.x)
            {
                delta.x = Math.Max(_viewBounds.min.x - ContentBounds.min.x,
                    _viewBounds.max.x - ContentBounds.max.x);
            }

            if (_viewBounds.min.y < ContentBounds.min.y)
            {
                delta.y = Math.Max(_viewBounds.min.y - ContentBounds.min.y,
                    _viewBounds.max.y - ContentBounds.max.y);
            }
            else if (_viewBounds.max.y > ContentBounds.max.y)
            {
                delta.y = Math.Min(_viewBounds.min.y - ContentBounds.min.y,
                    _viewBounds.max.y - ContentBounds.max.y);
            }

            if (!(delta.sqrMagnitude > float.Epsilon)) return;
            contentPos = content.anchoredPosition + delta;
            if (!Horizontal)
                contentPos.x = content.anchoredPosition.x;
            if (!Vertical)
                contentPos.y = content.anchoredPosition.y;
            AdjustBounds(ref _viewBounds, ref contentPivot, ref contentSize, ref contentPos);
        }

        private void UpdatePage()
        {
            float value;
            float size;
            float speed;
            switch (direction)
            {
                case Direction.Vertical:
                    value = content.anchoredPosition.y * (isReverse ? -1 : 1);
                    size = viewport.rect.height;
                    speed = _velocity.y;
                    break;
                case Direction.Horizontal:
                    value = content.anchoredPosition.x * (isReverse ? 1 : -1);
                    size = viewport.rect.width;
                    speed = _velocity.x * -1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var unscaledDeltaTime = Time.unscaledDeltaTime;
            value += speed * unscaledDeltaTime * (isReverse ? -1 : 1);

            var beforePage = page;
            if (value < 0)
                page = 0;
            else
                page = (int) Mathf.Floor(value / size + 0.5f);
            if (page >= content.childCount) page = content.childCount - 1;
            if (beforePage == page) return;
            onPageChanged.Invoke(page);
        }

        private void EnsureLayoutHasRebuilt()
        {
            if (!_hasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize *
                   Mathf.Sign(overStretching);
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal()
        {
            _contentChildren.Clear();
            for (var i = 0; i < content.childCount; i++)
            {
                var rect = content.GetChild(i) as RectTransform;
                if (rect == null || !rect.gameObject.activeInHierarchy)
                    continue;

                ListPool.AddRange(rect.GetComponents<ILayoutIgnorer>());

                if (ListPool.Count == 0)
                {
                    _contentChildren.Add(rect);
                    continue;
                }

                if (ListPool.Any(ignorer => !ignorer.ignoreLayout))
                {
                    _contentChildren.Add(rect);
                }
            }

            ListPool.Clear();
            _tracker.Clear();
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputVertical()
        {
        }

        public virtual void SetLayoutHorizontal()
        {
            _tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        public virtual void SetLayoutVertical()
        {
            var rect = ViewRect.rect;
            _viewBounds = new Bounds(rect.center, rect.size);
            ContentBounds = GetBounds();
            HandleSelfFittingAlongAxis(1);
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            switch (direction)
            {
                case Direction.Vertical:
                    if (axis != 1) return;
                    var height = viewport.rect.height;
                    _tracker.Add(this, content, DrivenTransformProperties.SizeDeltaY);
                    _tracker.Add(this, content, DrivenTransformProperties.AnchoredPositionY);
                    _tracker.Add(this, content, DrivenTransformProperties.AnchorMaxY);
                    _tracker.Add(this, content, DrivenTransformProperties.AnchorMinY);
                    _tracker.Add(this, content, DrivenTransformProperties.PivotY);

                    content.SetSizeWithCurrentAnchors((RectTransform.Axis) axis, height * content.childCount);
                    content.anchorMax = new Vector2(content.anchorMax.x, isReverse ? 0 : 1f);
                    content.anchorMin = new Vector2(content.anchorMin.x, isReverse ? 0 : 1f);
                    content.pivot = new Vector2(content.pivot.x, isReverse ? 0 : 1f);

                    for (var i = 0; i < ContentChildren.Count; i++)
                    {
                        var child = ContentChildren[i];
                        _tracker.Add(this, child, DrivenTransformProperties.SizeDeltaY);
                        _tracker.Add(this, child, DrivenTransformProperties.AnchoredPositionY);
                        _tracker.Add(this, child, DrivenTransformProperties.AnchorMaxY);
                        _tracker.Add(this, child, DrivenTransformProperties.AnchorMinY);
                        _tracker.Add(this, child, DrivenTransformProperties.PivotY);
                        child.anchoredPosition = new Vector2(child.anchoredPosition.x,
                            height / 2 * (isReverse ? 1 : -1) + height * i * (isReverse ? 1 : -1));
                        child.SetSizeWithCurrentAnchors((RectTransform.Axis) axis, height);
                        child.anchorMax = new Vector2(child.anchorMax.x, isReverse ? 0 : 1f);
                        child.anchorMin = new Vector2(child.anchorMin.x, isReverse ? 0 : 1f);
                        child.pivot = new Vector2(child.pivot.x, 0.5f);
                    }

                    break;
                case Direction.Horizontal:
                    if (axis != 0) return;
                    var width = viewport.rect.width;
                    _tracker.Add(this, content, DrivenTransformProperties.SizeDeltaX);
                    _tracker.Add(this, content, DrivenTransformProperties.AnchoredPositionX);
                    _tracker.Add(this, content, DrivenTransformProperties.AnchorMaxX);
                    _tracker.Add(this, content, DrivenTransformProperties.AnchorMinX);
                    _tracker.Add(this, content, DrivenTransformProperties.PivotX);

                    content.SetSizeWithCurrentAnchors((RectTransform.Axis) axis, width * content.childCount);
                    content.anchorMax = new Vector2(isReverse ? 1f : 0, content.anchorMax.y);
                    content.anchorMin = new Vector2(isReverse ? 1f : 0, content.anchorMin.y);
                    content.pivot = new Vector2(isReverse ? 1f : 0, content.pivot.y);

                    for (var i = 0; i < ContentChildren.Count; i++)
                    {
                        var child = ContentChildren[i];
                        _tracker.Add(this, child, DrivenTransformProperties.SizeDeltaX);
                        _tracker.Add(this, child, DrivenTransformProperties.AnchoredPositionX);
                        _tracker.Add(this, child, DrivenTransformProperties.AnchorMaxX);
                        _tracker.Add(this, child, DrivenTransformProperties.AnchorMinX);
                        _tracker.Add(this, child, DrivenTransformProperties.PivotX);
                        child.anchoredPosition =
                            new Vector2(width / 2 * (isReverse ? -1 : 1) + width * i * (isReverse ? -1 : 1),
                                child.anchoredPosition.y);
                        child.SetSizeWithCurrentAnchors((RectTransform.Axis) axis, width);
                        child.anchorMax = new Vector2(isReverse ? 1f : 0, child.anchorMax.y);
                        child.anchorMin = new Vector2(isReverse ? 1f : 0, child.anchorMin.y);
                        child.pivot = new Vector2(0.5f, child.pivot.y);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void UpdateOneScrollbarVisibility(bool xScrollingNeeded, bool xAxisEnabled,
            ScrollbarVisibility scrollbarVisibility, Scrollbar scrollbar)
        {
            if (!scrollbar) return;
            if (scrollbarVisibility == ScrollbarVisibility.Permanent)
            {
                if (scrollbar.gameObject.activeSelf != xAxisEnabled)
                    scrollbar.gameObject.SetActive(xAxisEnabled);
            }
            else
            {
                if (scrollbar.gameObject.activeSelf != xScrollingNeeded)
                    scrollbar.gameObject.SetActive(xScrollingNeeded);
            }
        }

        internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize,
            ref Vector3 contentPos)
        {
            var excess = viewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (contentPivot.x - 0.5f);
                contentSize.x = viewBounds.size.x;
            }

            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (contentPivot.y - 0.5f);
                contentSize.y = viewBounds.size.y;
            }
        }

        private readonly Vector3[] _mCorners = new Vector3[4];

        private Bounds GetBounds()
        {
            if (content == null)
                return new Bounds();
            content.GetWorldCorners(_mCorners);
            var viewWorldToLocalMatrix = ViewRect.worldToLocalMatrix;
            return InternalGetBounds(_mCorners, ref viewWorldToLocalMatrix);
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            return InternalCalculateOffset(ref _viewBounds, ref ContentBounds, direction, movementType, ref delta);
        }

        private Vector2 CalculatePageOffset()
        {
            var offset = Vector2.zero;
            switch (direction)
            {
                case Direction.Vertical:
                    offset.y = PagedPosition - content.anchoredPosition.y;
                    break;
                case Direction.Horizontal:
                    offset.x = PagedPosition - content.anchoredPosition.x;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return offset;
        }

        internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
        {
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (var j = 0; j < 4; j++)
            {
                var v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds,
            Direction direction, MovementType movementType, ref Vector2 delta)
        {
            var offset = Vector2.zero;
            if (movementType == MovementType.Unrestricted)
                return offset;

            Vector2 min = contentBounds.min;
            Vector2 max = contentBounds.max;

            switch (direction)
            {
                case Direction.Vertical:
                    min.y += delta.y;
                    max.y += delta.y;
                    if (max.y < viewBounds.max.y)
                        offset.y = viewBounds.max.y - max.y;
                    else if (min.y > viewBounds.min.y)
                        offset.y = viewBounds.min.y - min.y;
                    break;
                case Direction.Horizontal:
                    min.x += delta.x;
                    max.x += delta.x;
                    if (min.x > viewBounds.min.x)
                        offset.x = viewBounds.min.x - min.x;
                    else if (max.x < viewBounds.max.x)
                        offset.x = viewBounds.max.x - max.x;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return offset;
        }

        /// <summary>
        /// Event type used by the PagedScrollRect.
        /// </summary>
        [Serializable]
        public class PagedScrollRectEvent : UnityEvent<int>
        {
        }
    }
}