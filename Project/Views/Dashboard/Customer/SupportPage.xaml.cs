using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Pickers;

namespace Project.Views.Dashboard
{
    public sealed partial class SupportPage : Page
    {
        private string _selectedCategory = "Other";
        private string _attachedFileName = "";

        public SupportPage()
        {
            InitializeComponent();
            LoadSupportStats();
            LoadRecentTickets();
        }

        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag == null)
                return;

            string category = button.Tag.ToString();

            Frame.Navigate(typeof(Customer.SupportTicketPage), category);
        }

        private void LoadSupportStats()
        {
            if (SessionManager.CurrentUser == null)
            {
                ActiveTicketsText.Text = "0";
                return;
            }

            using var db = new AppDbContext();
            var supportService = new SupportTicketService(db);

            ActiveTicketsText.Text = supportService
                .GetOpenTicketCount(SessionManager.CurrentUser.UserId)
                .ToString();
        }

        private void RecentTicketsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not SupportTicket ticket)
                return;

            Frame.Navigate(typeof(Customer.SupportTicketDetailsPage), ticket.Id);
        }

        private void LoadRecentTickets()
        {
            if (SessionManager.CurrentUser == null)
            {
                RecentTicketsList.ItemsSource = new List<SupportTicket>();
                return;
            }

            using var db = new AppDbContext();
            var supportService = new SupportTicketService(db);

            RecentTicketsList.ItemsSource =
                supportService.GetRecentUserTickets(SessionManager.CurrentUser.UserId, 5);
        }
    }
}