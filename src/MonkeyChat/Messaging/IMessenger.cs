using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyChat.Messaging
{
    public interface IMessenger
    {
        Task<bool> Initialize();

        Task<bool> Close();

        Task LoadPrevMessages();

        void SendMessage(string text);

        Action<Message> MessageAdded { get; set; }

        Action<List<Message>> MessagesAdded { get; set; }
    }
}

