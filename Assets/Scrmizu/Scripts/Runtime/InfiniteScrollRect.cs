using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrmizu
{
    [AddComponentMenu("UI/Infinite Scroll Rect")]
    public class InfiniteScrollRect : NestedScrollRect
    {
        /// <summary>
        /// Direction of scroll.
        /// </summary>
        [SerializeField, Tooltip("Direction of scroll.")]
        private Direction direction = Direction.Vertical;

        /// <summary>
        /// Reverse direction scroll.
        /// </summary>
        [SerializeField, Tooltip("Reverse direction scroll.")]
        private bool isReverse = false;

        /// <summary>
        /// The contentItem that can be instantiated in the scroll area.
        /// </summary>
        [SerializeField, Tooltip("The contentItem that can be instantiated in the scroll area.")]
        private MonoBehaviour itemBase = null;

        /// <summary>
        /// Item size when default display.
        /// </summary>
        [SerializeField, Tooltip("Item size when default display.")]
        private float defaultItemSize = 10f;

        /// <summary>
        /// Item interval.
        /// </summary>
        [SerializeField, Tooltip("Item interval.")]
        private float itemInterval = 10f;

        /// <summary>
        /// Count to be instantiated.
        /// </summary>
        [SerializeField, Range(1, 100), Tooltip("Count to be instantiated.")]
        private int instantiatedItemCount = 9;

        /// <summary>
        /// Is nested scroll.
        /// </summary>
        [SerializeField, Tooltip("Is nested scroll.")]
        private bool isNestedScroll = true;

        private InfiniteScrollBinderBase[] _infiniteScrollBinders;

        private int _currentBinderIndex;
        private bool _isUpdateCanvasRequest;

        private List<object> _itemDataList = new List<object>();
        private List<float> _itemSizeList = new List<float>();

        protected IInfiniteScrollElementFactory _infiniteScrollElementFactory;

        /// <summary>
        /// Gets the index of the current item.
        /// </summary>
        /// <value>The index of the current item.</value>
        public int CurrentItemIndex { get; private set; }

        /// <summary>
        /// Get the position of the current item.　
        /// </summary>
        /// <value>The position of the current item.</value>
        public float CurrentPosition { get; private set; }

        /// <summary>
        /// Get count of items.
        /// </summary>
        /// <value>Count of items.</value>
        public int Count => _itemSizeList.Count;

        /// <summary>
        /// Max value that can be scrolled.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <value>The max scrollable position.</value>
        public float MaxScrollPosition
        {
            get
            {
                switch (direction)
                {
                    case Direction.Vertical:
                        return content.rect.height - viewport.rect.height;
                    case Direction.Horizontal:
                        return content.rect.width - viewport.rect.width;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected internal override bool IsNestedScroll => isNestedScroll;

        /// <summary>
        /// Factory to create InfiniteScrollRect elements.
        /// </summary>
        /// <value>Factory.</value>
        protected virtual IInfiniteScrollElementFactory InfiniteScrollElementFactory =>
            _infiniteScrollElementFactory != null ? _infiniteScrollElementFactory :
            (_infiniteScrollElementFactory = new StandardInfiniteScrollElementFactory(itemBase, content));

        private InfiniteScrollBinderBase[] InfiniteScrollBinders
        {
            get
            {
                if (_infiniteScrollBinders != null) return _infiniteScrollBinders;
                if (content == null) throw new Exception("Content is not set.");
                if (itemBase == null) throw new Exception("ItemBase is not set.");
                _infiniteScrollBinders = new InfiniteScrollBinderBase[instantiatedItemCount];
                for (var i = 0; i < instantiatedItemCount; i++)
                {
                    _infiniteScrollBinders[i] = InfiniteScrollElementFactory.Create();
                    _infiniteScrollBinders[i].SetInfiniteScroll(this);
                    _infiniteScrollBinders[i].Hide();
                }

                return _infiniteScrollBinders;
            }
        }

        /// <summary>
        /// Sets the item data.
        /// </summary>
        /// <param name="data">Data.</param>
        public void SetItemData(IEnumerable<object> data)
        {
            _itemDataList = data.ToList();
            _itemSizeList = Enumerable.Repeat(defaultItemSize, _itemDataList.Count).ToList();
            MovePositionAt(0);
            UpdateContents();
            _isUpdateCanvasRequest = true;
        }

        /// <summary>
        /// Adds the item data.
        /// </summary>
        /// <param name="data">Data.</param>
        public void AddItemData(object data)
        {
            _itemDataList.Add(data);
            _itemSizeList.Add(defaultItemSize);
            UpdateContents();
            _isUpdateCanvasRequest = true;
        }

        /// <summary>
        /// Adds the range item data.
        /// </summary>
        /// <param name="data">Data.</param>
        public void AddRangeItemData(IEnumerable<object> data)
        {
            var item = data.ToArray();
            _itemDataList.AddRange(item);
            _itemSizeList.AddRange(Enumerable.Repeat(defaultItemSize, item.Length));
            UpdateContents();
            _isUpdateCanvasRequest = true;
        }

        /// <summary>
        /// Inserts the item data.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="data">Data.</param>
        public void InsertItemData(int index, object data)
        {
            _itemDataList.Insert(index, data);
            _itemSizeList.Insert(index, defaultItemSize);

            if (CurrentItemIndex >= index) MovePosition(CurrentPosition + defaultItemSize + itemInterval);

            UpdateContents();
            _isUpdateCanvasRequest = true;
        }

        /// <summary>
        /// Inserts the range item data.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="data">Data.</param>
        public void InsertRangeItemData(int index, IEnumerable<object> data)
        {
            var item = data.ToArray();
            _itemDataList.InsertRange(index, item);
            _itemSizeList.InsertRange(index, Enumerable.Repeat(defaultItemSize, item.Length));

            if (CurrentItemIndex >= index)
            {
                var addSize = (defaultItemSize + itemInterval) * item.Length;
                MovePosition(CurrentPosition + addSize);
            }

            UpdateContents();
            _isUpdateCanvasRequest = true;
        }

        /// <summary>
        /// Removes the item data.
        /// </summary>
        /// <param name="index">Index.</param>
        public void RemoveAtItemData(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            if (index > _itemSizeList.Count) throw new ArgumentException("Deleted range is out of range.");
            var itemIndex = CurrentItemIndex;
            var itemSizeList = _itemSizeList.ToArray();
            _itemDataList.RemoveAt(index);
            _itemSizeList.RemoveAt(index);

            if (itemIndex == index)
            {
                MovePositionAt(Math.Min(_itemSizeList.Count - 1, index));
            }
            else if (itemIndex > index)
            {
                var removeSize = itemSizeList[index] + itemInterval;
                MovePosition(CurrentPosition - removeSize);
            }

            UpdateContents();
            _isUpdateCanvasRequest = true;
        }

        /// <summary>
        /// Removes the range item data.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="count">Count.</param>
        public void RemoveRangeItemData(int index, int count)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count is out of range.");
            if ((count + index) > _itemSizeList.Count) throw new ArgumentException("Deleted range is out of range.");
            var itemIndex = CurrentItemIndex;
            var itemSizeList = _itemSizeList.ToArray();
            _itemDataList.RemoveRange(index, count);
            _itemSizeList.RemoveRange(index, count);

            if (itemIndex >= index && itemIndex <= index + count - 1)
            {
                MovePositionAt(Math.Min(_itemSizeList.Count - 1, index));
            }
            else if (itemIndex > index)
            {
                var itemSizeRange = new float[count];
                Array.Copy(itemSizeList, index, itemSizeRange, 0, count);
                var removeSize = itemSizeRange.Sum() + itemInterval * (count);
                MovePosition(CurrentPosition - removeSize);
            }

            UpdateContents();
            _isUpdateCanvasRequest = true;
        }

        /// <summary>
        /// Clears the item data.
        /// </summary>
        public void ClearItemData()
        {
            _itemDataList.Clear();
            _itemSizeList.Clear();
            MovePositionAt(0);
            UpdateContents();
            _isUpdateCanvasRequest = true;
        }

        /// <summary>
        /// Moves the position.
        /// </summary>
        /// <param name="position">Position.</param>
        public void MovePosition(float position)
        {
            switch (direction)
            {
                case Direction.Vertical:
                    if (isReverse)
                        content.anchoredPosition = new Vector2(
                            content.anchoredPosition.x,
                            -1 * position
                        );
                    else
                        content.anchoredPosition = new Vector2(
                            content.anchoredPosition.x,
                            position
                        );
                    break;
                case Direction.Horizontal:
                    if (isReverse)
                        content.anchoredPosition = new Vector2(
                            position,
                            content.anchoredPosition.y
                        );
                    else
                        content.anchoredPosition = new Vector2(
                            -1 * position,
                            content.anchoredPosition.y
                        );
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CurrentPosition = position;
        }

        /// <summary>
        /// Moves the position at index.
        /// </summary>
        /// <param name="index">Index.</param>
        public void MovePositionAt(int index)
        {
            MovePosition(GetPositionAt(index));
        }

        /// <summary>
        /// Gets the position at index.
        /// </summary>
        /// <returns>Position.</returns>
        /// <param name="index">Index.</param>
        public float GetPositionAt(int index)
        {
            if (index < 0 || index > _itemSizeList.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            if (index == 0) return 0f;
            var itemSizeList = _itemSizeList.ToArray();
            var itemSizeRange = new float[index];
            Array.Copy(itemSizeList, 0, itemSizeRange, 0, index);
            return itemSizeRange.Sum() + itemInterval * index;
        }

        /// <summary>
        /// Gets the size at index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Size.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float GetItemSize(int index)
        {
            if (index < 0 || index >= _itemSizeList.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            return _itemSizeList[index];
        }

        /// <summary>
        /// Gets the item data at index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Item data.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public object GetItemData(int index)
        {
            if (index < 0 || index > _itemDataList.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            return _itemDataList[index];
        }

        internal void UpdateItemSize(InfiniteScrollBinderBase binder)
        {
            if (binder.ItemIndex < 0 || binder.ItemIndex > _itemSizeList.Count) return;
            float size;
            switch (direction)
            {
                case Direction.Vertical:
                    size = binder.Size.y;
                    break;
                case Direction.Horizontal:
                    size = binder.Size.x;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_itemSizeList[binder.ItemIndex].Equals(size)) return;
            _itemSizeList[binder.ItemIndex] = size;
            UpdateContents();
        }

        protected override void Awake()
        {
            base.Awake();
            if (!Application.isPlaying) return;
            onValueChanged.AddListener(OnScrollMove);

            UpdatePosition();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (Application.isPlaying)
            {
                for (var i = 0; i < InfiniteScrollBinders.Length; i++)
                {
                    InfiniteScrollBinders[i].UpdateSize();
                }
            }

            UpdateCanvas();
        }

        private void OnScrollMove(Vector2 call)
        {
            var pos = content.anchoredPosition;
            switch (direction)
            {
                case Direction.Vertical:
                    if (isReverse)
                        CurrentPosition = -1 * pos.y;
                    else CurrentPosition = pos.y;
                    break;
                case Direction.Horizontal:
                    if (isReverse)
                        CurrentPosition = pos.x;
                    else CurrentPosition = -1 * pos.x;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdatePosition();
        }

        private void UpdateContents()
        {
            var fullSize = _itemSizeList.Sum() + itemInterval * (_itemSizeList.Count - 1);
            switch (direction)
            {
                case Direction.Vertical:
                    content.sizeDelta = new Vector2(content.sizeDelta.x, fullSize);
                    break;
                case Direction.Horizontal:
                    content.sizeDelta = new Vector2(fullSize, content.sizeDelta.y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            var itemSizeList = _itemSizeList.ToArray();

            var smallItemIndex = 0;
            var bigItemIndex = itemSizeList.Length - 1;
            var bigItemIndexPosition = itemSizeList.Sum() + itemInterval * (itemSizeList.Length - 1);

            if (bigItemIndexPosition <= CurrentPosition)
            {
                CurrentItemIndex = itemSizeList.Length - 1;
            }
            else if (CurrentPosition < 0)
            {
                CurrentItemIndex = 0;
            }
            else
            {
                var i = 0;
                while (smallItemIndex < (bigItemIndex - 1))
                {
                    var middleItemIndex = (bigItemIndex + smallItemIndex) / 2;
                    var middleItemSizeList = new float[middleItemIndex + 1];
                    Array.Copy(itemSizeList, middleItemSizeList, middleItemIndex + 1);
                    var middleItemIndexPosition = middleItemSizeList.Sum() + itemInterval * middleItemIndex;

                    if (middleItemIndexPosition <= CurrentPosition)
                    {
                        smallItemIndex = middleItemIndex;
                    }
                    else if (middleItemIndexPosition > CurrentPosition)
                    {
                        bigItemIndex = middleItemIndex;
                        bigItemIndexPosition = middleItemIndexPosition;
                    }

                    i++;
                }

                CurrentItemIndex = smallItemIndex;
            }

            _currentBinderIndex = CurrentItemIndex % instantiatedItemCount;
            UpdateBinder();
        }

        private void UpdateBinder()
        {
            var data = _itemDataList.ToArray();
            var itemSizeList = _itemSizeList.ToArray();
            var fullSize = itemSizeList.Sum() + Mathf.Max(itemSizeList.Length - 1, 0f) * itemInterval;

            for (var i = 0; i < InfiniteScrollBinders.Length; i++)
            {
                var index = i;
                var infiniteScrollBinder = InfiniteScrollBinders[index];
                if (_currentBinderIndex > index) index = instantiatedItemCount + index;
                var itemIndex = CurrentItemIndex + index - _currentBinderIndex;
                float pos;
                if (data.Length <= itemIndex || itemIndex < 0)
                {
                    infiniteScrollBinder.Hide();
                    if (itemIndex < 0)
                        pos = -((itemInterval + defaultItemSize) * (0 - itemIndex));
                    else
                        pos = fullSize + (itemIndex - data.Length) * defaultItemSize +
                              (itemIndex - data.Length + 1) * itemInterval;
                }
                else
                {
                    infiniteScrollBinder.UpdateItemData(data[itemIndex], itemIndex);
                    var itemSizeRange = new float[itemIndex];
                    Array.Copy(itemSizeList, 0, itemSizeRange, 0, itemIndex);
                    pos = itemSizeRange.Sum() + itemInterval * itemIndex;
                }

                if (!isReverse) pos = pos * -1;
                switch (direction)
                {
                    case Direction.Vertical:
                        infiniteScrollBinder.UpdateItemPosition(new Vector2(0, pos));
                        break;
                    case Direction.Horizontal:
                        infiniteScrollBinder.UpdateItemPosition(new Vector2(pos * -1, 0));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void UpdateCanvas()
        {
            if (!_isUpdateCanvasRequest) return;
            _isUpdateCanvasRequest = false;
            Canvas.ForceUpdateCanvases();
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        private class InfiniteScrollBinder : InfiniteScrollBinderBase
        {
            private RectTransform _rectTransform;

            private RectTransform RectTransform =>
                _rectTransform != null ? _rectTransform : _rectTransform = GetComponent<RectTransform>();

            public override Vector2 Size
            {
                get
                {
                    var rect = RectTransform.rect;
                    return new Vector2(rect.width, rect.height);
                }
            }

            public override void UpdateItemPosition(Vector2 position)
            {
                RectTransform.anchoredPosition = position;
            }
        }

        private class StandardInfiniteScrollElementFactory : IInfiniteScrollElementFactory
        {
            private readonly MonoBehaviour _itemBase;
            private readonly Transform _content;
            private int i;

            public StandardInfiniteScrollElementFactory(MonoBehaviour itemBase, Transform content)
            {
                _itemBase = itemBase;
                _content = content;
            }

            InfiniteScrollBinderBase IInfiniteScrollElementFactory.Create()
            {
                var item = Instantiate(_itemBase, _content, false);
                item.name = $"{_itemBase.gameObject.name}_{i++}";
                return item.gameObject.GetComponent<InfiniteScrollBinderBase>() ??
                                            item.gameObject.AddComponent<InfiniteScrollBinder>();
            }
        }
    }

    public interface IInfiniteScrollElementFactory
    {
        InfiniteScrollBinderBase Create();
    }
}