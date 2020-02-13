// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scrmizu.Sample.Chat
{
    [RequireComponent(typeof(RectTransform))]
    public class ChatItem : MonoBehaviour, IInfiniteScrollItem
    {
        private Text _sendTime;
        private Text _message;
        private Image _userIcon;

        private Text SendTime => _sendTime != null
            ? _sendTime
            : _sendTime = GetComponentsInChildren<Text>()
                .FirstOrDefault(text => text.name == "SendTime");

        private Text Message => _message != null
            ? _message
            : _message = GetComponentsInChildren<Text>()
                .FirstOrDefault(text => text.name == "Message");

        private Image UserIcon => _userIcon != null
            ? _userIcon
            : _userIcon = GetComponentsInChildren<Image>()
                .FirstOrDefault(text => text.name == "UserIcon");

        void IInfiniteScrollItem.UpdateItemData(object data)
        {
            gameObject.SetActive(true);
            if (!(data is ChatItemData chatItemData)) return;
            SendTime.text = chatItemData.SendTime.ToString("yyyy/MM/dd HH:mm:ss");
            Message.text = chatItemData.Message;
            UserIcon.sprite = Resources.Load<Sprite>($"Textures/{chatItemData.UserId}");
        }

        void IInfiniteScrollItem.Hide()
        {
            gameObject.SetActive(false);
        }
    }

    [Serializable]
    public class ChatItemData
    {
        [field: SerializeField] public string ChatId { get; private set; }
        [field: SerializeField] public string UserId { get; private set; }
        [field: SerializeField] public string Message { get; private set; }

        [SerializeField] private long sendTimeTicks;
        public DateTime SendTime => new DateTime(sendTimeTicks);

        public ChatItemData(string chatId, string userId, string message, DateTime sendTime)
        {
            ChatId = chatId;
            UserId = userId;
            Message = message;
            sendTimeTicks = sendTime.Ticks;
        }
    }
}