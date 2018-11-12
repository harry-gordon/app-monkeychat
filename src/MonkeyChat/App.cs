using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Skip)]
namespace MonkeyChat
{
    public class App : Application
    {
        public App()
        {
            
            // The root page of your application
            MainPage = new NavigationPage(new RoomsPage()) 
            { 
                
            };
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }

    public static class ViewModelLocator
    {
        static MainChatViewModel chatVM;
        public static MainChatViewModel MainChatViewModel
        {
            get
            {
                if (chatVM == null)
                {
                    chatVM = new MainChatViewModel();
                    chatVM.InitializeMock();
                }
                return chatVM;

            }
        }

    }
}
