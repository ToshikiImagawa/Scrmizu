// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using UnityEngine;

namespace Scrmizu.Sample
{
    public class SimpleInfiniteScrollItem : MonoBehaviour, IInfiniteScrollItem
    {
        /// <summary>
        /// ScrollItem enters display area and updates display item data.
        /// </summary>
        /// <param name="data"></param>
        public void UpdateItemData(object data)
        {
            if (!(data is float width)) return;
            gameObject.SetActive(true);
            if (!(gameObject.transform is RectTransform rectTransform)) return;
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
        }

        /// <summary>
        /// Hide ScrollItem because it has left the display area.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}