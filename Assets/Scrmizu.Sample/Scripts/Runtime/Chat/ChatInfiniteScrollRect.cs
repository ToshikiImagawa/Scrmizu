// Scrmizu C# reference source
// Copyright (c) 2016-2019 COMCREATE. All rights reserved.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Scrmizu.Sample.Chat;
using UnityEngine;

namespace Scrmizu.Sample.Chat
{
    public class ChatInfiniteScrollRect : InfiniteScrollRect
    {
        private AnimationCurve _curve;
        
        protected override IInfiniteScrollItemRepository InfiniteScrollItemRepository
        {
            get
            {
                return _infiniteScrollItemRepository ?? (_infiniteScrollItemRepository =
                           new SortedInfiniteScrollItemRepository<DateTime, ChatItemData>(data => data.SendTime));
            }
        }

        public void ScrollAnimationEnd()
        {
            
        }

        private void UpdateFlame()
        {
          　 var pos = (new UnityEngine.Vector2(MaxScrollPosition, 0f) * _curve.Evaluate (Time.timeSinceLevelLoad)).x;
            
        }
    }
}