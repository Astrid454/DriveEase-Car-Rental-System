using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;

namespace Project.Views.Dashboard.Admin
{
    public sealed partial class AdminSupportTicketsPage : Page
    {
        private List<SupportTicket> _tickets = new();

        public AdminSupportTicketsPage()
        {
            InitializeComponent();
            LoadTickets();
        }

        private void LoadTickets()
        {
            using var db = new AppDbContext();
            var supportService = new SupportTicketService(db);

            _tickets = supportService.GetAllTickets();

            OpenTicketsText.Text = _tickets.Count(t => t.Status == "Pending").ToString();
            InProgressTicketsText.Text = _tickets.Count(t => t.Status == "In Progress").ToString();
            ResolvedTicketsText.Text = _tickets.Count(t => t.Status == "Resolved").ToString();

            DisplayTickets();
        }

        private void DisplayTickets()
        {
            TicketsPanel.Children.Clear();

            string selectedFilter = GetSelectedFilter();

            var filteredTickets = _tickets;

            if (selectedFilter != "All")
            {
                filteredTickets = _tickets
                    .Where(t => t.Status == selectedFilter)
                    .ToList();
            }

            if (filteredTickets.Count == 0)
            {
                TicketsPanel.Children.Add(new TextBlock
                {
                    Text = "No support tickets found.",
                    FontSize = 14,
                    Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                });

                return;
            }

            foreach (var ticket in filteredTickets)
            {
                TicketsPanel.Children.Add(CreateTicketCard(ticket));
            }
        }

        private string GetSelectedFilter()
        {
            if (TicketFilterBox.SelectedItem is ComboBoxItem item &&
                item.Content != null)
            {
                return item.Content.ToString();
            }

            return "All";
        }

        private Border CreateTicketCard(SupportTicket ticket)
        {
            var card = new Border
            {
                Padding = new Thickness(18),
                CornerRadius = new CornerRadius(14),
                BorderThickness = new Thickness(1),
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(35, 30, 41, 59))
            };

            var grid = new Grid
            {
                ColumnSpacing = 18
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var infoPanel = new StackPanel { Spacing = 6 };

            infoPanel.Children.Add(new TextBlock
            {
                Text = ticket.Subject,
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"Category: {ticket.Category}",
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"Created: {ticket.CreatedAt:dd MMM yyyy, HH:mm}",
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            infoPanel.Children.Add(CreateStatusBadge(ticket.Status));

            Grid.SetColumn(infoPanel, 0);

            var messagePanel = new StackPanel { Spacing = 6 };

            messagePanel.Children.Add(new TextBlock
            {
                Text = ticket.Message,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 620
            });

            Grid.SetColumn(messagePanel, 1);

            var actionsPanel = new StackPanel
            {
                Spacing = 10,
                VerticalAlignment = VerticalAlignment.Center
            };

            var detailsButton = new Button
            {
                Content = "Details",
                Tag = ticket.Id,
                VerticalAlignment = VerticalAlignment.Center
            };

            detailsButton.Click += TicketDetails_Click;

            actionsPanel.Children.Add(detailsButton);

            Grid.SetColumn(actionsPanel, 2);


            grid.Children.Add(infoPanel);
            grid.Children.Add(messagePanel);
            grid.Children.Add(actionsPanel);

            card.Child = grid;

            return card;
        }

        private void TicketDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            int ticketId = (int)button.Tag;

            Frame.Navigate(typeof(AdminSupportTicketDetailsPage), ticketId);
        }


        private Border CreateStatusBadge(string status)
        {
            var color = status switch
            {
                "Pending" => Windows.UI.Color.FromArgb(255, 245, 158, 11),
                "In Progress" => Windows.UI.Color.FromArgb(255, 59, 130, 246),
                "Resolved" => Windows.UI.Color.FromArgb(255, 34, 197, 94),
                _ => Windows.UI.Color.FromArgb(255, 148, 163, 184)
            };

            return new Border
            {
                Padding = new Thickness(8, 3, 8, 3),
                CornerRadius = new CornerRadius(10),
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(35, color.R, color.G, color.B)),
                Child = new TextBlock
                {
                    Text = status,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(color)
                }
            };
        }

        private void TicketFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TicketsPanel == null)
                return;

            DisplayTickets();
        }
    }
}