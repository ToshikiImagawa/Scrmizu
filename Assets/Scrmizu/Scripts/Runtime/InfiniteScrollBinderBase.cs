// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using UnityEngine;

namespace Scrmizu
{
    public abstract class InfiniteScrollBinderBase : MonoBehaviour
    {
        [SerializeField] private Vector2 currentSize;
        private Vector2 _beforeSize = Vector2.one * -1f;

        /// <summary>
        /// Get item size.
        /// </summary>
        public abstract Vector2 Size { get; }

        /// <summary>
        /// Get item index.
        /// </summary>
        internal int ItemIndex { get; private set; }

        /// <summary>
        /// Get data.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        internal object Data { get; private set; }

        /// <summary>
        /// Get InfiniteScrollItem.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected IInfiniteScrollItem InfiniteScrollItem =>
            _infiniteScrollItem ??= GetComponent<IInfiniteScrollItem>() ??
                                    throw new InvalidOperationException(
                                        $"Component implementing {nameof(IInfiniteScrollItem)} must be attached to GameObject."
                                    );

        private IInfiniteScrollItem _infiniteScrollItem;

        /// <summary>
        /// Get parent InfiniteScrollRect.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected InfiniteScrollRect ParentInfiniteScrollRect { get; private set; }

        /// <summary>
        /// Hide
        /// </summary>
        internal void Hide()
        {
            _beforeSize = Size;
            ItemIndex = -1;
            InfiniteScrollItem.Hide();
            OnHide();
        }

        /// <summary>
        /// Update item data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="itemIndex"></param>
        internal void UpdateItemData(object data, int itemIndex)
        {
            if (itemIndex == ItemIndex && Data.Equals(data)) return;
            _beforeSize = Size;
            currentSize = Vector2.zero;
            Data = data;
            ItemIndex = itemIndex;
            InfiniteScrollItem.UpdateItemData(data);
            OnUpdateItemData(data);
        }

        /// <summary>
        /// Update item position.
        /// </summary>
        /// <param name="position"></param>
        public abstract void UpdateItemPosition(Vector2 position);

        /// <summary>
        /// Update item size.
        /// </summary>
        internal void UpdateSize()
        {
            if (Size == currentSize) return;
            currentSize = Size;
            if (currentSize == _beforeSize) return;
            ParentInfiniteScrollRect.UpdateItemSize(this);
        }

        protected virtual void OnHide()
        {
        }

        protected virtual void OnUpdateItemData(object data)
        {
        }

        /// <summary>
        /// Set parent InfiniteScrollRect.
        /// </summary>
        /// <param name="parentInfiniteScrollRect"></param>
        internal void SetInfiniteScroll(InfiniteScrollRect parentInfiniteScrollRect)
        {
            ParentInfiniteScrollRect = parentInfiniteScrollRect;
        }
    }
}