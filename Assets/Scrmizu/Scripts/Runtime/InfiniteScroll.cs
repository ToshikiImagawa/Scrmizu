using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Scrmizu
{
    public class InfiniteScroll : ScrollRect
    {
        /// <summary>
        /// Direction of scroll.
        /// </summary>
        [SerializeField, Tooltip("Direction of scroll.")]
        private Direction direction;

        /// <summary>
        /// Reverse direction scroll.
        /// </summary>
        [SerializeField, Tooltip("Reverse direction scroll.")]
        private bool isReverse;

        /// <summary>
        /// The contentItem that can be instantiated in the scroll area.
        /// </summary>
        [SerializeField, Tooltip("The contentItem that can be instantiated in the scroll area.")]
        private MonoBehaviour itemBase;

        /// <summary>
        /// Item size when default display.
        /// </summary>
        [SerializeField, Tooltip("Item size when default display.")]
        private float defaultItemSize;

        /// <summary>
        /// Item interval.
        /// </summary>
        [SerializeField, Tooltip("Item interval.")]
        private float itemInterval;

        /// <summary>
        /// Count to be instantiated.
        /// </summary>
        [SerializeField, Range(1, 100), Tooltip("Count to be instantiated.")]
        private int instantiatedItemCount = 9;

        private InfiniteScrollBinder[] _infiniteScrollBinders;

        private float _currentPosition;
        private int _currentBinderIndex;
        private float _currentItemIndexPosition;

        private List<object> _itemDataList = new List<object>();
        private List<float> _itemSizeList = new List<float>();

        /// <summary>
        /// Gets the index of the current item.
        /// </summary>
        /// <value>The index of the current item.</value>
        public int CurrentItemIndex { get; private set; }

        private InfiniteScrollBinder[] InfiniteScrollBinders
        {
            get
            {
                if (_infiniteScrollBinders != null) return _infiniteScrollBinders;
                if (content == null) throw new Exception("Content is not set.");
                if (itemBase == null) throw new Exception("ItemBase is not set.");
                _infiniteScrollBinders = new InfiniteScrollBinder[instantiatedItemCount];
                for (var i = 0; i < instantiatedItemCount; i++)
                {
                    var item = Instantiate(itemBase, content, false);
                    item.name = $"{nameof(itemBase)}_{i}";
                    _infiniteScrollBinders[i] = item.gameObject.AddComponent<InfiniteScrollBinder>();
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
            StartCoroutine(UpdateCanvas());
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
            StartCoroutine(UpdateCanvas());
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
            StartCoroutine(UpdateCanvas());
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

            if (CurrentItemIndex >= index) MovePosition(_currentPosition + defaultItemSize + itemInterval);

            UpdateContents();
            StartCoroutine(UpdateCanvas());
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
                MovePosition(_currentPosition + addSize);
            }

            UpdateContents();
            StartCoroutine(UpdateCanvas());
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
                MovePosition(_currentPosition - removeSize);
            }
            UpdateContents();
            StartCoroutine(UpdateCanvas());
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
                MovePosition(_currentPosition - removeSize);
            }
            UpdateContents();
            StartCoroutine(UpdateCanvas());
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
            StartCoroutine(UpdateCanvas());
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
            _currentPosition = position;
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
        /// <returns>Positin</returns>
        /// <param name="index">Index.</param>
        public float GetPositionAt(int index)
        {
            if (index < 0 || index > _itemSizeList.Count) throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            if (index == 0) return 0f;
            var itemSizeList = _itemSizeList.ToArray();
            var itemSizeRange = new float[index];
            Array.Copy(itemSizeList, 0, itemSizeRange, 0, index);
            return itemSizeRange.Sum() + itemInterval * index;
        }

        internal void UpdateItemSize(InfiniteScrollBinder binder)
        {
            if (binder.ItemIndex < 0 || binder.ItemIndex > _itemSizeList.Count) return;
            float size;
            switch (direction)
            {
                case Direction.Vertical:
                    size = binder.size.y;
                    break;
                case Direction.Horizontal:
                    size = binder.size.x;
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
            if (!Application.isPlaying) return;
            for (var i = 0; i < InfiniteScrollBinders.Length; i++)
            {
                InfiniteScrollBinders[i].UpdateSize();
            }
        }

        private void OnScrollMove(Vector2 call)
        {
            var pos = content.anchoredPosition;
            switch (direction)
            {
                case Direction.Vertical:
                    if (isReverse)
                        _currentPosition = -1 * pos.y;
                    else _currentPosition = pos.y;
                    break;
                case Direction.Horizontal:
                    if (isReverse)
                        _currentPosition = pos.x;
                    else _currentPosition = -1 * pos.x;
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
            var smallItemIndexPosition = 0f;
            var bigItemIndexPosition = itemSizeList.Sum() + itemInterval * (itemSizeList.Length - 1);

            if(bigItemIndexPosition <= _currentPosition)
            {
                CurrentItemIndex = itemSizeList.Length - 1;
                _currentItemIndexPosition = bigItemIndexPosition;
            }
            else if(_currentPosition < 0)
            {
                CurrentItemIndex = 0;
                _currentItemIndexPosition = 0;
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

                    if(middleItemIndexPosition <= _currentPosition)
                    {
                        smallItemIndex = middleItemIndex;
                        smallItemIndexPosition = middleItemIndexPosition;
                    }
                    else if (middleItemIndexPosition > _currentPosition)
                    {
                        bigItemIndex = middleItemIndex;
                        bigItemIndexPosition = middleItemIndexPosition;
                    }
                    i++;
                }
                CurrentItemIndex = smallItemIndex;
                _currentItemIndexPosition = smallItemIndexPosition;
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

        private IEnumerator UpdateCanvas()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Direction type
    /// </summary>
    public enum Direction
    {
        Vertical = 0,
        Horizontal = 1,
    }
}