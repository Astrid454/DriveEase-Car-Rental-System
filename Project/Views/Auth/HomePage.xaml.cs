using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Project.Views.Auth
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        private void GoToSignIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.Navigate(typeof(SignInPage));
        }

        private void GoToSignUp_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.Navigate(typeof(SignUpPage));
        }
    }
}