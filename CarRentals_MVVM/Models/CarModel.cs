namespace CarRentals_MVVM.Models
{
    public class CarModel
    {
        public string CarId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public string Status { get; set; } = "Available";
        public decimal PricePerHour { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string[] AvailableColors { get; set; } = [];
    }
}