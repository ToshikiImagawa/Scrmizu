// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Scrmizu.Sample.DI;

namespace Scrmizu.Sample.Chat
{
    public class ChatMockService : IChatService
    {
        [InjectField] private IChatRepository _chatRepository;
        public event Action<ChatItemData> ChatEventListener;
        public event Action<ChatItemData[]> UpdateEventListener;
        private readonly SynchronizationContext _unitySynchronizationContext = SynchronizationContext.Current;

        public async Task<bool> SendMessage(string message)
        {
            var item = await _chatRepository.Create(message);
            if (item == null) return false;
            _unitySynchronizationContext.Post(state => { ChatEventListener?.Invoke(item); }, null);
            return true;
        }
    }
}