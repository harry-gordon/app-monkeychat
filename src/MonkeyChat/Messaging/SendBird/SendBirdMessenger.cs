using System;
using System.Threading.Tasks;
using Plugin.DeviceInfo;
using SendBird;

namespace MonkeyChat.Messaging.SendBird
{
    public class SendBirdMessenger : IMessenger
    {
        private OpenChannel _channel = null;

        public Action<Message> MessageAdded { get; set; }

        public async Task<bool> Initialize()
        {
            InitSendBird();

            await Connect();
            await CreateAndEnterChannel("general");

            return true;
        }

        public async Task<bool> Close()
        {
            await LeaveChannel();
            await Disconnect();

            return true;
        }

        private void InitSendBird()
        {
            SendBirdClient.Init("4BF3A2F0-9767-446A-B771-79CF34C611E5");
        }

        private async Task Connect()
        {
            Console.WriteLine($"SendBird: Connecting...");
            var tcs = new TaskCompletionSource<SendBirdException>();

            var userId = GetUserId();
            SendBirdClient.Connect(userId, (user, ex) =>
            {
                Console.WriteLine($"SendBird: Connected with ID {userId}");
                tcs.SetResult(ex);
            });

            HandleException(await tcs.Task);
        }

        private async Task Disconnect()
        {
            var tcs = new TaskCompletionSource<bool>();

            SendBirdClient.Disconnect(() =>
            {
                tcs.SetResult(true);
            });

            await tcs.Task;
        }

        private async Task CreateAndEnterChannel(string channelId)
        {
            Console.WriteLine($"SendBird: Attempting to join channel...");

            var channel = await GetOrCreateChannel(channelId);
            await EnterChannel(channel);

            SubscribeToMessages();
        }

        private async Task<OpenChannel> GetOrCreateChannel(string channelId)
        {
            var tcs = new TaskCompletionSource<SendBirdException>();
            OpenChannel channel = null;

            OpenChannel.CreateChannel(channelId, null, null, (openChannel, ex) => {
                Console.WriteLine($"SendBird: Created channel \"{openChannel.Name}\"");
                channel = openChannel;
                tcs.SetResult(ex);
            });

            HandleException(await tcs.Task);
            return channel;
        }

        private async Task EnterChannel(OpenChannel channel)
        {
            var tcs = new TaskCompletionSource<SendBirdException>();

            await LeaveChannel();

            channel.Enter(ex => {
                Console.WriteLine($"SendBird: Entered channel \"{channel.Name}\"");
                tcs.SetResult(ex);
            });

            HandleException(await tcs.Task);

            _channel = channel;
        }

        private async Task LeaveChannel()
        {
            if (_channel == null) return;

            var tcs = new TaskCompletionSource<SendBirdException>();
            _channel.Exit(tcs.SetResult);
            await tcs.Task;

            _channel = null;
        }

        private void SubscribeToMessages()
        {
            var id = Guid.NewGuid().ToString();
            var handler = new SendBirdClient.ChannelHandler();
            handler.OnMessageReceived = (baseChannel, baseMessage) =>
            {
                if (baseMessage is UserMessage userMessage)
                {
                    MessageAdded.Invoke(new Message
                    {
                        IsIncoming = userMessage.Sender.UserId != GetUserId(),
                        MessageDateTime = DateTime.FromFileTime(userMessage.CreatedAt),
                        Text = userMessage.Message
                    });
                }
            };
            SendBirdClient.AddChannelHandler(id, handler);
        }

        public void SendMessage(string text)
        {
            _channel.SendUserMessage(text, string.Empty, (userMessage, e) =>
            {
                HandleException(e);
            });
        }

        private static void HandleException(SendBirdException ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"SendBird: Exception: {ex.Message}");
                throw ex;
            }
        }

        private string GetUserId()
        {
            return CrossDeviceInfo.Current.Id;
        }
    }
}
