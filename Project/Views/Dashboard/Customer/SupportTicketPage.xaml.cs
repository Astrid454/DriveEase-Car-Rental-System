using CarRental.Backend.Data;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.Storage.Pickers;

namespace Project.Views.Dashboard.Customer
{
    public sealed partial class SupportTicketPage : Page
    {
        private string _selectedCategory = "Other";
        private string _attachedFileName = "";

        public SupportTicketPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string category && !string.IsNullOrWhiteSpace(category))
            {
                _selectedCategory = category;
            }

            CategoryText.Text = _selectedCategory;
        }

        private async void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Current);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                _attachedFileName = file.Name;
                AttachedFileText.Text = $"Attached: {_attachedFileName}";
            }
        }

        private void SubmitTicket_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "";

            string subject = SubjectTextBox.Text.Trim();
            string message = MessageTextBox.Text.Trim();

            if (SessionManager.CurrentUser == null)
            {
                StatusText.Foreground = new SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113));

                StatusText.Text = "You must be signed in to submit a ticket.";
                return;
            }

            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
            {
                StatusText.Foreground = new SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113));

                StatusText.Text = "Please complete the subject and message.";
                return;
            }

            if (!string.IsNullOrEmpty(_attachedFileName))
            {
                message += $"\n\nAttached file: {_attachedFileName}";
            }

            using var db = new AppDbContext();
            var supportService = new SupportTicketService(db);

            supportService.CreateTicket(
                SessionManager.CurrentUser.UserId,
                subject,
                _selectedCategory,
                message);

            StatusText.Foreground = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 52, 211, 153));

            StatusText.Text = "Ticket submitted successfully.";

            SubjectTextBox.Text = "";
            MessageTextBox.Text = "";
            _attachedFileName = "";
            AttachedFileText.Text = "No file attached";
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}