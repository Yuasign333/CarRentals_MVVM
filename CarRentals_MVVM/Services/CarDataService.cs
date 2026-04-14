using System.Collections.Generic;
using System.Windows;
using CarRentals_MVVM.Models;
using Microsoft.Data.SqlClient;

namespace CarRentals_MVVM.Services
{
    /// <summary>
    /// Static in-memory data store for the entire application.
    /// Acts as the single source of truth for all car and rental data.
    /// Since this is a prototype, all data lives in memory and resets on app restart.
    /// Connected to: AddCarViewModel (create/delete cars),
    /// BrowseCarsViewModel (read cars, create rentals),
    /// MyRentalsViewModel (read rentals),
    /// FleetStatusWindow (read all cars).
    /// </summary>
    public static class CarDataService
    {
        /// <summary>
        /// The master list of all cars in the fleet.
        /// Pre-loaded with seed data on app startup.
        /// Modified by AddCarViewModel (add/remove) and BrowseCarsViewModel (update status).
        /// </summary>
        public static List<CarModel> Cars { get; } = new()
        {
            //new CarModel
            //{
            //    CarId           = "C001",
            //    Name            = "Toyota Camry",
            //    Category        = "Sedan",
            //    FuelType        = "Standard Engine",
            //    Status          = "Available",
            //    PricePerHour    = 40,
            //    ImageUrl        = "https://i.imgur.com/gMFP5tP.jpeg",
            //    AvailableColors = [ "White", "Gray" ],



            //},
            //new CarModel
            //{
            //    CarId           = "C002",
            //    Name            = "Ford Explorer",
            //    Category        = "SUV",
            //    FuelType        = "Standard Engine",
            //    Status          = "Available",
            //    PricePerHour    = 75,
            //    ImageUrl        = "https://i.imgur.com/vgEvOtG.jpeg",
            //    AvailableColors = [ "White" ],



            //},
            //new CarModel
            //{
            //    CarId           = "C003",
            //    Name            = "Honda Civic",
            //    Category        = "Sedan",
            //    FuelType        = "Standard Engine",
            //    Status          = "Available",
            //    PricePerHour    = 35,
            //    ImageUrl        = "https://i.imgur.com/adLesJ3.jpeg",
            //    AvailableColors = [ "White", "Red", "Blue" ],



            //},
            //new CarModel
            //{
            //    CarId           = "C004",
            //    Name            = "Tesla Model 3",
            //    Category        = "Sedan",
            //    FuelType        = "EV",
            //    Status          = "Available",
            //    PricePerHour    = 65,
            //    ImageUrl        = "https://i.imgur.com/69GQ8YT.jpeg",
            //    AvailableColors = [ "White" ],



            //},
            //new CarModel
            //{
            //    CarId           = "C005",
            //    Name            = "Toyota HiAce",
            //    Category        = "Van",
            //    FuelType        = "Standard Engine",
            //    Status          = "Available",
            //    PricePerHour    = 80,
            //    ImageUrl        = "https://i.imgur.com/UN1XHVO.jpeg",
            //    AvailableColors = [ "White", "Black" ],



            //},
            //new CarModel
            //{
            //    CarId           = "C006",
            //    Name            = "Hyundai Tucson",
            //    Category        = "SUV",
            //    FuelType        = "Standard Engine",
            //    Status          = "Maintenance",
            //    PricePerHour    = 55,
            //    ImageUrl        = "https://i.imgur.com/iWOAwIV.jpeg",
            //    AvailableColors = [ "White", "Black", "Blue" ],



            //},
        };






        /// <summary>
        /// The master list of all rental transactions.
        /// Starts empty and is populated at runtime when customers confirm bookings.
        /// Connected to: BrowseCarsViewModel.ConfirmCommand (add),
        /// MyRentalsViewModel (read by customer ID).
        /// </summary>
        public static List<RentalModel> Rentals { get; } = new();

