// Scrmizu C# reference source
// Copyright (c) 2016-2019 COMCREATE. All rights reserved.

using System;
using System.Collections.Generic;

namespace Scrmizu.Sample
{
    public class ChatInfiniteScrollRect : InfiniteScrollRect
    {
        protected override IInfiniteScrollItemRepository InfiniteScrollItemRepository
        {
            get
            {
                return _infiniteScrollItemRepository ?? (_infiniteScrollItemRepository =
                           new SortedInfiniteScrollItemRepository<DateTime, ChatItemData>(data => data.SendTime));
            }
        }

        public void AddChatItemData(ChatItemData data)
        {
            AddItemData(data);
        }

        public void AddRangeChatItemData(IEnumerable<ChatItemData> data)
        {
            AddRangeItemData(data);
        }

        [UnityEngine.ContextMenu("ChatInit")]
        public void ChatInit()
        {
            var data = new List<ChatItemData>();
            var message = string.Empty;
            for (var i = 0; i < 10; i++)
            {
                message += $"Test : {i}";
                var span = TimeSpan.FromSeconds(UnityEngine.Random.Range(-365 * 24 * 60 * 60, 365 * 24 * 60 * 60));
                data.Add(new ChatItemData(i % 2 == 1 ? "sukumizu" : "sukumizu_red", message,
                    DateTime.Now + span));
                message += "\r\n";
            }

            ClearItemData();
            AddRangeChatItemData(data);
            MovePositionAt(9);
        }

        public void AddChat(string message)
        {
            
            AddChatItemData(new ChatItemData("sukumizu", message, DateTime.Now));
        }
    }
}