using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace Project.Views.Dashboard.Admin
{
    public sealed partial class AdminSupportTicketDetailsPage : Page
    {
        private int _ticketId;
        private SupportTicket _ticket;

        public AdminSupportTicketDetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not int ticketId)
                return;

            _ticketId = ticketId;

            LoadTicketDetails();
        }

        private void LoadTicketDetails()
        {
            using var db = new AppDbContext();

            _ticket = db.SupportTickets
                .FirstOrDefault(t => t.Id == _ticketId);

            if (_ticket == null)
                return;

            SubjectText.Text = _ticket.Subject;
            CategoryText.Text = $"Category: {_ticket.Category}";
            StatusText.Text = $"Status: {_ticket.Status}";
            UpdateStatusColor();

            if (_ticket.Status == "Resolved")
            {
                ActionStatusText.Text = "This ticket has already been resolved.";
            }
            else
            {
                ActionStatusText.Text = "";
            }
            CreatedAtText.Text = $"Created at: {_ticket.CreatedAt:dd MMM yyyy, HH:mm}";
            MessageText.Text = _ticket.Message;

            AdminResponseBox.Text = _ticket.AdminResponse ?? "";

            if (_ticket.Status == "Resolved")
            {
                MarkInProgressButton.IsEnabled = false;
                SendResponseButton.IsEnabled = false;
                AdminResponseBox.IsReadOnly = true;
                ActionStatusText.Text = "This ticket has already been resolved.";
            }
            else
            {
                MarkInProgressButton.IsEnabled = true;
                SendResponseButton.IsEnabled = true;
                AdminResponseBox.IsReadOnly = false;
            }
        }

        private void UpdateStatusColor()
        {
            if (_ticket.Status == "Pending")
            {
                StatusText.Foreground = new SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 245, 158, 11));
            }
            else if (_ticket.Status == "In Progress")
            {
                StatusText.Foreground = new SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 96, 165, 250));
            }
            else if (_ticket.Status == "Resolved")
            {
                StatusText.Foreground = new SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 34, 197, 94));
            }
        }

        private void MarkInProgress_Click(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();
            var supportService = new SupportTicketService(db);

            supportService.UpdateTicketStatus(_ticketId, "In Progress");

            ActionStatusText.Foreground = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 96, 165, 250));

            ActionStatusText.Text = "Ticket marked as in progress.";

            LoadTicketDetails();
        }

        private void SendResponse_Click(object sender, RoutedEventArgs e)
        {
            string response = AdminResponseBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(response))
            {
                ActionStatusText.Foreground = new SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113));

                ActionStatusText.Text = "Please write a response before resolving the ticket.";
                return;
            }

            using var db = new AppDbContext();
            var supportService = new SupportTicketService(db);

            supportService.ResolveTicketWithResponse(_ticketId, response);

            ActionStatusText.Foreground = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 52, 211, 153));

            ActionStatusText.Text = "Response sent and ticket resolved.";

            LoadTicketDetails();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}