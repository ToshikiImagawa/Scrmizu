// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Threading.Tasks;

namespace Scrmizu.Sample.Chat
{
    public interface IChatRepository
    {
        Task<ChatItemData> Create(string message);
        Task<ChatItemData[]> FindAll();
        Task<ChatItemData> Find(Func<ChatItemData, bool> predicate);
        Task<bool> Delete(string chatId);
    }
}