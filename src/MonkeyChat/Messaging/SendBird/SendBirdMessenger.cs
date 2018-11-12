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

            Connect();
            CreateAndEnterChannel("general");

            return await Task.FromResult(true);
        }

        private void Init()
        {
            SendBirdClient.Init("4BF3A2F0-9767-446A-B771-79CF34C611E5");

            // api token? c62b7c19c7d8bc4ee5b3bf68050e5b789fc52c1c
        }

        private async void Connect()
        {
            var tcs = new TaskCompletionSource<SendBirdException>();

            var userId = CrossDeviceInfo.Current.Id;
            SendBirdClient.Connect(userId, (user, ex) =>
            {
                tcs.SetResult(ex);
            });

            HandleException(await tcs.Task);
        }

        private async void CreateAndEnterChannel(string channel)
        {
            await CreateChannel(channel);
            await EnterChannel();
        }

        private async Task CreateChannel(string channel)
        {
            var tcs = new TaskCompletionSource<SendBirdException>();

            OpenChannel.CreateChannel(channel, null, null, (openChannel, ex) => {
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
                tcs.SetResult(ex);
            });

            HandleException(await tcs.Task);
        }

        public void SendMessage(string text)
        {
            _channel.SendUserMessage(text, null, (userMessage, e) =>
            {
                // TODO: Handle this better
                HandleException(e);
            });
        }

        private void HandleException(SendBirdException ex)
        {
            System.Diagnostics.Debug.WriteLine($"SendBird Exception: {ex.Message}");
        }

        public Action<Message> MessageAdded { get; set; }
    }
}
