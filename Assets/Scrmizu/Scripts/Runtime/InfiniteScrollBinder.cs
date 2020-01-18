// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using UnityEngine;

namespace Scrmizu
{
    public sealed class InfiniteScrollBinder : InfiniteScrollBinderBase
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
}