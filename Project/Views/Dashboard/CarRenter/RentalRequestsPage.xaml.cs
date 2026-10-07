using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.Views.Dashboard.CarRenter
{
    public sealed partial class RentalRequestsPage : Page
    {
        private List<Reservation> _reservations = new();

        public RentalRequestsPage()
        {
            InitializeComponent();
            LoadRequests();
        }

        private void LoadRequests()
        {
            if (!SessionManager.IsLoggedIn)
                return;

            using var db = new AppDbContext();
            var reservationService = new ReservationService(db);

            int ownerUserId = SessionManager.CurrentUser.UserId;

            _reservations = reservationService.GetOwnerReservations(ownerUserId);

            PendingRequestsText.Text = reservationService
                .CountOwnerReservationsByStatus(ownerUserId, ReservationStatus.Pending)
                .ToString();

            ApprovedRequestsText.Text = reservationService
                .CountOwnerReservationsByStatus(ownerUserId, ReservationStatus.Confirmed)
                .ToString();

            RejectedRequestsText.Text = reservationService
                .CountOwnerReservationsByStatus(ownerUserId, ReservationStatus.Cancelled)
                .ToString();

            DisplayRequests();
        }

        private void DisplayRequests()
        {
            if (RequestsListPanel == null)
                return;

            RequestsListPanel.Children.Clear();

            string selectedFilter = GetSelectedFilter();

            var filteredRequests = _reservations;

            if (selectedFilter != "All requests")
            {
                filteredRequests = _reservations
                    .Where(r => r.Status.ToString() == selectedFilter)
                    .ToList();
            }

            if (filteredRequests.Count == 0)
            {
                RequestsListPanel.Children.Add(new TextBlock
                {
                    Text = "No rental requests found.",
                    Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                    FontSize = 14
                });

                return;
            }

            foreach (var reservation in filteredRequests)
            {
                RequestsListPanel.Children.Add(CreateRequestCard(reservation));
            }
        }

        private string GetSelectedFilter()
        {
            if (RequestsFilterBox.SelectedItem is ComboBoxItem item &&
                item.Content != null)
            {
                return item.Content.ToString();
            }

            return "All requests";
        }

        private Border CreateRequestCard(Reservation reservation)
        {

            string customerName = reservation.Customer == null
                ? "Unknown customer"
                : $"{reservation.Customer.FirstName} {reservation.Customer.LastName}";

            string initials = GetInitials(customerName);

            var statusBackground = GetStatusBackground(reservation.Status);
            var statusForeground = GetStatusForeground(reservation.Status);

            var card = new Border
            {
                Padding = new Thickness(16),
                CornerRadius = new CornerRadius(10),
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                BorderThickness = new Thickness(1)
            };

            var grid = new Grid
            {
                ColumnSpacing = 16
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var avatar = new Border
            {
                Width = 48,
                Height = 48,
                CornerRadius = new CornerRadius(24),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(34, 110, 168, 254)),
                Child = new TextBlock
                {
                    Text = initials,
                    FontSize = 14,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 110, 168, 254)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            Grid.SetColumn(avatar, 0);

            var infoPanel = new StackPanel
            {
                Spacing = 6
            };

            var titleRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8
            };

            titleRow.Children.Add(new TextBlock
            {
                Text = customerName,
                FontSize = 17,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            titleRow.Children.Add(new Border
            {
                Padding = new Thickness(8, 3, 8, 3),
                CornerRadius = new CornerRadius(10),
                Background = statusBackground,
                Child = new TextBlock
                {
                    Text = reservation.Status.ToString(),
                    FontSize = 11,
                    Foreground = statusForeground
                }
            });

            infoPanel.Children.Add(titleRow);

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"{reservation.Car.Brand} {reservation.Car.Model} : {reservation.StartDate:dd MMM yyyy, HH:mm} - {reservation.EndDate:dd MMM yyyy, HH:mm}",
                FontSize = 13,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });
            infoPanel.Children.Add(CreateProgressTimeline(reservation.Status));

            decimal displayedTotal = reservation.TotalCost;

            if (reservation.Status == ReservationStatus.Returned ||
                reservation.Status == ReservationStatus.Completed)
            {
                displayedTotal += reservation.LateFee;
            }

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"Estimated total: € {displayedTotal:0.00}",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            Grid.SetColumn(infoPanel, 1);

            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (reservation.Status == ReservationStatus.Pending)
            {
                var approveButton = new Button
                {
                    Content = "Approve",
                    Tag = reservation.ReservationId
                };
                approveButton.Click += Approve_Click;

                var rejectButton = new Button
                {
                    Content = "Reject",
                    Tag = reservation.ReservationId
                };
                rejectButton.Click += Reject_Click;

                actionsPanel.Children.Add(approveButton);
                actionsPanel.Children.Add(rejectButton);
            }

            var detailsButton = new Button
            {
                Content = "Details",
                Tag = reservation.ReservationId
            };
            detailsButton.Click += Details_Click;

            actionsPanel.Children.Add(detailsButton);

            Grid.SetColumn(actionsPanel, 2);

            grid.Children.Add(avatar);
            grid.Children.Add(infoPanel);
            grid.Children.Add(actionsPanel);

            card.Child = grid;

            return card;
        }

        private StackPanel CreateProgressTimeline(ReservationStatus status)
        {

            if (status == ReservationStatus.Cancelled)
            {
                return new StackPanel();
            }
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Margin = new Thickness(0, 6, 0, 0)
            };

            var steps = new List<ReservationStatus>
            {
                ReservationStatus.Pending,
                ReservationStatus.Confirmed,
                ReservationStatus.PickedUp,
                ReservationStatus.Returned,
                ReservationStatus.Completed
            };

            int currentIndex = steps.IndexOf(status);

            foreach (var step in steps)
            {
                bool isDone = currentIndex >= steps.IndexOf(step);

                panel.Children.Add(new Border
                {
                    Width = 10,
                    Height = 10,
                    CornerRadius = new CornerRadius(5),
                    Background = isDone
                        ? GetStatusForeground(step)
                        : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 71, 85, 105))
                });
            }

            return panel;
        }

        private string GetInitials(string name)
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return "?";

            if (parts.Length == 1)
                return parts[0][0].ToString().ToUpper();

            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        }

        private SolidColorBrush GetStatusBackground(ReservationStatus status)
        {
            return status switch
            {
                ReservationStatus.Pending => new SolidColorBrush(Windows.UI.Color.FromArgb(34, 245, 158, 11)),
                ReservationStatus.Confirmed => new SolidColorBrush(Windows.UI.Color.FromArgb(34, 59, 130, 246)),
                ReservationStatus.PickedUp => new SolidColorBrush(Windows.UI.Color.FromArgb(34, 139, 92, 246)),
                ReservationStatus.Returned => new SolidColorBrush(Windows.UI.Color.FromArgb(34, 6, 182, 212)),
                ReservationStatus.Completed => new SolidColorBrush(Windows.UI.Color.FromArgb(34, 34, 197, 94)),
                ReservationStatus.Cancelled => new SolidColorBrush(Windows.UI.Color.FromArgb(34, 239, 68, 68)),
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(34, 100, 116, 139))
            };
        }

        private SolidColorBrush GetStatusForeground(ReservationStatus status)
        {
            return status switch
            {
                ReservationStatus.Pending => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 158, 11)),
                ReservationStatus.Confirmed => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 59, 130, 246)),
                ReservationStatus.PickedUp => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 139, 92, 246)),
                ReservationStatus.Returned => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 6, 182, 212)),
                ReservationStatus.Completed => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 34, 197, 94)),
                ReservationStatus.Cancelled => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 239, 68, 68)),
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 116, 139))
            };
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            int reservationId = (int)button.Tag;

            Frame.Navigate(typeof(OwnerReservationDetailsPage), reservationId);
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            int reservationId = (int)button.Tag;

            using var db = new AppDbContext();
            var reservationService = new ReservationService(db);

            reservationService.ConfirmReservation(reservationId);

            LoadRequests();
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            int reservationId = (int)button.Tag;

            using var db = new AppDbContext();
            var reservationService = new ReservationService(db);

            reservationService.RejectReservation(reservationId);

            LoadRequests();
        }

        private void RequestsFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayRequests();
        }
    }
}