// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Scrmizu
{
    [AddComponentMenu("UI/Infinite Scroll Rect", 37)]
    [SelectionBase]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
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

        private List<float> _itemSizeList = new List<float>();

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
                Assert.IsNotNull(content, "Content is not set.");
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
        protected virtual IInfiniteScrollElementFactory InfiniteScrollElementFactory { get; set; }

        /// <summary>
        /// Repository to 
        /// </summary>
        /// <value>Repository.</value>
        protected virtual IInfiniteScrollItemRepository InfiniteScrollItemRepository { get; set; }

        private IInfiniteScrollElementFactory InnerInfiniteScrollElementFactory
        {
            get
            {
                if (InfiniteScrollElementFactory != null) return InfiniteScrollElementFactory;
                Assert.IsNotNull(content, "Content is not set.");
                Assert.IsNotNull(itemBase, "ItemBase is not set.");
                return InfiniteScrollElementFactory = new StandardInfiniteScrollElementFactory(itemBase, content);
            }
        }

        private IInfiniteScrollItemRepository InnerInfiniteScrollItemRepository =>
            InfiniteScrollItemRepository ??
            (InfiniteScrollItemRepository = new StandardInfiniteScrollItemRepository());

        private InfiniteScrollBinderBase[] InfiniteScrollBinders
        {
            get
            {
                if (_infiniteScrollBinders != null) return _infiniteScrollBinders;
                _infiniteScrollBinders = new InfiniteScrollBinderBase[instantiatedItemCount];
                for (var i = 0; i < instantiatedItemCount; i++)
                {
                    _infiniteScrollBinders[i] = InnerInfiniteScrollElementFactory.Create();
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
            InnerInfiniteScrollItemRepository.Clear();
            InnerInfiniteScrollItemRepository.AddRange(data ?? new object[0]);
            _itemSizeList = Enumerable.Repeat(defaultItemSize, InnerInfiniteScrollItemRepository.Count).ToList();
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
            InnerInfiniteScrollItemRepository.Add(data);
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
            InnerInfiniteScrollItemRepository.AddRange(item);
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
            InnerInfiniteScrollItemRepository.Insert(index, data);
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
            InnerInfiniteScrollItemRepository.InsertRange(index, item);
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
            InnerInfiniteScrollItemRepository.RemoveAt(index);
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
            InnerInfiniteScrollItemRepository.RemoveRange(index, count);
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
            InnerInfiniteScrollItemRepository.Clear();
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
            var contentRectTransform = content;
            Assert.IsNotNull(contentRectTransform, "Content is not set.");
            switch (direction)
            {
                case Direction.Vertical:
                    if (isReverse)
                        contentRectTransform.anchoredPosition = new Vector2(
                            contentRectTransform.anchoredPosition.x,
                            -1 * position
                        );
                    else
                        contentRectTransform.anchoredPosition = new Vector2(
                            contentRectTransform.anchoredPosition.x,
                            position
                        );
                    break;
                case Direction.Horizontal:
                    if (isReverse)
                        contentRectTransform.anchoredPosition = new Vector2(
                            position,
                            contentRectTransform.anchoredPosition.y
                        );
                    else
                        contentRectTransform.anchoredPosition = new Vector2(
                            -1 * position,
                            contentRectTransform.anchoredPosition.y
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
            if (index < 0 || index > InnerInfiniteScrollItemRepository.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            return InnerInfiniteScrollItemRepository[index];
        }

        internal void UpdateItemSize(InfiniteScrollBinderBase binder)
        {
            if (binder.ItemIndex < 0 || binder.ItemIndex >= _itemSizeList.Count) return;
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
                foreach (var binder in InfiniteScrollBinders)
                {
                    binder.UpdateSize();
                }
            }

            UpdateCanvas();
        }

        private void OnScrollMove(Vector2 call)
        {
            Assert.IsNotNull(content, "Content is not set.");
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
            var contentRectTransform = content;
            Assert.IsNotNull(contentRectTransform, "Content is not set.");
            var fullSize = _itemSizeList.Sum() + itemInterval * (_itemSizeList.Count - 1);
            switch (direction)
            {
                case Direction.Vertical:
                    contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, fullSize);
                    break;
                case Direction.Horizontal:
                    contentRectTransform.sizeDelta = new Vector2(fullSize, contentRectTransform.sizeDelta.y);
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
                    }
                }

                CurrentItemIndex = smallItemIndex;
            }

            _currentBinderIndex = CurrentItemIndex % instantiatedItemCount;
            UpdateBinder();
        }

        private void UpdateBinder()
        {
            var data = InnerInfiniteScrollItemRepository.ToArray();
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
    }

    public sealed class StandardInfiniteScrollElementFactory : IInfiniteScrollElementFactory
    {
        private readonly MonoBehaviour _itemBase;
        private readonly Transform _content;
        private int _index;

        public StandardInfiniteScrollElementFactory(MonoBehaviour itemBase, Transform content)
        {
            _itemBase = itemBase;
            _content = content;
        }

        InfiniteScrollBinderBase IInfiniteScrollElementFactory.Create()
        {
            var item = UnityEngine.Object.Instantiate(_itemBase, _content, false);
            item.name = $"{_itemBase.gameObject.name}_{_index++}";
            return item.gameObject.GetComponent<InfiniteScrollBinderBase>() ??
                   item.gameObject.AddComponent<InfiniteScrollBinder>();
        }
    }

    public sealed class StandardInfiniteScrollItemRepository : IInfiniteScrollItemRepository
    {
        private readonly List<object> _cacheItemData = new List<object>();
        public int Count => _cacheItemData.Count;

        public object this[int index] => _cacheItemData[index];

        public void Add(object item)
        {
            _cacheItemData.Add(item);
        }

        public void AddRange(IEnumerable<object> collection)
        {
            _cacheItemData.AddRange(collection);
        }

        public void Insert(int index, object item)
        {
            _cacheItemData.Insert(index, item);
        }

        public void InsertRange(int index, IEnumerable<object> collection)
        {
            _cacheItemData.InsertRange(index, collection);
        }

        public void RemoveAt(int index)
        {
            _cacheItemData.RemoveAt(index);
        }

        public void RemoveRange(int index, int count)
        {
            _cacheItemData.RemoveRange(index, count);
        }

        public void Clear()
        {
            _cacheItemData.Clear();
        }

        public object[] ToArray()
        {
            return _cacheItemData.ToArray();
        }
    }

    public interface IInfiniteScrollElementFactory
    {
        /// <summary>
        /// Create InfiniteScrollBinder.
        /// </summary>
        /// <returns>InfiniteScrollBinder.</returns>
        InfiniteScrollBinderBase Create();
    }

    public interface IInfiniteScrollItemRepository
    {
        int Count { get; }
        object this[int index] { get; }
        void Add(object item);
        void AddRange(IEnumerable<object> collection);
        void Insert(int index, object item);
        void InsertRange(int index, IEnumerable<object> collection);
        void RemoveAt(int index);
        void RemoveRange(int index, int count);
        void Clear();
        object[] ToArray();
    }
}