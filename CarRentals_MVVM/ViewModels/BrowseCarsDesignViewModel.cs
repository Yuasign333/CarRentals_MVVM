// ─────────────────────────────────────────────────────────────────────────────
// Design-time only ViewModel for BrowseCarsWindow.xaml.
// Provides fake data so the Visual Studio designer shows a preview of all
// 3 pages without needing to run the application.
// Change the Page number to switch which page is previewed in the designer.
// This class is NEVER used at runtime — only in the XAML designer.
// Connected to: BrowseCarsWindow.xaml (via d:DataContext)
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    public class BrowseCarsDesignViewModel
    {
        // ── Page switching (designer preview only) ─────────────────────────────

        /// <summary>
        /// Change this number to preview a different page in the designer:
        ///   1 = Car list (Page 1)
        ///   2 = Color picker (Page 2)
        ///   3 = Booking form (Page 3)
        /// Also change d:Visibility on the corresponding page section in the XAML.
        /// </summary>
        public int Page { get; } = 1;

        /// <summary>True when Page 1 (car list) is active.</summary>
        public bool IsPageList => Page == 1;

        /// <summary>True when Page 2 (color picker) is active.</summary>
        public bool IsPageColor => Page == 2;

        /// <summary>True when Page 3 (booking form) is active.</summary>
        public bool IsPageForm => Page == 3;

        // ── Display labels ─────────────────────────────────────────────────────

        /// <summary>
        /// Fake user label shown in the top-right badge in the designer.
        /// </summary>
        public string UserLabel { get; } = "Customer: C001";

        // ── Filter defaults ────────────────────────────────────────────────────

        /// <summary>
        /// Default category filter shown in the designer.
        /// </summary>
        public string SelectedCategory { get; set; } = "All";

        /// <summary>
        /// Default fuel type filter shown in the designer.
        /// </summary>
        public string SelectedFuel { get; set; } = "All";

        // ── Page 2 design data ─────────────────────────────────────────────────

        /// <summary>
        /// Default selected color shown on Page 2 in the designer.
        /// </summary>
        public string SelectedColor { get; set; } = "White";

        // ── Page 3 design data ─────────────────────────────────────────────────

        /// <summary>
        /// Fake driver name shown in the booking form on Page 3 in the designer.
        /// </summary>
        public string DriverName { get; set; } = "Juan Dela Cruz";

        /// <summary>
        /// Fake rental hours shown in the booking form on Page 3 in the designer.
        /// </summary>
        public int Hours { get; set; } = 3;

        /// <summary>
        /// Fake deposit amount shown in the Rental Estimate box on Page 3.
        /// </summary>
        public decimal Deposit { get; } = 50m;

        /// <summary>
        /// Fake base price shown in the Rental Estimate box on Page 3.
        /// </summary>
        public decimal BasePrice { get; } = 120m;

        /// <summary>
        /// Fake total due shown in the Rental Estimate box on Page 3.
        /// </summary>
        public decimal TotalDue { get; } = 170m;

        // ── Selected car (used for Page 2 and 3 previews) ─────────────────────

        /// <summary>
        /// Fake selected car shown on Page 2 (color picker) and Page 3 (booking form).
        /// Provides the car name, image, price, and color options for the designer.
        /// </summary>
        public CarModel SelectedCar { get; } = new CarModel
        {
            CarId = "C001",
            Name = "Toyota Camry",
            Category = "Sedan",
            FuelType = "Standard Engine",
            PricePerHour = 40,
            ImageUrl = "https://i.imgur.com/gMFP5tP.jpeg",
            AvailableColors = ["White", "Gray"]
        };

        // ── Car list (used for Page 1 preview) ────────────────────────────────

        /// <summary>
        /// Fake car list shown as cards on Page 1 of BrowseCarsWindow.
        /// Matches the seed data in CarDataService so the designer
        /// looks the same as the running application.
        /// </summary>
        public ObservableCollection<CarModel> FilteredCars { get; } = new()
        {
            new CarModel
            {
                CarId           = "C001",
                Name            = "Toyota Camry",
                Category        = "Sedan",
                FuelType        = "Standard Engine",
                Status          = "Available",
                PricePerHour    = 40,
                ImageUrl        = "https://i.imgur.com/gMFP5tP.jpeg",
                AvailableColors = [ "White", "Gray" ]
            },
            new CarModel
            {
                CarId           = "C002",
                Name            = "Ford Explorer",
                Category        = "SUV",
                FuelType        = "Standard Engine",
                Status          = "Available",
                PricePerHour    = 75,
                ImageUrl        = "https://i.imgur.com/vgEvOtG.jpeg",
                AvailableColors = [ "White" ]
            },
            new CarModel
            {
                CarId           = "C003",
                Name            = "Honda Civic",
                Category        = "Sedan",
                FuelType        = "Standard Engine",
                Status          = "Available",
                PricePerHour    = 35,
                ImageUrl        = "https://i.imgur.com/adLesJ3.jpeg",
                AvailableColors = [ "White", "Red", "Blue" ]
            },
            new CarModel
            {
                CarId           = "C004",
                Name            = "Tesla Model 3",
                Category        = "Sedan",
                FuelType        = "EV",
                Status          = "Available",
                PricePerHour    = 65,
                ImageUrl        = "https://i.imgur.com/69GQ8YT.jpeg",
                AvailableColors = [ "White" ]
            },
            new CarModel
            {
                CarId           = "C005",
                Name            = "Toyota HiAce",
                Category        = "Van",
                FuelType        = "Standard Engine",
                Status          = "Available",
                PricePerHour    = 80,
                ImageUrl        = "https://i.imgur.com/UN1XHVO.jpeg",
                AvailableColors = [ "White", "Black" ]
            },
            new CarModel
            {
                CarId           = "C006",
                Name            = "Hyundai Tucson",
                Category        = "SUV",
                FuelType        = "Standard Engine",
                Status          = "Available",
                PricePerHour    = 55,
                ImageUrl        = "https://i.imgur.com/iWOAwIV.jpeg",
                AvailableColors = [ "White", "Black", "Blue" ]
            },
        };
    }
}