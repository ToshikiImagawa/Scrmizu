// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scrmizu.Sample.Chat
{
    public class ChatMockRepository : IChatRepository
    {
        private const string ChatCacheKey = "Chat_Cache";
        private const string MyUserId = "sukumizu";
        private Dictionary<string, ChatItemData> _cache = new Dictionary<string, ChatItemData>();
        private readonly SynchronizationContext _unitySynchronizationContext = SynchronizationContext.Current;

        public async Task<ChatItemData> Create(string message)
        {
            return await Task.Run(() =>
            {
                var guidValue = Guid.NewGuid();
                var item = new ChatItemData(guidValue.ToString(), MyUserId, message, DateTime.Now);
                _cache[item.ChatId] = item;
                Save();
                return item;
            });
        }

        public async Task<ChatItemData[]> FindAll()
        {
            Load();
            return await Task.Run(() => _cache.Values.ToArray());
        }

        public async Task<ChatItemData> Find(Func<ChatItemData, bool> predicate)
        {
            Load();
            return await Task.Run(() => _cache.Values.FirstOrDefault(predicate));
        }

        public async Task<bool> Delete(string chatId)
        {
            return await Task.Run(() =>
            {
                if (!_cache.ContainsKey(chatId)) return false;
                _cache.Remove(chatId);
                Save();
                return true;
            });
        }

        private void Save()
        {
            _unitySynchronizationContext.Send(state =>
            {
                var json = JsonUtility.ToJson(new ChatItemDataArray(_cache.Values.ToArray()));
                PlayerPrefs.SetString(ChatCacheKey, json);
                PlayerPrefs.Save();
            }, null);
        }

        private void Load()
        {
            var json = PlayerPrefs.GetString(ChatCacheKey);
            var array = JsonUtility.FromJson<ChatItemDataArray>(json);
            _cache = array != null
                ? array.items.ToDictionary(item => item.ChatId, item => item)
                : new Dictionary<string, ChatItemData>();
        }

        [Serializable]
        private class ChatItemDataArray
        {
            public ChatItemData[] items;

            public ChatItemDataArray()
            {
                items = new ChatItemData[0];
            }

            public ChatItemDataArray(ChatItemData[] items)
            {
                this.items = items;
            }
        }
    }
}