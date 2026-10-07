using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Project.Views.Dashboard
{
    public sealed partial class FavoritesPage : Page
    {
        public FavoritesPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            if (SessionManager.CurrentUser == null)
                return;

            using var db = new AppDbContext();
            var favoriteService = new FavoriteService(db);

            FavoritesGridView.ItemsSource =
                favoriteService.GetFavoriteCars(SessionManager.CurrentUser.UserId);
        }

        private void FavoritesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Car car)
            {
                Frame.Navigate(typeof(Project.Views.Dashboard.Customer.CarDetailsPage), car.CarId);
            }
        }
    }
}