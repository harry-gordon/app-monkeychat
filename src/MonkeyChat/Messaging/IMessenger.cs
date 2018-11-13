using System;
using System.Threading.Tasks;

namespace MonkeyChat.Messaging
{
    public interface IMessenger
    {
        Task<bool> Initialize();

        Task<bool> Close();

        void SendMessage(string text);

        Action<Message> MessageAdded { get; set; }
    }
}

