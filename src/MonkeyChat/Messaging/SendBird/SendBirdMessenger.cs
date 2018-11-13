using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.DeviceInfo;
using SendBird;

namespace MonkeyChat.Messaging.SendBird
{
    public class SendBirdMessenger : IMessenger
    {
        private OpenChannel _channel = null;

        public Action<Message> MessageAdded { get; set; }
        public Action<List<Message>> MessagesAdded { get; set; }

        public async Task<bool> Initialize()
        {
            InitSendBird();

            await Connect();
            await CreateAndEnterChannel("general");

            return true;
        }

        public async Task<bool> Close()
        {
            // TODO: Unsubscribe from messages

            await LeaveChannel();
            await Disconnect();

            return true;
        }

        public async Task LoadPrevMessages()
        {
            if (_channel == null) return;

            await FetchChannelMessages(_channel);
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

        private async Task CreateAndEnterChannel(string channelName)
        {
            Console.WriteLine($"SendBird: Attempting to join channel...");

            var channel = await GetOrCreateChannel(channelName);
            await EnterChannel(channel);

            SubscribeToMessages();
        }

        private async Task FetchChannelMessages(OpenChannel channel)
        {
            var tcs = new TaskCompletionSource<SendBirdException>();

            var prevMessageListQuery = channel.CreatePreviousMessageListQuery();
            prevMessageListQuery.Load(30, false, (messages, ex) =>
            {
                HandleMessages(messages);
                tcs.SetResult(ex);
            });

            HandleException(await tcs.Task);
        }

        private void HandleMessage(BaseMessage baseMessage)
        {
            MessageAdded.Invoke(BuildMessage(baseMessage));
        }

        private void HandleMessages(List<BaseMessage> baseMessages)
        {
            MessagesAdded.Invoke(baseMessages.Select(BuildMessage).ToList());
        }

        private Message BuildMessage(BaseMessage baseMessage)
        {
            if (baseMessage is UserMessage userMessage)
            {
                return new Message
                {
                    IsIncoming = userMessage.Sender.UserId != GetUserId(),
                    MessageDateTime = DateTime.FromFileTime(userMessage.CreatedAt),
                    Text = userMessage.Message
                };
            }

            return new Message
            {
                IsIncoming = true,
                MessageDateTime = DateTime.FromFileTime(baseMessage.CreatedAt),
                Text = "[unhandled message]"
            };
        }

        private async Task<OpenChannel> GetOrCreateChannel(string channelName)
        {
            var channel = await GetChannel(channelName) ?? await CreateChannel(channelName);
            return channel;
        }

        private async Task<OpenChannel> CreateChannel(string channelName)
        {
            var tcs = new TaskCompletionSource<SendBirdException>();
            OpenChannel channel = null;

            OpenChannel.CreateChannel(channelName, null, null, (openChannel, ex) => {
                Console.WriteLine($"SendBird: Created channel \"{openChannel.Name}\"");
                channel = openChannel;
                tcs.SetResult(ex);
            });

            HandleException(await tcs.Task);
            return channel;
        }

        private async Task<OpenChannel> GetChannel(string channelName)
        {
            var channels = await GetChannels(channelName);
            return channels.FirstOrDefault(c => c.Name == channelName);
        }

        private async Task<List<OpenChannel>> GetChannels(string channelName = null)
        {
            var tcs = new TaskCompletionSource<SendBirdException>();

            var result = new List<OpenChannel>();
            var channelListQuery = OpenChannel.CreateOpenChannelListQuery();
            channelListQuery.NameKeyword = channelName;
            channelListQuery.Next((channels, ex) =>
            {
                result = channels;
                tcs.SetResult(ex);
            });

            HandleException(await tcs.Task);
            return result;
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
                HandleMessage(baseMessage);
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
