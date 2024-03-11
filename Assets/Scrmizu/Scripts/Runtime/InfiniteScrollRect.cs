// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Collections;
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

        private int _currentBinderIndex = -1;
        private bool _isUpdateCanvasRequest;

        private DefaultValueList _itemSizeList;

        private DefaultValueList ItemSizeList => _itemSizeList ??= new DefaultValueList(defaultItemSize);

        /// <summary>
        /// Gets the index of the current item.
        /// </summary>
        /// <value>The index of the current item.</value>
        public int CurrentItemIndex { get; private set; } = -1;

        /// <summary>
        /// Get the position of the current item.　
        /// </summary>
        /// <value>The position of the current item.</value>
        public float CurrentPosition { get; private set; }

        /// <summary>
        /// Get count of items.
        /// </summary>
        /// <value>Count of items.</value>
        public int Count => ItemSizeList.Count;

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
                Assert.IsNotNull(viewport, "Viewport is not set.");
                var contentRect = content.rect;
                var viewportRect = viewport.rect;
                return direction switch
                {
                    Direction.Vertical => contentRect.height - viewportRect.height,
                    Direction.Horizontal => contentRect.width - viewportRect.width,
                    _ => throw new ArgumentOutOfRangeException()
                };
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
            InfiniteScrollItemRepository ??= new StandardInfiniteScrollItemRepository();

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
            InnerInfiniteScrollItemRepository.AddRange(data ?? Array.Empty<object>());
            _itemSizeList = new DefaultValueList(defaultItemSize, InnerInfiniteScrollItemRepository.Count);
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
            ItemSizeList.Add();
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
            ItemSizeList.AddRange(item.Length);
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
            ItemSizeList.Insert(index);

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
            ItemSizeList.InsertRange(index, item.Length);

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
            if (index > ItemSizeList.Count) throw new ArgumentException("Deleted range is out of range.");
            var itemIndex = CurrentItemIndex;
            var itemSizeList = ItemSizeList.ToArray();
            InnerInfiniteScrollItemRepository.RemoveAt(index);
            ItemSizeList.RemoveAt(index);

            if (itemIndex == index)
            {
                MovePositionAt(Math.Min(ItemSizeList.Count - 1, index));
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
            if (count + index > ItemSizeList.Count) throw new ArgumentException("Deleted range is out of range.");
            var itemIndex = CurrentItemIndex;
            var itemSizeList = ItemSizeList.ToArray();
            InnerInfiniteScrollItemRepository.RemoveRange(index, count);
            ItemSizeList.RemoveRange(index, count);

            if (itemIndex >= index && itemIndex <= index + count - 1)
            {
                MovePositionAt(Math.Min(ItemSizeList.Count - 1, index));
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
            ItemSizeList.Clear();
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
            if (index < 0 || index > ItemSizeList.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            if (index == 0) return 0f;
            var itemSizeList = ItemSizeList.ToArray();
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
            if (index < 0 || index >= ItemSizeList.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            return ItemSizeList[index];
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
            if (binder.ItemIndex < 0 || binder.ItemIndex >= ItemSizeList.Count) return;
            var size = direction switch
            {
                Direction.Vertical => binder.Size.y,
                Direction.Horizontal => binder.Size.x,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (ItemSizeList[binder.ItemIndex].Equals(size)) return;
            ItemSizeList[binder.ItemIndex] = size;
        }

        internal bool IsInitialized(InfiniteScrollBinderBase binder)
        {
            if (binder.ItemIndex < 0 || binder.ItemIndex >= ItemSizeList.Count) return false;
            return ItemSizeList.Contains(binder.ItemIndex);
        }

        protected override void Awake()
        {
            base.Awake();
            if (!Application.isPlaying) return;
            onValueChanged.AddListener(OnScrollMove);
            var itemSizeList = ItemSizeList.ToArray();
            UpdateCurrentItemIndex(itemSizeList);
            UpdateBinder(itemSizeList);
        }

        protected virtual void Update()
        {
            if (Application.isPlaying)
            {
                foreach (var binder in InfiniteScrollBinders)
                {
                    binder.UpdateSize();
                }

                UpdateContents();
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

            var itemSizeList = ItemSizeList.ToArray();
            var updated = UpdateCurrentItemIndex(itemSizeList);
            // 変更なければスキップ
            if (!updated) return;
            UpdateBinder(itemSizeList);
        }

        private void UpdateContents()
        {
            var contentRectTransform = content;
            Assert.IsNotNull(contentRectTransform, "Content is not set.");
            var currentSizeDelta = contentRectTransform.sizeDelta;
            var contentFullSize = direction switch
            {
                Direction.Vertical => currentSizeDelta.y,
                Direction.Horizontal => currentSizeDelta.x,
                _ => throw new ArgumentOutOfRangeException()
            };
            var newFullSize = ItemSizeList.Sum() + itemInterval * (ItemSizeList.Count - 1);
            var newSizeDelta = direction switch
            {
                Direction.Vertical => new Vector2(currentSizeDelta.x, newFullSize),
                Direction.Horizontal => new Vector2(newFullSize, currentSizeDelta.y),
                _ => throw new ArgumentOutOfRangeException()
            };

            // サイズに変更がなければスキップ
            if (!(Math.Abs(contentFullSize - newFullSize) > float.Epsilon)) return;
            contentRectTransform.sizeDelta = newSizeDelta;
            var itemSizeList = ItemSizeList.ToArray();
            UpdateCurrentItemIndex(itemSizeList);
            UpdateBinder(itemSizeList);
        }

        /// <summary>
        /// Update current itemIndex
        /// </summary>
        /// <param name="itemSizeList"></param>
        private bool UpdateCurrentItemIndex(float[] itemSizeList)
        {
            var smallItemIndex = 0;
            var bigItemIndex = itemSizeList.Length - 1;
            var bigItemIndexPosition = itemSizeList.Sum() + itemInterval * (itemSizeList.Length - 1);

            var beforeItemIndex = CurrentItemIndex;
            var beforeBinderIndex = _currentBinderIndex;
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
            return beforeItemIndex != CurrentItemIndex || beforeBinderIndex != _currentBinderIndex;
        }

        /// <summary>
        /// Update binder.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void UpdateBinder(float[] itemSizeList)
        {
            var data = InnerInfiniteScrollItemRepository.ToArray();
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

                if (!isReverse) pos *= -1;
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

        private class DefaultValueList : IEnumerable<float>
        {
            private readonly List<float?> _list;
            private readonly float _defaultValue;
            public int Count => _list.Count;

            public float this[int index]
            {
                set => _list[index] = value;
                get => _list[index] ?? _defaultValue;
            }

            public DefaultValueList(float defaultValue)
            {
                _defaultValue = defaultValue;
                _list = new List<float?>();
            }

            public DefaultValueList(float defaultValue, int count)
            {
                _defaultValue = defaultValue;
                _list = Enumerable.Repeat((float?)null, count).ToList();
            }

            public bool Contains(int index)
            {
                if (index < 0 || index >= Count) return false;
                return _list[index].HasValue;
            }

            public void Add()
            {
                _list.Add(null);
            }

            public void AddRange(int count)
            {
                _list.AddRange(Enumerable.Repeat((float?)null, count));
            }

            public void Insert(int index)
            {
                _list.Insert(index, null);
            }

            public void InsertRange(int index, int count)
            {
                _list.InsertRange(index, Enumerable.Repeat((float?)null, count));
            }

            public void RemoveAt(int index)
            {
                _list.RemoveAt(index);
            }

            public void RemoveRange(int index, int count)
            {
                _list.RemoveRange(index, count);
            }

            public void Clear()
            {
                _list.Clear();
            }

            public IEnumerator<float> GetEnumerator()
            {
                foreach (var item in _list)
                {
                    yield return item ?? _defaultValue;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
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