        /// <summary>
        /// Returns a copy of all cars in the fleet.
        /// Used by FleetStatusWindow and BrowseCarsViewModel to display the full list.
        /// </summary>
        public static async Task<List<CarModel>> GetAll()
        {
            var cars = new List<CarModel>();

            string connectionString = @"Server=.\MSSQLSERVER01;Database=RENTAL_REVS_DATABASE;User Id=sa;Password=ccl2;TrustServerCertificate=True;";
            string query = "SELECT * FROM Cars";

            try
            {
                // Use 'await' on the connection open and the reader execution
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(); // Non-blocking open

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync()) // Non-blocking execute
                        {
                            // Use ReadAsync to keep the loop asynchronous
                            while (await reader.ReadAsync())
                            {
                                var car = new CarModel
                                {
                                    CarId = reader["CarId"].ToString() ?? "",
                                    Name = reader["Name"].ToString() ?? "",
                                    Category = reader["Category"].ToString() ?? "",
                                    FuelType = reader["FuelType"].ToString() ?? "",
                                    Status = reader["Status"].ToString() ?? "",
                                    PricePerHour = Convert.ToDecimal(reader["PricePerHour"]),
                                    ImageUrl = reader["ImageUrl"].ToString() ?? "",
                                    AvailableColors = (reader["AvailableColors"] == DBNull.Value ? ""
                                        : reader["AvailableColors"].ToString())
                                        ?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                                        ?? Array.Empty<string>()
                                };
                                cars.Add(car);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection failed: " + ex.Message);
            }

            return cars;
        }

        /// <summary>
        /// Returns only cars with Status = "Available".
        /// Reserved for future use — BrowseCarsViewModel filters directly using LINQ.
        /// </summary>

        public static async Task<List<CarModel>> GetAvailable()
        {
            // Await the task to get the actual list
            List<CarModel> allCars = await GetAll();
            List<CarModel> availableCars = new List<CarModel>();

            foreach (var car in allCars)
            {
                if (car.Status == "Available")
                {
                    availableCars.Add(car);
                }
            }
            return availableCars;
        }


        /// <summary>
        /// Returns all rentals that belong to the given customer ID.
        /// Used by MyRentalsViewModel to show only the logged-in customer's rentals.
        /// </summary>
        /// <param name="id">The customer's user ID (e.g. "C001").</param>
        public static List<RentalModel> GetByCustomer(string id)
        {
            var result = new List<RentalModel>();

            foreach (var rental in Rentals)
            {
                if (rental.CustomerId == id)
                {
                    result.Add(rental);
                }
            }

            return result;
        }

        /// <summary>
        /// Finds and returns a single car by its CarId.
        /// Returns null if no match is found.
        /// </summary>
        /// <param name="carId">The car ID to search for (e.g. "C003").</param>
        public static async Task<CarModel?> GetById(string carId)
        {
            List<CarModel> allCars = await GetAll();

            foreach (var car in allCars)
            {
                if (car.CarId == carId)
                {
                    return car;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a new car to the fleet.
        /// Called by AddCarViewModel.SaveCommand after validation passes.
        /// </summary>
        /// <param name="car">The CarModel to add.</param>
        public static void AddCar(CarModel car)
        {
            Cars.Add(car);
        }

        /// <summary>
        /// Removes a car from the fleet by its CarId.
        /// Called by AddCarViewModel.DeleteCommand when the admin confirms deletion.
        /// </summary>
        /// <param name="carId">The ID of the car to remove.</param>
        public static async void RemoveCar(string carId)
        {
            var car = await GetById(carId);

            if (car != null)
            {
                Cars.Remove(car);
            }
        }

        /// <summary>
        /// Adds a completed rental transaction to the rentals list.
        /// Called by BrowseCarsViewModel.ConfirmCommand after booking is confirmed.
        /// </summary>
        /// <param name="rental">The RentalModel to store.</param>
        public static void AddRental(RentalModel rental)
        {
            Rentals.Add(rental);
        }

        
    }
}