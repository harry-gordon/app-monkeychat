using System;
using System.Threading.Tasks;

namespace MonkeyChat.Messaging
{
    public interface IMessenger
    {
        Task<bool> InitializeAsync();

        void SendMessage(string text);

        Action<Message> MessageAdded { get; set; }
    }
}

