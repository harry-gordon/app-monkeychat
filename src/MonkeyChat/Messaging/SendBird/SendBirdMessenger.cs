using System;
using System.Threading.Tasks;
using Plugin.DeviceInfo;
using SendBird;

namespace MonkeyChat.Messaging.SendBird
{
    public class SendBirdMessenger : IMessenger
    {
        private OpenChannel _channel;

        public async Task<bool> InitializeAsync()
        {
            Init();

            await Connect();
            await CreateAndEnterChannel("general");

            return await Task.FromResult(true);
        }

        private void Init()
        {
            SendBirdClient.Init("4BF3A2F0-9767-446A-B771-79CF34C611E5");

            // api token? c62b7c19c7d8bc4ee5b3bf68050e5b789fc52c1c
        }

        private async Task Connect()
        {
            Console.WriteLine($"SendBird: Connecting...");
            var tcs = new TaskCompletionSource<SendBirdException>();

            var userId = CrossDeviceInfo.Current.Id;
            SendBirdClient.Connect(userId, (user, ex) =>
            {
                Console.WriteLine($"SendBird: Connected with ID {userId}");
                tcs.SetResult(ex);
            });

            HandleException(await tcs.Task);
        }

        private async Task CreateAndEnterChannel(string channel)
        {
            Console.WriteLine($"SendBird: Attempting to join channel...");

            await CreateChannel(channel);
            await EnterChannel();
        }

        private async Task CreateChannel(string channel)
        {
            var tcs = new TaskCompletionSource<SendBirdException>();

            OpenChannel.CreateChannel(channel, null, null, (openChannel, ex) => {
                Console.WriteLine($"SendBird: Created channel \"{openChannel.Name}\"");
                _channel = openChannel;
                tcs.SetResult(ex);
            });

            HandleException(await tcs.Task);
        }

        private async Task EnterChannel()
        {
            var tcs = new TaskCompletionSource<SendBirdException>();

            _channel.Enter(ex => {
                // TODO: Exit the channel later
                Console.WriteLine($"SendBird: Entered channel \"{_channel.Name}\"");
                tcs.SetResult(ex);
            });

            HandleException(await tcs.Task);
        }

        public void SendMessage(string text)
        {
            _channel.SendUserMessage(text, string.Empty, (userMessage, e) =>
            {
                // TODO: Handle this better
                HandleException(e);
            });
        }

        private void HandleException(SendBirdException ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"SendBird: Exception: {ex.Message}");
                throw ex;
            }
        }

        public Action<Message> MessageAdded { get; set; }
    }
}
