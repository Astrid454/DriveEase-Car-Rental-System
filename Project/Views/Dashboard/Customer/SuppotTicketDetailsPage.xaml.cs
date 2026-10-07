using CarRental.Backend.Data;
using CarRental.Backend.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;

namespace Project.Views.Dashboard.Customer
{
    public sealed partial class SupportTicketDetailsPage : Page
    {
        private SupportTicket _ticket;

        public SupportTicketDetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not int ticketId)
                return;

            using var db = new AppDbContext();

            _ticket = db.SupportTickets
                .FirstOrDefault(t => t.Id == ticketId);

            if (_ticket == null)
                return;

            LoadTicketDetails();
        }

        private void LoadTicketDetails()
        {
            SubjectText.Text = _ticket.Subject;
            CategoryText.Text = $"Category: {_ticket.Category}";
            StatusText.Text = $"Status: {_ticket.Status}";
            CreatedAtText.Text = $"Created at: {_ticket.CreatedAt:dd MMM yyyy, HH:mm}";

            MessageText.Text = _ticket.Message;

            if (string.IsNullOrWhiteSpace(_ticket.AdminResponse))
            {
                AdminResponseText.Text = "No admin response yet.";
                ResolvedAtText.Text = "";
            }
            else
            {
                AdminResponseText.Text = _ticket.AdminResponse;
                ResolvedAtText.Text = _ticket.ResolvedAt == null
                    ? ""
                    : $"Resolved at: {_ticket.ResolvedAt:dd MMM yyyy, HH:mm}";
            }
        }

        private void Back_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}