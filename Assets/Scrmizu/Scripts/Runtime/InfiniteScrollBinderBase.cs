// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using UnityEngine;

namespace Scrmizu
{
    public abstract class InfiniteScrollBinderBase : MonoBehaviour
    {
        [SerializeField] private Vector2 currentSize;

        /// <summary>
        /// Get item size.
        /// </summary>
        public abstract Vector2 Size { get; }

        /// <summary>
        /// Get item index.
        /// </summary>
        internal int ItemIndex { get; private set; }

        /// <summary>
        /// Get InfiniteScrollItem.
        /// </summary>
        protected IInfiniteScrollItem InfiniteScrollItem { get; set; }

        /// <summary>
        /// Get parent InfiniteScrollRect.
        /// </summary>
        protected InfiniteScrollRect ParentInfiniteScrollRect { get; private set; }

        /// <summary>
        /// Hide
        /// </summary>
        internal void Hide()
        {
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
            currentSize = Vector2.zero;
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

        private void Awake()
        {
            InfiniteScrollItem = GetComponent<IInfiniteScrollItem>();
            if (InfiniteScrollItem == null)
                throw new Exception(
                    "Component implementing IInfiniteScrollItem must be attached to GameObject.");
        }
    }
}