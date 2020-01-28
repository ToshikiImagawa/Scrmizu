// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using UnityEngine;

namespace Scrmizu.Sample
{
    [RequireComponent(typeof(RectTransform))]
    public class ChatItem : MonoBehaviour, IInfiniteScrollItem
    {
        void IInfiniteScrollItem.UpdateItemData(object data)
        {
            gameObject.SetActive(true);
        }

        void IInfiniteScrollItem.Hide()
        {
            gameObject.SetActive(false);
        }
    }

    public class ChatItemData
    {
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime SendTime { get; set; }
    }
}