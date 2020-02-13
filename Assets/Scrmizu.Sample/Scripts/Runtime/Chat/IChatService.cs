// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Threading.Tasks;

namespace Scrmizu.Sample.Chat
{
    public interface IChatService
    {
        event Action<ChatItemData> ChatEventListener;
        event Action<ChatItemData[]> UpdateEventListener;

        Task<bool> SendMessage(string message);

        Task UpdateAll();
    }
}