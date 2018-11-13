using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyChat
{
    public partial class MainChatPage : ContentPage
    {
        MainChatViewModel vm;
        public MainChatPage()
        {
            InitializeComponent();
            Title = "#general";
            BindingContext = vm =  new MainChatViewModel();

            vm.Messages.CollectionChanged += (sender, e) =>
            {
                UpdateScroll();
            };

            vm.LoadPrevMessages();
        }

        private void UpdateScroll()
        {
            // TODO: Hack to make we scroll to the most recent message
            Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(1000);
                var target = vm.Messages[vm.Messages.Count - 1];
                MessagesListView.ScrollTo(target, ScrollToPosition.End, true);
            });            
        }

        void MyListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            MessagesListView.SelectedItem = null;
        }

        void MyListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            MessagesListView.SelectedItem = null;
        }
    }
}
