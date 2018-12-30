using System.Collections;
using UnityEngine;

namespace Scrmizu
{
    internal class InfiniteScrollBinder : MonoBehaviour
    {
        private InfiniteScroll _infiniteScroll;

        private RectTransform _rectTransform;
        private IInfiniteScrollItem _infiniteScrollItem;
        private int _itemIndex;

        private RectTransform RectTransform =>
            _rectTransform != null ? _rectTransform : _rectTransform = GetComponent<RectTransform>();

        public Vector2 size = Vector2.zero;


        public int ItemIndex => _itemIndex;

        private IInfiniteScrollItem InfiniteScrollItem =>
            _infiniteScrollItem ?? (_infiniteScrollItem = GetComponent<IInfiniteScrollItem>());

        public void SetInfiniteScroll(InfiniteScroll infiniteScroll)
        {
            _infiniteScroll = infiniteScroll;
        }

        public void Hide()
        {
            _itemIndex = -1;
            InfiniteScrollItem.Hide();
        }

        public void UpdateItemData(object data, int itemIndex)
        {
            _itemIndex = itemIndex;
            InfiniteScrollItem.UpdateItemData(data);
        }

        public void UpdateItemPosition(Vector2 position)
        {
            RectTransform.anchoredPosition = position;
        }

        public void UpdateSize()
        {
            var newSize = new Vector2(RectTransform.rect.width, RectTransform.rect.height);
            if (newSize == Vector2.zero) return;
            if (size == newSize) return;
            size = newSize;
            _infiniteScroll.UpdateItemSize(this);
        }
    }
}