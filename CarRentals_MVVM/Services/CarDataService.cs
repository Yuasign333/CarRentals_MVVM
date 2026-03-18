using System.Collections.Generic;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.Services
{
    public static class CarDataService
    {
        public static List<CarModel> Cars { get; } = new()
        {
            new CarModel { CarId="C001", Name="Toyota Camry",   Category="Sedan", FuelType="Standard Engine", Status="Available", PricePerHour=40,
                ImageUrl="https://i.imgur.com/gMFP5tP.jpeg",
                AvailableColors=["White","Gray"] },
            new CarModel { CarId="C002", Name="Ford Explorer",  Category="SUV",   FuelType="Standard Engine", Status="Available", PricePerHour=75,
                ImageUrl="https://i.imgur.com/vgEvOtG.jpeg",
                AvailableColors=["White"] },
            new CarModel { CarId="C003", Name="Honda Civic",    Category="Sedan", FuelType="Standard Engine", Status="Available", PricePerHour=35,
                ImageUrl="https://i.imgur.com/adLesJ3.jpeg",
                AvailableColors=["White","Red","Blue"] },
            new CarModel { CarId="C004", Name="Tesla Model 3",  Category="Sedan", FuelType="EV",              Status="Available", PricePerHour=65,
                ImageUrl="https://i.imgur.com/69GQ8YT.jpeg",
                AvailableColors=["White"] },
            new CarModel { CarId="C005", Name="Toyota HiAce",   Category="Van",   FuelType="Standard Engine", Status="Available", PricePerHour=80,
                ImageUrl="https://i.imgur.com/UN1XHVO.jpeg",
                AvailableColors=["White","Black"] },
            new CarModel { CarId="C006", Name="Hyundai Tucson", Category="SUV",   FuelType="Standard Engine", Status="Available", PricePerHour=55,
                ImageUrl="https://i.imgur.com/iWOAwIV.jpeg",
                AvailableColors=["White","Black","Blue"] },
        };

        public static List<RentalModel> Rentals { get; } = new();

        public static List<CarModel> GetAll()
        {
            return new List<CarModel>(Cars);
        }

        public static List<CarModel> GetAvailable()
        {
            var result = new List<CarModel>();
            foreach (var car in Cars)
            {
                if (car.Status == "Available")
                    result.Add(car);
            }
            return result;
        }

        public static List<RentalModel> GetByCustomer(string id)
        {
            var result = new List<RentalModel>();
            foreach (var rental in Rentals)
            {
                if (rental.CustomerId == id)
                    result.Add(rental);
            }
            return result;
        }

        public static CarModel? GetById(string carId)
        {
            foreach (var car in Cars)
            {
                if (car.CarId == carId)
                    return car;
            }
            return null;
        }

        public static void AddCar(CarModel car)
        {
            Cars.Add(car);
        }

        public static void RemoveCar(string carId)
        {
            var car = GetById(carId);
            if (car != null)
                Cars.Remove(car);
        }

        public static void AddRental(RentalModel rental)
        {
            Rentals.Add(rental);
        }
    }
}