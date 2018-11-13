using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Humanizer;
using MvvmHelpers;

namespace MonkeyChat
{
    public class Message : ObservableObject
    {
        string text;

        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        string userId;

        public string UserId
        {
            get { return userId; }
            set { SetProperty(ref userId, value); }
        }

        DateTime messageDateTime;

        public DateTime MessageDateTime
        {
            get { return messageDateTime; }
            set { SetProperty(ref messageDateTime, value); }
        }

        public string MessageTimeDisplay => MessageDateTime.Humanize();

        bool isIncoming;

        public bool IsIncoming
        {
            get { return isIncoming; }
            set { SetProperty(ref isIncoming, value); }
        }

        public bool HasAttachement => !string.IsNullOrEmpty(attachementUrl);

        string attachementUrl;

        public string AttachementUrl
        {
            get { return attachementUrl; }
            set { SetProperty(ref attachementUrl, value); }
        }

        private readonly List<string> _colors = new List<string> { "#E0BBE4", "#957DAD", "#D291BC", "#E0BBE4", "#FEC8D8", "#FFDFD3" };

        public string BackgroundColor
        {
            get
            {
                MD5 md5Hasher = MD5.Create();
                var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(userId));
                var value = BitConverter.ToInt32(hashed, 0);
                return _colors[value % _colors.Count];
            }
        }

    }
}

