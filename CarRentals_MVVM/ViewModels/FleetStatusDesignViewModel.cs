// ─────────────────────────────────────────────────────────────────────────────
// Design-time only ViewModel for FleetStatusWindow.xaml.
// Provides fake car data so the Visual Studio designer shows a preview
// of the fleet table without needing to run the application.
// This class is NEVER used at runtime — only in the XAML designer.
// Connected to: FleetStatusWindow.xaml (via d:DataContext)
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;


namespace CarRentals_MVVM.ViewModels
{
    public class FleetStatusDesignViewModel
    {
        /// <summary>
        /// Fake user label shown in the top-right badge in the designer.
        /// </summary>
        public string UserLabel { get; } = "Agent: A001";

        /// <summary>
        /// Fake car data shown in the fleet table in the designer.
        /// Includes all three status types so badge colors can be previewed.
        /// </summary>
        public ObservableCollection<CarModel> Cars { get; } = new()
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
                Status          = "Rented",
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
                Status          = "Maintenance",
                PricePerHour    = 55,
                ImageUrl        = "https://i.imgur.com/iWOAwIV.jpeg",
                AvailableColors = [ "White", "Black", "Blue" ]
            },
        };
    }
}