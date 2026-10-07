using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Project.Views.Dashboard.CarRenter
{
    public sealed partial class AddCarPage : Page
    {
        private string selectedImagePath = "";
        private int? editingCarId = null;

        private readonly List<string> romanianCounties = new()
        {
            "AB", "AR", "AG", "BC", "BH", "BN", "BR", "BT", "BV", "BZ",
            "CL", "CS", "CJ", "CT", "CV", "DB", "DJ", "GL", "GR", "GJ",
            "HR", "HD", "IL", "IS", "IF", "MM", "MH", "MS", "NT", "OT",
            "PH", "SM", "SJ", "SB", "SV", "TR", "TM", "TL", "VS", "VL",
            "VN", "B"
        };

        public AddCarPage()
        {
            InitializeComponent();
            LoadCounties();
            LoadGarages();

            CountyBox.SelectionChanged += PlateInput_Changed;
            PlateDigitsBox.TextChanged += PlateInput_Changed;
            PlateLettersBox.TextChanged += PlateInput_Changed;
        }

        public void InitializeEditMode(int carId)
        {
            editingCarId = carId;
            LoadCarData(carId);

            PageTitleText.Text = "Edit Car";
            PageSubtitleText.Text = "Update your vehicle details.";
            SaveButton.Content = "Save Changes";
        }

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int carId)
            {
                InitializeEditMode(carId);
            }
        }

        private void LoadCounties()
        {
            CountyBox.Items.Clear();

            foreach (var county in romanianCounties)
                CountyBox.Items.Add(county);
        }

        private void LoadGarages()
        {
            using var db = new AppDbContext();
            var garageService = new GarageService(db);

            var garages = garageService.GetAllGarages();

            GarageBox.Items.Clear();

            foreach (var garage in garages)
            {
                GarageBox.Items.Add(new ComboBoxItem
                {
                    Content = $"{garage.Name} - {garage.City} ({garageService.CountAvailableSpots(garage.GarageId)} spots left)",
                    Tag = garage.GarageId
                });
            }
        }

        private void LoadCarData(int carId)
        {
            using var db = new AppDbContext();
            var carService = new CarService(db);

            var car = carService.GetCarById(carId);

            if (car == null)
                return;

            BrandBox.Text = car.Brand;
            ModelBox.Text = car.Model;
            YearBox.Text = car.Year.ToString();
            PriceBox.Text = car.PricePerDay.ToString();
            selectedImagePath = car.ImagePath;
            ImageText.Text = string.IsNullOrWhiteSpace(car.ImagePath)
                ? "No image selected"
                : System.IO.Path.GetFileName(car.ImagePath);

            UpdateImagePreview();

            ParsePlate(car.PlateNumber);

            foreach (ComboBoxItem item in GarageBox.Items)
            {
                if ((int)item.Tag == car.GarageId)
                {
                    GarageBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void ParsePlate(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return;

            var parts = plate.Split('-');

            if (parts.Length != 3)
                return;

            CountyBox.SelectedItem = parts[0];
            PlateDigitsBox.Text = parts[1];
            PlateLettersBox.Text = parts[2];
        }

        private void PlateInput_Changed(object sender, object e)
        {
            PlatePreviewText.Text = $"Plate preview: {BuildPlateNumber()}";
        }

        private string BuildPlateNumber()
        {
            if (CountyBox.SelectedItem == null)
                return "-";

            string county = CountyBox.SelectedItem.ToString();
            string digits = PlateDigitsBox.Text.Trim();
            string letters = PlateLettersBox.Text.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(digits) || string.IsNullOrWhiteSpace(letters))
                return "-";

            return $"{county}-{digits}-{letters}";
        }

        private async void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();

            var hwnd = WindowNative.GetWindowHandle(MainWindow.Current);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                selectedImagePath = file.Path;
                ImageText.Text = Path.GetFileName(file.Path);
                UpdateImagePreview();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void SaveCar_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            string brand = BrandBox.Text.Trim();
            string model = ModelBox.Text.Trim();
            string plateNumber = BuildPlateNumber();

            if (string.IsNullOrWhiteSpace(brand))
            {
                ErrorText.Text = "Brand is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                ErrorText.Text = "Model is required.";
                return;
            }

            if (!int.TryParse(YearBox.Text.Trim(), out int year))
            {
                ErrorText.Text = "Year must be a valid number.";
                return;
            }

            int currentYear = DateTime.Now.Year;

            if (year < 1990 || year > currentYear)
            {
                ErrorText.Text = $"Year must be between 1990 and {currentYear}.";
                return;
            }

            if (!IsValidRomanianPlate())
            {
                ErrorText.Text = "Plate number is invalid. Use county + 2 or 3 digits + 3 letters. Example: CJ-10-ABC or B-123-ABC.";
                return;
            }

            if (!decimal.TryParse(PriceBox.Text.Trim(), out decimal price))
            {
                ErrorText.Text = "Price must be a valid number.";
                return;
            }

            if (price <= 0)
            {
                ErrorText.Text = "Price per day must be greater than 0.";
                return;
            }

            if (GarageBox.SelectedItem is not ComboBoxItem selectedGarageItem)
            {
                ErrorText.Text = "Please select a garage.";
                return;
            }

            int garageId = (int)selectedGarageItem.Tag;

            using var db = new AppDbContext();
            var carService = new CarService(db);
            var garageService = new GarageService(db);

            if (editingCarId.HasValue
                ? carService.PlateNumberExistsForOtherCar(plateNumber, editingCarId.Value)
                : carService.PlateNumberExists(plateNumber))
            {
                ErrorText.Text = "A car with this plate number already exists.";
                return;
            }

            bool changingGarage = true;

            if (editingCarId.HasValue)
            {
                var existingCar = carService.GetCarById(editingCarId.Value);
                changingGarage = existingCar.GarageId != garageId;
            }

            if (changingGarage && !garageService.HasAvailableSpot(garageId))
            {
                ErrorText.Text = "Selected garage is full.";
                return;
            }

            Car car;

            if (editingCarId.HasValue)
            {
                car = carService.GetCarById(editingCarId.Value);
            }
            else
            {
                car = new Car
                {
                    Status = CarStatus.Available,
                    UserId = SessionManager.CurrentUser.UserId
                };
            }

            car.Brand = brand;
            car.Model = model;
            car.Year = year;
            car.PlateNumber = plateNumber;
            car.PricePerDay = price;
            car.ImagePath = selectedImagePath;
            car.GarageId = garageId;

            if (editingCarId.HasValue)
                carService.UpdateCar(car);
            else
                carService.AddCar(car);

            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private bool IsValidRomanianPlate()
        {
            if (CountyBox.SelectedItem == null)
                return false;

            string county = CountyBox.SelectedItem.ToString();
            string digits = PlateDigitsBox.Text.Trim();
            string letters = PlateLettersBox.Text.Trim().ToUpper();

            if (!romanianCounties.Contains(county))
                return false;

            if (!Regex.IsMatch(digits, @"^\d{2,3}$"))
                return false;

            if (!Regex.IsMatch(letters, @"^[A-Z]{3}$"))
                return false;

            return true;
        }

        private void UpdateImagePreview()
        {
            if (string.IsNullOrWhiteSpace(selectedImagePath))
            {
                CarImagePreview.Source = null;
                return;
            }

            CarImagePreview.Source = new BitmapImage(new Uri(selectedImagePath));
        }
    }
}