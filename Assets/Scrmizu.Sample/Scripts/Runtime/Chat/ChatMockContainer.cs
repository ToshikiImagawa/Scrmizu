// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using Scrmizu.Sample.DI;

namespace Scrmizu.Sample.Chat
{
    public class ChatMockContainer : Container
    {
        protected override void Install()
        {
            Bind<IChatService, ChatMockService>();
            BindAsSingleLazy<IChatRepository, ChatMockRepository>();
        }
    }
}