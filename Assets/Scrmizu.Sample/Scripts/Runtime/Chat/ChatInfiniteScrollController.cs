// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Scrmizu.Sample.DI;
using UnityEngine;

namespace Scrmizu.Sample.Chat
{
    public class ChatInfiniteScrollController : MonoBehaviour
    {
        [InjectField] private IChatService _chatService;

        [SerializeField] private Container container;
        private ChatInfiniteScrollRect _chatInfiniteScrollRect;
        private SynchronizationContext _unitySynchronizationContext;

        private ChatInfiniteScrollRect ChatInfiniteScrollRect => _chatInfiniteScrollRect != null
            ? _chatInfiniteScrollRect
            : _chatInfiniteScrollRect = GetComponent<ChatInfiniteScrollRect>();


        public void AddChat(string message)
        {
            Task.Run(async () => { await _chatService.SendMessage(message); });
            _unitySynchronizationContext = SynchronizationContext.Current;
        }

        private void Awake()
        {
            container.Inject(this);
            _chatService.ChatEventListener += ChatEventHandler;
            _chatService.UpdateEventListener += UpdateEventListener;
        }

        private void OnDestroy()
        {
            _chatService.ChatEventListener -= ChatEventHandler;
            _chatService.UpdateEventListener -= UpdateEventListener;
        }

        private async void ChatEventHandler(ChatItemData data)
        {
            var isScrollEnd = ChatInfiniteScrollRect.MaxScrollPosition <= ChatInfiniteScrollRect.CurrentPosition;
            ChatInfiniteScrollRect.AddItemData(data);
            if (!isScrollEnd) return;
            await Task.Delay(100);
            _unitySynchronizationContext.Post(
                obj => { ChatInfiniteScrollRect.MovePosition(ChatInfiniteScrollRect.MaxScrollPosition); }, null);
        }

        private async void UpdateEventListener(ChatItemData[] data)
        {
            ChatInfiniteScrollRect.ClearItemData();
            ChatInfiniteScrollRect.AddRangeItemData(data);
            await Task.Delay(100);
            _unitySynchronizationContext.Post(
                obj => { ChatInfiniteScrollRect.MovePosition(ChatInfiniteScrollRect.MaxScrollPosition); }, null);
        }
    }
}