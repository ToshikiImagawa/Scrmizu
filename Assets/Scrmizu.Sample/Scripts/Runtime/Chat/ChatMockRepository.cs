// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;

namespace Scrmizu.Sample.Chat
{
    public class ChatMockRepository : IChatRepository
    {
        private const string MyUserId = "sukumizu";
        private readonly Dictionary<string, ChatItemData> _cache = new Dictionary<string, ChatItemData>();

        public async Task<ChatItemData> Create(string message)
        {
            var guidValue = Guid.NewGuid();
            var item = new ChatItemData(guidValue.ToString(), MyUserId, message, DateTime.Now);
            _cache[item.ChatId] = item;
            return item;
        }

        public Task<ChatItemData[]> FindAll()
        {
            return new Task<ChatItemData[]>(() => _cache.Values.ToArray());
        }

        public Task<ChatItemData> Find(Func<ChatItemData, bool> predicate)
        {
            return new Task<ChatItemData>(() => _cache.Values.FirstOrDefault(predicate));
        }

        public Task<bool> Delete(string chatId)
        {
            return new Task<bool>(() =>
            {
                if (!_cache.ContainsKey(chatId)) return false;
                _cache.Remove(chatId);
                return true;
            });
        }
    }
}