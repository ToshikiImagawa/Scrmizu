using System;
using UnityEngine;
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
    public class PagedScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup, ILayoutController
    {
        [SerializeField, Tooltip("The content that can be scrolled. It should be a child of the GameObject with ScrollRect on it.")]
        private RectTransform content;

        [SerializeField, Tooltip("Direction of scroll.")]
        private Direction direction = Direction.Vertical;

        [SerializeField, Tooltip("The behavior to use when the content moves beyond the scroll rect.")]
        private MovementType movementType = MovementType.Elastic;

        [SerializeField, Tooltip("The amount of elasticity to use when the content moves beyond the scroll rect.")]
        private float elasticity = 0.1f;

        [SerializeField, Tooltip("Should movement inertia be enabled?")]
        private bool inertia = true;

        [SerializeField, Tooltip("The rate at which movement slows down.")]
        private float decelerationRate = 0.135f; // Only used when inertia is enabled

        [SerializeField, Tooltip("The sensitivity to scroll wheel and track pad scroll events.")]
        private float scrollSensitivity = 1.0f;

        [SerializeField, Tooltip("Reference to the viewport RectTransform that is the parent of the content RectTransform.")]
        private RectTransform viewport;

        [SerializeField, Tooltip("Optional Scrollbar object linked to the scrolling of the ScrollRect.")]
        private Scrollbar scrollbar;


        [SerializeField, Tooltip("The mode of visibility for the scrollbar.")]
        private ScrollbarVisibility scrollbarVisibility;

        [SerializeField, Tooltip("The space between the scrollbar and the viewport.")]
        private float scrollbarSpacing;

        [SerializeField, Tooltip("Callback executed when the position of the child changes.")]
        private ScrollRectEvent onValueChanged = new ScrollRectEvent();

        [NonSerialized] private RectTransform _rectTransform;

        [NonSerialized] private bool _hasRebuiltLayout = false;


        private RectTransform _scrollbarRect;

        private DrivenRectTransformTracker _tracker;

        // The offset from handle position to mouse down position
        private Vector2 pointerStartLocalCursor = Vector2.zero;
        protected Vector2 contentStartPosition = Vector2.zero;

        protected Bounds _contentBounds;
        private Bounds _viewBounds;

        private Vector2 _velocity;
        private RectTransform _viewRect;
        private readonly Vector3[] _corners = new Vector3[4];

        private bool _dragging;

        private Vector2 _prevPosition = Vector2.zero;
        private Bounds _prevContentBounds;
        private Bounds _prevViewBounds;

        private bool _sliderExpand;
        private float _sliderLength;

        /// <summary>
        /// The content that can be scrolled. It should be a child of the GameObject with ScrollRect on it.
        /// </summary>
        public RectTransform Content { get { return content; } set { content = value; } }
        /// <summary>
        /// Direction of scroll.
        /// </summary>
        public Direction Direction { get { return direction; } set { direction = value; } }
        /// <summary>
        /// The amount of elasticity to use when the content moves beyond the scroll rect.
        /// </summary>
        public float Elasticity { get { return elasticity; } set { elasticity = value; } }
        /// <summary>
        /// The behavior to use when the content moves beyond the scroll rect.
        /// </summary>
        public MovementType MovementType { get { return movementType; } set { movementType = value; } }
        /// <summary>
        /// Should movement inertia be enabled?
        /// </summary>
        public bool Inertia { get { return inertia; } set { inertia = value; } }

        /// <summary>
        /// The rate at which movement slows down.
        /// </summary>
        public float DecelerationRate { get { return decelerationRate; } set { decelerationRate = value; } }
        /// <summary>
        /// The sensitivity to scroll wheel and track pad scroll events.
        /// </summary>
        public float ScrollSensitivity { get { return scrollSensitivity; } set { scrollSensitivity = value; } }

        /// <summary>
        /// Reference to the viewport RectTransform that is the parent of the content RectTransform.
        /// </summary>
        public RectTransform Viewport { get { return viewport; } set { viewport = value; SetDirtyCaching(); } }

        /// <summary>
        /// Optional Scrollbar object linked to the scrolling of the ScrollRect.
        /// </summary>
        public Scrollbar Scrollbar
        {
            get
            {
                return scrollbar;
            }
            set
            {
                if (scrollbar)
                    scrollbar.onValueChanged.RemoveListener(SetNormalizedPosition);
                scrollbar = value;
                if (scrollbar)
                    scrollbar.onValueChanged.AddListener(SetNormalizedPosition);
                SetDirtyCaching();
            }
        }

        /// <summary>
        /// The mode of visibility for the scrollbar.
        /// </summary>
        public ScrollbarVisibility ScrollbarVisibility { get { return scrollbarVisibility; } set { scrollbarVisibility = value; SetDirtyCaching(); } }
        /// <summary>
        /// The space between the scrollbar and the viewport.
        /// </summary>
        public float ScrollbarSpacing { get { return scrollbarSpacing; } set { scrollbarSpacing = value; SetDirty(); } }

        /// <summary>
        /// Callback executed when the position of the child changes.
        /// </summary>
        public ScrollRectEvent OnValueChanged { get { return onValueChanged; } set { onValueChanged = value; } }

        /// <summary>
        /// The scroll position as a value between 0 and 1.
        /// </summary>
        public float NormalizedPosition
        {
            get
            {
                UpdateBounds();
                switch (direction)
                {
                    case Direction.Vertical:
                        if (_contentBounds.size.y <= _viewBounds.size.y)
                            return (_viewBounds.min.y > _contentBounds.min.y) ? 1 : 0;
                        return (_viewBounds.min.y - _contentBounds.min.y) / (_contentBounds.size.y - _viewBounds.size.y);
                    case Direction.Horizontal:
                        if (_contentBounds.size.x <= _viewBounds.size.x)
                            return (_viewBounds.min.x > _contentBounds.min.x) ? 1 : 0;
                        return (_viewBounds.min.x - _contentBounds.min.x) / (_contentBounds.size.x - _viewBounds.size.x);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (direction)
                {
                    case Direction.Vertical:
                        SetNormalizedPosition(value, 1);
                        break;
                    case Direction.Horizontal:
                        SetNormalizedPosition(value, 0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float minWidth => -1;
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float preferredWidth => -1;
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float flexibleWidth => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float minHeight => -1;
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float preferredHeight => -1;
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float flexibleHeight => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual int layoutPriority => -1;

        protected RectTransform ViewRect
        {
            get
            {
                if (_viewRect == null)
                    _viewRect = viewport;
                if (_viewRect == null)
                    _viewRect = (RectTransform)transform;
                return _viewRect;
            }
        }

        private RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        private bool ScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                {
                    switch (direction)
                    {
                        case Direction.Vertical:
                            return _contentBounds.size.y > _viewBounds.size.y + 0.01f;
                        case Direction.Horizontal:
                            return _contentBounds.size.x > _viewBounds.size.x + 0.01f;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal() { }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputVertical() { }

        public virtual void LayoutComplete() { }

        public virtual void GraphicUpdateComplete() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executing"></param>
        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                _hasRebuiltLayout = true;
            }
        }
        /// <summary>
        /// Handling for when the content is beging being dragged.
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            pointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position, eventData.pressEventCamera, out pointerStartLocalCursor);
            contentStartPosition = content.anchoredPosition;
            _dragging = true;
        }
        /// <summary>
        /// Handling for when the content is dragged.
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            UpdateBounds();

            var pointerDelta = localCursor - pointerStartLocalCursor;
            Vector2 position = contentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(position - content.anchoredPosition);
            position += offset;
            if (movementType == MovementType.Elastic)
            {
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, _viewBounds.size.x);
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, _viewBounds.size.y);
            }

            SetContentAnchoredPosition(position);
        }
        /// <summary>
        /// Handling for when the content has finished being dragged.
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _dragging = false;
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _velocity = Vector2.zero;
        }

        public virtual void OnScroll(PointerEventData eventData)
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = eventData.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;

            switch (direction)
            {
                case Direction.Vertical:
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        delta.y = delta.x;
                    delta.x = 0;
                    break;
                case Direction.Horizontal:
                    if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                        delta.x = delta.y;
                    delta.y = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Vector2 position = content.anchoredPosition;
            position += delta * scrollSensitivity;
            if (movementType == MovementType.Clamped)
                position += CalculateOffset(position - content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void SetLayoutHorizontal()
        {
            _tracker.Clear();

            if (_sliderExpand || _sliderExpand)
            {
                _tracker.Add(this, ViewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                ViewRect.anchorMin = Vector2.zero;
                ViewRect.anchorMax = Vector2.one;
                ViewRect.sizeDelta = Vector2.zero;
                ViewRect.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
                _contentBounds = GetBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (_sliderExpand && ScrollingNeeded)
            {
                switch (direction)
                {
                    case Direction.Vertical:
                        ViewRect.sizeDelta = new Vector2(-(_sliderLength + scrollbarSpacing), ViewRect.sizeDelta.y);
                        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                        break;
                    case Direction.Horizontal:
                        ViewRect.sizeDelta = new Vector2(ViewRect.sizeDelta.x, -(_sliderLength + scrollbarSpacing));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
                _contentBounds = GetBounds();
                if (direction == Direction.Vertical && ViewRect.sizeDelta.x == 0 && ViewRect.sizeDelta.y < 0)
                    ViewRect.sizeDelta = new Vector2(-(_sliderLength + scrollbarSpacing), ViewRect.sizeDelta.y);

            }
        }

        void ILayoutController.SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
            _contentBounds = GetBounds();
        }

        /// <summary>
        /// Sets the anchored position of the content.
        /// </summary>
        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            switch (direction)
            {
                case Direction.Vertical:
                    position.x = content.anchoredPosition.x;
                    break;
                case Direction.Horizontal:
                    position.y = content.anchoredPosition.y;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (position != content.anchoredPosition)
            {
                content.anchoredPosition = position;
                UpdateBounds();
            }
        }

        /// <summary>
        /// Calculate the bounds the ScrollRect should be using.
        /// </summary>
        protected void UpdateBounds()
        {
            _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
            _contentBounds = GetBounds();

            if (content == null)
                return;

            Vector3 contentSize = _contentBounds.size;
            Vector3 contentPos = _contentBounds.center;
            var contentPivot = content.pivot;
            AdjustBounds(ref _viewBounds, ref contentPivot, ref contentSize, ref contentPos);
            _contentBounds.size = contentSize;
            _contentBounds.center = contentPos;

            if (movementType == MovementType.Clamped)
            {
                // Adjust content so that content bounds bottom (right side) is never higher (to the left) than the view bounds bottom (right side).
                // top (left side) is never lower (to the right) than the view bounds top (left side).
                // All this can happen if content has shrunk.
                // This works because content size is at least as big as view size (because of the call to InternalUpdateBounds above).
                Vector2 delta = Vector2.zero;
                if (_viewBounds.max.x > _contentBounds.max.x)
                {
                    delta.x = Math.Min(_viewBounds.min.x - _contentBounds.min.x, _viewBounds.max.x - _contentBounds.max.x);
                }
                else if (_viewBounds.min.x < _contentBounds.min.x)
                {
                    delta.x = Math.Max(_viewBounds.min.x - _contentBounds.min.x, _viewBounds.max.x - _contentBounds.max.x);
                }

                if (_viewBounds.min.y < _contentBounds.min.y)
                {
                    delta.y = Math.Max(_viewBounds.min.y - _contentBounds.min.y, _viewBounds.max.y - _contentBounds.max.y);
                }
                else if (_viewBounds.max.y > _contentBounds.max.y)
                {
                    delta.y = Math.Min(_viewBounds.min.y - _contentBounds.min.y, _viewBounds.max.y - _contentBounds.max.y);
                }
                if (delta.sqrMagnitude > float.Epsilon)
                {
                    contentPos = content.anchoredPosition + delta;

                    switch (direction)
                    {
                        case Direction.Vertical:
                            contentPos.x = content.anchoredPosition.x;
                            break;
                        case Direction.Horizontal:
                            contentPos.y = content.anchoredPosition.y;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    AdjustBounds(ref _viewBounds, ref contentPivot, ref contentSize, ref contentPos);
                }
            }
        }

        /// <summary>
        /// Set the horizontal or vertical scroll position as a value between 0 and 1, with 0 being at the left or at the bottom.
        /// </summary>
        /// <param name="value">The position to set, between 0 and 1.</param>
        /// <param name="axis">The axis to set: 0 for horizontal, 1 for vertical.</param>
        protected virtual void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            UpdateBounds();
            // How much the content is larger than the view.
            float hiddenLength = _contentBounds.size[axis] - _viewBounds.size[axis];
            // Where the position of the lower left corner of the content bounds should be, in the space of the view.
            float contentBoundsMinPosition = _viewBounds.min[axis] - value * hiddenLength;
            // The new content localPosition, in the space of the view.
            float newLocalPosition = content.localPosition[axis] + contentBoundsMinPosition - _contentBounds.min[axis];

            Vector3 localPosition = content.localPosition;
            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                content.localPosition = localPosition;
                _velocity[axis] = 0;
                UpdateBounds();
            }
        }
        /// <summary>
        /// Helper function to update the previous data fields on a ScrollRect. Call this before you change data in the ScrollRect.
        /// </summary>
        protected void UpdatePrevData()
        {
            if (content == null) _prevPosition = Vector2.zero;
            else _prevPosition = content.anchoredPosition;
            _prevViewBounds = _viewBounds;
            _prevContentBounds = _contentBounds;
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

        private void UpdateScrollbarLayout()
        {
            if (!_sliderExpand || !scrollbar) return;
            switch (direction)
            {
                case Direction.Vertical:
                    _tracker.Add(this, _scrollbarRect,
                        DrivenTransformProperties.AnchorMinY |
                        DrivenTransformProperties.AnchorMaxY |
                        DrivenTransformProperties.SizeDeltaY |
                        DrivenTransformProperties.AnchoredPositionY);
                    _scrollbarRect.anchorMin = new Vector2(_scrollbarRect.anchorMin.x, 0);
                    _scrollbarRect.anchorMax = new Vector2(_scrollbarRect.anchorMax.x, 1);
                    _scrollbarRect.anchoredPosition = new Vector2(_scrollbarRect.anchoredPosition.x, 0);
                    _scrollbarRect.sizeDelta = new Vector2(_scrollbarRect.sizeDelta.x, 0);
                    break;
                case Direction.Horizontal:
                    _tracker.Add(this, _scrollbarRect,
                        DrivenTransformProperties.AnchorMinX |
                        DrivenTransformProperties.AnchorMaxX |
                        DrivenTransformProperties.SizeDeltaX |
                        DrivenTransformProperties.AnchoredPositionX);
                    _scrollbarRect.anchorMin = new Vector2(0, _scrollbarRect.anchorMin.y);
                    _scrollbarRect.anchorMax = new Vector2(1, _scrollbarRect.anchorMax.y);
                    _scrollbarRect.anchoredPosition = new Vector2(0, _scrollbarRect.anchoredPosition.y);
                    _scrollbarRect.sizeDelta = new Vector2(0, _scrollbarRect.sizeDelta.y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateCachedData()
        {
            var transform = this.transform;
            _scrollbarRect = scrollbar == null ? null : scrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            bool viewIsChild = (ViewRect.parent == transform);
            bool scrollbarIsChild = (!_scrollbarRect || _scrollbarRect.parent == transform);
            bool allAreChildren = (viewIsChild && scrollbarIsChild);

            _sliderExpand = allAreChildren && _scrollbarRect && scrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            switch (direction)
            {
                case Direction.Vertical:
                    _sliderLength = (_scrollbarRect == null ? 0 : _scrollbarRect.rect.height);
                    break;
                case Direction.Horizontal:
                    _sliderLength = (_scrollbarRect == null ? 0 : _scrollbarRect.rect.width);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetNormalizedPosition(float value)
        {
            switch (direction)
            {
                case Direction.Vertical:
                    SetNormalizedPosition(value, 1);
                    break;
                case Direction.Horizontal:
                    SetNormalizedPosition(value, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void UpdateScrollbars(Vector2 offset)
        {
            if (scrollbar)
            {
                if (_contentBounds.size.x > 0)
                    scrollbar.size = Mathf.Clamp01((_viewBounds.size.x - Mathf.Abs(offset.x)) / _contentBounds.size.x);
                else
                    scrollbar.size = 1;

                scrollbar.value = NormalizedPosition;
            }
        }

        private void EnsureLayoutHasRebuilt()
        {
            if (!_hasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        private Bounds GetBounds()
        {
            if (content == null)
                return new Bounds();
            content.GetWorldCorners(_corners);
            var viewWorldToLocalMatrix = _viewRect.worldToLocalMatrix;
            return InternalGetBounds(_corners, ref viewWorldToLocalMatrix);
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            return InternalCalculateOffset(ref _viewBounds, ref _contentBounds, direction, movementType, ref delta);
        }

        internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds, Direction direction, MovementType movementType, ref Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
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
                    throw new ArgumentOutOfRangeException();

            }
            return offset;
        }
        internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
        {
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize, ref Vector3 contentPos)
        {
            Vector3 excess = viewBounds.size - contentSize;
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

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }
    }
}