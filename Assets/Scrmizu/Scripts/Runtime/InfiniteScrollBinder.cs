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

        public Rect rect = Rect.zero;

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
            if (gameObject.activeSelf) StartCoroutine(LateUpdateSize());
        }

        public void UpdateItemPosition(Vector2 position)
        {
            RectTransform.anchoredPosition = position;
            UpdateSize();
        }

        private void UpdateSize()
        {
            if (rect == RectTransform.rect) return;
            if (RectTransform.rect == Rect.zero) return;
            rect = RectTransform.rect;
            _infiniteScroll.UpdateItemSize(this);
        }

        private IEnumerator LateUpdateSize()
        {
            yield return null;
            yield return null;
            UpdateSize();
        }
    }
}