// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using UnityEngine;

namespace Scrmizu.Sample
{
    [RequireComponent(typeof(InfiniteScrollRect))]
    public class SimpleInfiniteScrollController : MonoBehaviour
    {
        private InfiniteScrollRect _infiniteScrollRect;

        private InfiniteScrollRect InfiniteScrollRect => _infiniteScrollRect != null
            ? _infiniteScrollRect : _infiniteScrollRect = GetComponent<InfiniteScrollRect>();

        private void Awake()
        {
            InfiniteScrollRect.SetItemData(new object[]
            {
                200f, 
                300f, 
                400f, 
                500f, 
                600f, 
                700f, 
                800f, 
                900f, 
                1000f
            });
        }
    }
}