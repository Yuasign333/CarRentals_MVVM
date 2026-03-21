using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    public class AddCarDesignViewModel
    {
        public string UserLabel { get; } = "Agent: A001";
        public string NewCarId { get; set; } = string.Empty;
        public string NewName { get; set; } = string.Empty;
        public string NewCategory { get; set; } = string.Empty;
        public string NewFuelType { get; set; } = string.Empty;
        public decimal NewPricePerHour { get; set; } = 0;
        public string NewStatus { get; set; } = string.Empty;
        public string NewImageUrl { get; set; } = string.Empty;

        public ObservableCollection<CarModel> CarList { get; } = new()
        {
            new CarModel { CarId="C001", Name="Toyota Camry",   Category="Sedan",
                FuelType="Standard Engine", Status="Available", PricePerHour=40,
                ImageUrl="https://i.imgur.com/gMFP5tP.jpeg" },
            new CarModel { CarId="C002", Name="Ford Explorer",  Category="SUV",
                FuelType="Standard Engine", Status="Available", PricePerHour=75,
                ImageUrl="https://i.imgur.com/vgEvOtG.jpeg" },
            new CarModel { CarId="C003", Name="Honda Civic",    Category="Sedan",
                FuelType="Standard Engine", Status="Available", PricePerHour=35,
                ImageUrl="https://i.imgur.com/adLesJ3.jpeg" },
            new CarModel { CarId="C004", Name="Tesla Model 3",  Category="Sedan",
                FuelType="EV", Status="Available", PricePerHour=65,
                ImageUrl="https://i.imgur.com/69GQ8YT.jpeg" },
            new CarModel { CarId="C005", Name="Toyota HiAce",   Category="Van",
                FuelType="Standard Engine", Status="Available", PricePerHour=80,
                ImageUrl="https://i.imgur.com/UN1XHVO.jpeg" },
            new CarModel { CarId="C006", Name="Hyundai Tucson", Category="SUV",
                FuelType="Standard Engine", Status="Rented", PricePerHour=55,
                ImageUrl="https://i.imgur.com/iWOAwIV.jpeg" },
        };
    }
}