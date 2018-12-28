using System;
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


        private float _currentPosition;
        private int _currentItemIndex;
        private int _currentBinderIndex;
        private float _currentItemIndexPosition;

        private List<object> _itemDataList = new List<object>();
        private List<float> _itemSize = new List<float>();

        public void SetItemData(IEnumerable<object> data)
        {
            _itemDataList = data.ToList();
            _itemSize = Enumerable.Repeat(defaultItemSize, _itemDataList.Count).ToList();
            UpdateContents();
        }

        public void AddItemData(object data)
        {
            _itemDataList.Add(data);
            _itemSize.Add(defaultItemSize);
            UpdateContents();
        }

        public void InsertItemData(int index, object data)
        {
            _itemDataList.Insert(index, data);
            _itemSize.Insert(index, defaultItemSize);
            UpdateContents();
        }

        public void RemoveItemData(int index)
        {
            _itemDataList.RemoveAt(index);
            _itemSize.RemoveAt(index);
            UpdateContents();
        }

        internal void UpdateItemSize(InfiniteScrollBinder binder)
        {
            if (binder.ItemIndex < 0 || binder.ItemIndex > _itemSize.Count) return;
            float size;
            switch (direction)
            {
                case Direction.Vertical:
                    size = binder.rect.height;
                    break;
                case Direction.Horizontal:
                    size = binder.rect.width;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.LogError($"{binder.ItemIndex} => {size}");
            if (_itemSize[binder.ItemIndex].Equals(size)) return;
            Debug.LogError($"{binder.ItemIndex} => {size}");
            _itemSize[binder.ItemIndex] = size;
            UpdatePosition();
        }

        protected override void Awake()
        {
            base.Awake();
            if (!Application.isPlaying) return;
            onValueChanged.AddListener(OnScrollMove);

            UpdatePosition();
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
                        _currentPosition = -1 * pos.x;
                    else _currentPosition = pos.x;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdatePosition();
        }

        private void UpdateContents()
        {
            var fullSize = _itemSize.Sum() + itemInterval * (_itemSize.Count - 1);
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
            var itemSizeList = _itemSize.ToArray();
            _currentItemIndexPosition = 0f;
            _currentItemIndex = -1;
            do
            {
                _currentItemIndex++;
                var itemSize = itemSizeList.Length > _currentItemIndex
                    ? itemSizeList[_currentItemIndex]
                    : defaultItemSize;
                _currentItemIndexPosition += itemSize + itemInterval;
            } while (_currentItemIndexPosition < _currentPosition);

            _currentBinderIndex = _currentItemIndex % instantiatedItemCount;
            UpdateBinder();
        }

        private void UpdateBinder()
        {
            var data = _itemDataList.ToArray();
            var itemSizeList = _itemSize.ToArray();
            var fullSize = itemSizeList.Sum() + Mathf.Max(itemSizeList.Length - 1, 0f) * itemInterval;

            for (var i = 0; i < InfiniteScrollBinders.Length; i++)
            {
                var index = i;
                var infiniteScrollBinder = InfiniteScrollBinders[index];
                if (_currentBinderIndex > index) index = instantiatedItemCount + index;
                var itemIndex = _currentItemIndex + index - _currentBinderIndex;
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
                        infiniteScrollBinder.UpdateItemPosition(new Vector2(pos, 0));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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