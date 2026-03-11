using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    public class RentCarDesignViewModel
    {
        public string UserLabel { get; } = "Customer: C001";
        public bool IsStep1 { get; } = true;
        public bool IsStep2 { get; } = false;
        public string SelectedColor { get; set; } = "White";
        public string DriverName { get; set; } = "";
        public int Hours { get; set; } = 1;
        public decimal Deposit { get; } = 50m;
        public decimal BasePrice { get; } = 40m;
        public decimal TotalDue { get; } = 90m;

        public CarModel Car { get; } = new CarModel
        {
            CarId = "C001",
            Name = "Toyota Camry",
            Category = "Sedan",
            FuelType = "Standard Engine",
            Status = "Available",
            PricePerHour = 40,
            ImageUrl = "https://i.imgur.com/Jl5yTES.jpeg",
            AvailableColors = ["White", "Black", "Silver", "Red", "Blue"]
        };
       
      
    }
}