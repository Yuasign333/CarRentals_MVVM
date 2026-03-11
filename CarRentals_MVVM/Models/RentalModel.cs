using System;
namespace CarRentals_MVVM.Models
{
    public class RentalModel
    {
        public string RentalId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CarId { get; set; } = string.Empty;
        public string CarName { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public int Hours { get; set; } = 1;
        public decimal BasePrice { get; set; }
        public decimal Deposit { get; set; } = 50;
        public decimal TotalCost => BasePrice + Deposit;
        public string Status { get; set; } = "Active";
    }
}