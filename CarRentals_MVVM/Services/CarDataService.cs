using System.Collections.Generic;
using System.IO;
using System.Printing;
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
           
        };



        /// <summary>
        /// The master list of all rental transactions.
        /// Starts empty and is populated at runtime when customers confirm bookings.
        /// Connected to: BrowseCarsViewModel.ConfirmCommand (add),
        /// MyRentalsViewModel (read by customer ID).
        /// </summary>
        public static List<RentalModel> Rentals { get; } = new();

        // ── CHAT ─────────────────────────────────────────────────────────────────────

        public static async Task SaveChatMessage(string senderId, string receiverId, string message)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_SaveChatMessage", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SenderId", senderId);
                cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
                cmd.Parameters.AddWithValue("@Message", message);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("SaveChatMessage failed: " + ex.Message); }
        }

        public static async Task<List<ChatMessage>> GetChatMessages(string userId1, string userId2)
        {
            var list = new List<ChatMessage>();
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetChatMessages", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId1", userId1);
                cmd.Parameters.AddWithValue("@UserId2", userId2);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string senderId = reader["SenderId"].ToString() ?? "";
                    list.Add(new ChatMessage
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        SenderId = senderId,
                        ReceiverId = reader["ReceiverId"].ToString() ?? "",
                        Text = reader["Message"].ToString() ?? "",
                        Time = Convert.ToDateTime(reader["SentAt"]).ToString("HH:mm"),
                        IsFromUser = senderId == userId1
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("GetChatMessages failed: " + ex.Message); }
            return list;
        }

        public static async Task<List<CustomerModel>> GetChatCustomers()
        {
            var list = new List<CustomerModel>();
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetChatCustomers", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new CustomerModel
                    {
                        CustomerId = reader["CustomerID"].ToString() ?? "",
                        FullName = reader["FullName"].ToString() ?? "",
                        Username = reader["Username"].ToString() ?? "",
                        ProfilePicturePath = reader["ProfilePicturePath"].ToString() ?? ""
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("GetChatCustomers failed: " + ex.Message); }
            return list;
        }

        // ── PROFILE UPDATE ────────────────────────────────────────────────────────────

        public static async Task UpdateCustomerProfile(
            string customerId, string newUsername, string profilePicPath)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_UpdateCustomerProfile", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@NewUsername", newUsername);
                cmd.Parameters.AddWithValue("@ProfilePic", profilePicPath);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("UpdateProfile failed: " + ex.Message); }
        }

        public static async Task<bool> UsernameExistsExcept(string username, string excludeId)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Customers WHERE Username=@u AND CustomerID != @id", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@id", excludeId);
                return (int)await cmd.ExecuteScalarAsync() > 0;
            }
            catch { return false; }
        }

        // ── DUPLICATE CHECK ───────────────────────────────────────────────────────────

        public static async Task<bool> ContactExists(string contact)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Customers WHERE ContactNumber=@c", conn);
                cmd.Parameters.AddWithValue("@c", contact);
                return (int)await cmd.ExecuteScalarAsync() > 0;
            }
            catch { return false; }
        }

        public static async Task<bool> LicenseExists(string license)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Customers WHERE LicenseNumber=@l", conn);
                cmd.Parameters.AddWithValue("@l", license);
                return (int)await cmd.ExecuteScalarAsync() > 0;
            }
            catch { return false; }
        }

        public static async Task<bool> PasswordExists(string password)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Customers WHERE Password=@p", conn);
                cmd.Parameters.AddWithValue("@p", password);
                return (int)await cmd.ExecuteScalarAsync() > 0;
            }
            catch { return false; }
        }

        public static async Task<CustomerModel> GetCustomerById(string customerId)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                using var cmd = new SqlCommand(
                    "SELECT FullName, ProfilePicturePath FROM Customers WHERE CustomerID = @id", conn);

                // If CustomerID in your SQL database is an INT, change customerId to int.Parse(customerId)
                cmd.Parameters.AddWithValue("@id", customerId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new CustomerModel
                    {
                        FullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : "Unknown",
                        ProfilePicturePath = reader["ProfilePicturePath"] != DBNull.Value ? reader["ProfilePicturePath"].ToString() : string.Empty
                    };
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<CustomerModel?> GetCustomerByUsername(string username)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT * FROM Customers WHERE Username = @u", conn);
                cmd.Parameters.AddWithValue("@u", username);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new CustomerModel
                    {
                        CustomerId = reader["CustomerID"].ToString() ?? "",
                        FullName = reader["FullName"].ToString() ?? "",
                        Username = reader["Username"].ToString() ?? "",
                        ContactNumber = reader["ContactNumber"].ToString() ?? "",
                        LicenseNumber = reader["LicenseNumber"].ToString() ?? "",
                        // THIS WAS THE MISSING LINE RIGHT HERE:
                        ProfilePicturePath = reader["ProfilePicturePath"] != DBNull.Value ? reader["ProfilePicturePath"].ToString() ?? "" : ""
                    };
                }
            }
            catch (Exception ex) { MessageBox.Show("GetCustomerByUsername failed: " + ex.Message); }
            return null;
        }

        //If driver duplicates
        public static bool IsDriverNameInUse(string driverName)
        {
            string ConnectionString = @"Server=DESKTOP-8P1VJSE;Database=RENTAL_REVS_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;";

            // for pc
            //string connectionString = @"Server=.\MSSQLSERVER01;Database=RENTAL_REVS_DATABASE;User Id=sa;Password=ccl2;TrustServerCertificate=True;";

            // If the name is null or empty, it's technically not "in use" in this context
            if (string.IsNullOrWhiteSpace(driverName)) return false;

            int count = 0;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                // Query checks for the name specifically on ACTIVE rentals
                string query = @"
                    SELECT COUNT(1) 
                    FROM Rentals 
                    WHERE DriverName = @DriverName 
                      AND Status = 'Active'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Always use parameters to prevent SQL injection!
                    cmd.Parameters.AddWithValue("@DriverName", driverName.Trim());

                    try
                    {
                        conn.Open();
                        // ExecuteScalar returns the first column of the first row (the COUNT)
                        count = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                        // Log or handle your database error here
                        System.Windows.MessageBox.Show("Database error: " + ex.Message);
                    }
                }
            }

            // If count is greater than 0, the driver is currently renting a car
            return count > 0;
        }



        /// <summary>
        /// Returns a copy of all cars in the fleet.
        /// Used by FleetStatusWindow and BrowseCarsViewModel to display the full list.
        /// </summary>
        /// 



        public static async Task<List<CarModel>> GetAll()
        {
            var cars = new List<CarModel>(); //

            //for laptop
            string connectionString = @"Server=DESKTOP-8P1VJSE;Database=RENTAL_REVS_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;";

            // for pc
            //string connectionString = @"Server=.\MSSQLSERVER01;Database=RENTAL_REVS_DATABASE;User Id=sa;Password=ccl2;TrustServerCertificate=True;";

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
        public static async Task AddCar(CarModel car)
        {
            //for laptop
            string connectionString = @"Server=DESKTOP-8P1VJSE;Database=RENTAL_REVS_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;";


            // for pc
            //string connectionString = @"Server=.\MSSQLSERVER01;Database=RENTAL_REVS_DATABASE;User Id=sa;Password=ccl2;TrustServerCertificate=True;";

            // 1. ADD AvailableColors and @colors TO THE SQL QUERY
            string query = "INSERT INTO Cars (CarId, Name, Category, FuelType, Status, PricePerHour, ImageUrl, AvailableColors) " +
                           "VALUES (@id, @name, @cat, @fuel, @status, @price, @img, @colors)";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", car.CarId);
                        cmd.Parameters.AddWithValue("@name", car.Name);
                        cmd.Parameters.AddWithValue("@cat", car.Category);
                        cmd.Parameters.AddWithValue("@fuel", car.FuelType);
                        cmd.Parameters.AddWithValue("@status", car.Status);
                        cmd.Parameters.AddWithValue("@price", car.PricePerHour);
                        cmd.Parameters.AddWithValue("@img", car.ImageUrl);

                        // ADD THE MISSING PARAMETER HERE (converts array to comma string)
                        cmd.Parameters.AddWithValue("@colors", string.Join(", ", car.AvailableColors));

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection failed: " + ex.Message);
            }
        }
        public static async Task DeleteCar(string carId)
        {
            //for laptop
            string connectionString = @"Server=DESKTOP-8P1VJSE;Database=RENTAL_REVS_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;";


            // for pc
            //string connectionString = @"Server=.\MSSQLSERVER01;Database=RENTAL_REVS_DATABASE;User Id=sa;Password=ccl2;TrustServerCertificate=True;";

            // Make sure the table name matches your SQL database
            string query = "DELETE FROM Cars WHERE CarId = @id";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        // Map your parameter
                        cmd.Parameters.AddWithValue("@id", carId);

                        // ExecuteNonQuery is for DELETE, INSERT, and UPDATE
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Database connection failed: " + ex.Message);
            }

           
        }

        public static async Task UpdateCar(CarModel car)
        {
            //for laptop
            string connectionString = @"Server=DESKTOP-8P1VJSE;Database=RENTAL_REVS_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;";


            // for pc
            //string connectionString = @"Server=.\MSSQLSERVER01;Database=RENTAL_REVS_DATABASE;User Id=sa;Password=ccl2;TrustServerCertificate=True;";

            // 1. ADD AvailableColors = @colors TO THE SQL QUERY
            string query = @"UPDATE Cars 
                     SET Name = @name, 
                         Category = @cat, 
                         FuelType = @fuel, 
                         PricePerHour = @price, 
                         Status = @status, 
                         ImageUrl = @img,
                         AvailableColors = @colors
                     WHERE CarId = @id";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", car.CarId);
                        cmd.Parameters.AddWithValue("@name", car.Name);
                        cmd.Parameters.AddWithValue("@cat", car.Category);
                        cmd.Parameters.AddWithValue("@fuel", car.FuelType);
                        cmd.Parameters.AddWithValue("@price", car.PricePerHour);
                        cmd.Parameters.AddWithValue("@status", car.Status);
                        cmd.Parameters.AddWithValue("@img", car.ImageUrl);

                        // 2. ADD THE MISSING PARAMETER HERE (converts array to comma string)
                        cmd.Parameters.AddWithValue("@colors", string.Join(", ", car.AvailableColors));

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection failed: " + ex.Message);
            }
        }

        // for laptop

        private static readonly string _conn =
            @"Server=DESKTOP-8P1VJSE;Database=RENTAL_REVS_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;";

        // for pc
        //private static readonly string _conn =  @"Server=.\MSSQLSERVER01;Database=RENTAL_REVS_DATABASE;User Id=sa;Password=ccl2;TrustServerCertificate=True;";



        // ── RENTALS ─────────────────────────────────────────────────────────────

        public static async Task<List<RentalModel>> GetAllRentals()
        {
            var list = new List<RentalModel>();
            string query = "SELECT * FROM Rentals";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new RentalModel
                    {
                        RentalId = reader["RentalId"].ToString() ?? "",
                        CustomerId = reader["CustomerId"].ToString() ?? "",
                        CarId = reader["CarId"].ToString() ?? "",
                        CarName = reader["CarName"].ToString() ?? "",
                        DriverName = reader["DriverName"].ToString() ?? "",
                        Color = reader["Color"].ToString() ?? "",
                        Hours = Convert.ToInt32(reader["Hours"]),
                        BasePrice = Convert.ToDecimal(reader["BasePrice"]),
                        Deposit = Convert.ToDecimal(reader["Deposit"]),
                        TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                        RentalDate = Convert.ToDateTime(reader["RentalDate"]),
                        Status = reader["Status"].ToString() ?? "Active"
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("GetAllRentals failed: " + ex.Message); }
            return list;
        }

        public static async Task<List<RentalModel>> GetRentalsByCustomer(string customerId)
        {
            var list = new List<RentalModel>();
            string query = "SELECT * FROM Rentals WHERE CustomerId = @cid";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cid", customerId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new RentalModel
                    {
                        RentalId = reader["RentalId"].ToString() ?? "",
                        CustomerId = reader["CustomerId"].ToString() ?? "",
                        CarId = reader["CarId"].ToString() ?? "",
                        CarName = reader["CarName"].ToString() ?? "",
                        DriverName = reader["DriverName"].ToString() ?? "",
                        Color = reader["Color"].ToString() ?? "",
                        Hours = Convert.ToInt32(reader["Hours"]),
                        BasePrice = Convert.ToDecimal(reader["BasePrice"]),
                        Deposit = Convert.ToDecimal(reader["Deposit"]),
                        TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                        RentalDate = Convert.ToDateTime(reader["RentalDate"]),
                        Status = reader["Status"].ToString() ?? "Active"
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("GetRentalsByCustomer failed: " + ex.Message); }
            return list;
        }

        public static async Task<string> GetNextRentalId()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Rentals", conn);
                int count = (int)await cmd.ExecuteScalarAsync();
                return $"R{(count + 1):D4}";
            }
            catch { return $"R{DateTime.Now.Ticks:D4}"; }
        }

        public static async Task SaveRental(RentalModel rental)
        {
            string query = @"INSERT INTO Rentals 
        (RentalId, CustomerId, CarId, CarName, DriverName, Color, Hours, 
         BasePrice, Deposit, TotalAmount, RentalDate, Status)
        VALUES 
        (@rid, @cid, @carid, @carname, @driver, @color, @hours,
         @base, @deposit, @total, @date, @status)";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@rid", rental.RentalId);
                cmd.Parameters.AddWithValue("@cid", rental.CustomerId);
                cmd.Parameters.AddWithValue("@carid", rental.CarId);
                cmd.Parameters.AddWithValue("@carname", rental.CarName);
                cmd.Parameters.AddWithValue("@driver", rental.DriverName);
                cmd.Parameters.AddWithValue("@color", rental.Color);
                cmd.Parameters.AddWithValue("@hours", rental.Hours);
                cmd.Parameters.AddWithValue("@base", rental.BasePrice);
                cmd.Parameters.AddWithValue("@deposit", rental.Deposit);
                cmd.Parameters.AddWithValue("@total", rental.TotalAmount);
                cmd.Parameters.AddWithValue("@date", rental.RentalDate);
                cmd.Parameters.AddWithValue("@status", rental.Status);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("SaveRental failed: " + ex.Message); throw; }
        }

        public static async Task UpdateRentalStatus(string rentalId, string newStatus)
        {
            string query = "UPDATE Rentals SET Status = @status WHERE RentalId = @id";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@status", newStatus);
                cmd.Parameters.AddWithValue("@id", rentalId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("UpdateRentalStatus failed: " + ex.Message); }
        }

        // ── MAINTENANCE ──────────────────────────────────────────────────────────

        public static async Task<List<MaintenanceModel>> GetAllMaintenance()
        {
            var list = new List<MaintenanceModel>();
            string query = "SELECT * FROM Maintenance";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new MaintenanceModel
                    {
                        MaintenanceId = reader["MaintenanceId"].ToString() ?? "",
                        CarId = reader["CarId"].ToString() ?? "",
                        TechnicianName = reader["TechnicianName"].ToString() ?? "",
                        Description = reader["Description"].ToString() ?? "",
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        EndDate = reader["EndDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["EndDate"]),
                        Cost = Convert.ToDecimal(reader["Cost"]),
                        Status = reader["Status"].ToString() ?? "In Progress"
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("GetAllMaintenance failed: " + ex.Message); }
            return list;
        }

        public static async Task<string> GetNextMaintenanceId()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Maintenance", conn);
                int count = (int)await cmd.ExecuteScalarAsync();
                return $"M{(count + 1):D4}";
            }
            catch { return $"M{DateTime.Now.Ticks:D4}"; }
        }

        public static async Task SaveMaintenance(MaintenanceModel m)
        {
            string query = @"INSERT INTO Maintenance 
        (MaintenanceId, CarId, TechnicianName, Description, StartDate, Cost, Status)
        VALUES (@id, @carid, @tech, @desc, @start, @cost, @status)";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", m.MaintenanceId);
                cmd.Parameters.AddWithValue("@carid", m.CarId);
                cmd.Parameters.AddWithValue("@tech", m.TechnicianName);
                cmd.Parameters.AddWithValue("@desc", m.Description);
                cmd.Parameters.AddWithValue("@start", m.StartDate);
                cmd.Parameters.AddWithValue("@cost", m.Cost);
                cmd.Parameters.AddWithValue("@status", m.Status);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("SaveMaintenance failed: " + ex.Message); throw; }
        }

        public static async Task CompleteMaintenance(string maintenanceId, decimal cost)
        {
            string query = @"UPDATE Maintenance 
                     SET Status='Completed', EndDate=@end, Cost=@cost 
                     WHERE MaintenanceId=@id";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@end", DateTime.Now);
                cmd.Parameters.AddWithValue("@cost", cost);
                cmd.Parameters.AddWithValue("@id", maintenanceId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("CompleteMaintenance failed: " + ex.Message); }
        }

        // ── FILE PATHS ───────────────────────────────────────────────────────────
        // RentalRevData/
        //   Customers/
        //     C001/
        //       Receipts/
        //         Receipt_R0001.txt
        //   Admin/
        //     ReturnReports/
        //       Return_R0001.txt
        //     MaintenanceLogs/
        //       Maint_M0001.txt
        //     RevenueReports/
        //       Revenue_20260101.txt

        private static readonly string _baseDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RentalRevData");

        private static string CustomerReceiptPath(string customerId, string rentalId)
        {
            string dir = Path.Combine(_baseDir, "Customers", customerId, "Receipts");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"Receipt_{rentalId}.txt");
        }
        private static string ReturnReportPath(string rentalId)
        {
            string dir = Path.Combine(_baseDir, "Admin", "ReturnReports");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"Return_{rentalId}.txt");
        }
        private static string MaintenanceLogPath(string maintenanceId)
        {
            string dir = Path.Combine(_baseDir, "Admin", "MaintenanceLogs");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"Maint_{maintenanceId}.txt");
        }
        private static string RevenueReportPath()
        {
            string dir = Path.Combine(_baseDir, "Admin", "RevenueReports");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir,
                $"Revenue_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        }

        // Updated receipt method — replaces the old one
        public static void GenerateReceipt(RentalModel rental)
        {
            try
            {
                string path = CustomerReceiptPath(rental.CustomerId, rental.RentalId);
                using var w = new StreamWriter(path, append: false);
                w.WriteLine("========================================");
                w.WriteLine("         RENTAL REV. — RECEIPT          ");
                w.WriteLine("========================================");
                w.WriteLine($"Rental ID    : {rental.RentalId}");
                w.WriteLine($"Date         : {rental.RentalDate:yyyy-MM-dd HH:mm:ss}");
                w.WriteLine("----------------------------------------");
                w.WriteLine($"Customer ID  : {rental.CustomerId}");
                w.WriteLine($"Driver Name  : {rental.DriverName}");
                w.WriteLine("----------------------------------------");
                w.WriteLine($"Car ID       : {rental.CarId}");
                w.WriteLine($"Car Name     : {rental.CarName}");
                w.WriteLine($"Color        : {rental.Color}");
                w.WriteLine($"Hours        : {rental.Hours}");
                w.WriteLine("----------------------------------------");
                w.WriteLine($"Base Price   : ${rental.BasePrice:F2}");
                w.WriteLine($"Deposit      : ${rental.Deposit:F2}");
                w.WriteLine($"TOTAL DUE    : ${rental.TotalAmount:F2}");
                w.WriteLine("========================================");
                w.WriteLine("  Thank you for choosing Rental Rev!   ");
                w.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Receipt file error: {ex.Message}",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void GenerateReturnReport(RentalModel rental)
        {
            try
            {
                string path = ReturnReportPath(rental.RentalId);
                using var w = new StreamWriter(path, append: false);
                w.WriteLine("========================================");
                w.WriteLine("      RENTAL REV. — RETURN REPORT       ");
                w.WriteLine("========================================");
                w.WriteLine($"Rental ID    : {rental.RentalId}");
                w.WriteLine($"Return Date  : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                w.WriteLine("----------------------------------------");
                w.WriteLine($"Customer ID  : {rental.CustomerId}");
                w.WriteLine($"Car ID       : {rental.CarId}");
                w.WriteLine($"Car Name     : {rental.CarName}");
                w.WriteLine($"Driver Name  : {rental.DriverName}");
                w.WriteLine($"Hours Rented : {rental.Hours}");
                w.WriteLine("----------------------------------------");
                w.WriteLine($"Total Paid   : ${rental.TotalAmount:F2}");
                w.WriteLine($"Status       : Returned");
                w.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Return report error: {ex.Message}",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void GenerateMaintenanceLog(MaintenanceModel m, string carName)
        {
            try
            {
                string path = MaintenanceLogPath(m.MaintenanceId);
                using var w = new StreamWriter(path, append: false);
                w.WriteLine("========================================");
                w.WriteLine("    RENTAL REV. — MAINTENANCE LOG       ");
                w.WriteLine("========================================");
                w.WriteLine($"Maintenance ID : {m.MaintenanceId}");
                w.WriteLine($"Car ID         : {m.CarId}");
                w.WriteLine($"Car Name       : {carName}");
                w.WriteLine($"Technician     : {m.TechnicianName}");
                w.WriteLine($"Description    : {m.Description}");
                w.WriteLine($"Start Date     : {m.StartDate:yyyy-MM-dd HH:mm:ss}");
                w.WriteLine($"End Date       : {(m.EndDate.HasValue ? m.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "In Progress")}");
                w.WriteLine($"Cost           : ${m.Cost:F2}");
                w.WriteLine($"Status         : {m.Status}");
                w.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Maintenance log error: {ex.Message}",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void GenerateRevenueReport(
            List<RentalModel> rentals, decimal total, decimal avgPerRental)
        {
            try
            {
                string path = RevenueReportPath();
                using var w = new StreamWriter(path, append: false);
                w.WriteLine("========================================");
                w.WriteLine("     RENTAL REV. — REVENUE REPORT      ");
                w.WriteLine("========================================");
                w.WriteLine($"Generated     : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                w.WriteLine($"Total Rentals : {rentals.Count}");
                w.WriteLine($"Total Revenue : ${total:F2}");
                w.WriteLine($"Avg Per Rental: ${avgPerRental:F2}");
                w.WriteLine("----------------------------------------");
                w.WriteLine($"{"Rental ID",-10} {"Car",-18} {"Customer",-10} {"Hours",5} {"Total",10}");
                w.WriteLine(new string('-', 56));
                foreach (var r in rentals)
                    w.WriteLine($"{r.RentalId,-10} {r.CarName,-18} {r.CustomerId,-10} {r.Hours,5} ${r.TotalAmount,9:F2}");
                w.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Revenue report error: {ex.Message}",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ── CUSTOMER SIGNUP ──────────────────────────────────────────────────────

        public static async Task<string> GetNextCustomerId()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Customers", conn);
                int count = (int)await cmd.ExecuteScalarAsync();
                return $"C{(count + 1):D3}";
            }
            catch { return $"C{DateTime.Now.Millisecond:D3}"; }
        }

        public static async Task<bool> UsernameExists(string username)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Customers WHERE Username = @u", conn);
                cmd.Parameters.AddWithValue("@u", username);
                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
            catch { return false; }
        }

        public static async Task RegisterCustomer(CustomerModel c)
        {
            string query = @"EXEC sp_RegisterCustomer 
        @CustomerId, @FullName, @Username, @Password,
        @Contact, @License, @SecurityQ, @SecurityA";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_RegisterCustomer", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerId", c.CustomerId);
                cmd.Parameters.AddWithValue("@FullName", c.FullName);
                cmd.Parameters.AddWithValue("@Username", c.Username);
                cmd.Parameters.AddWithValue("@Password", c.Password);
                cmd.Parameters.AddWithValue("@Contact", c.ContactNumber);
                cmd.Parameters.AddWithValue("@License", c.LicenseNumber);
                cmd.Parameters.AddWithValue("@SecurityQ", c.SecurityQuestion);
                cmd.Parameters.AddWithValue("@SecurityA", c.SecurityAnswer);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Registration failed: " + ex.Message);
                throw;
            }
        }
        // ── FORGOT PASSWORD ──────────────────────────────────────────────────────────

        public static async Task<(bool Found, string Question, string CustomerId)>
            GetSecurityQuestion(string username)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetSecurityQuestion", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return (true,
                        reader["SecurityQuestion"].ToString() ?? "",
                        reader["CustomerID"].ToString() ?? "");
                }
                return (false, "", "");
            }
            catch { return (false, "", ""); }
        }

        public static async Task<(bool Success, string Message)>
            ResetPassword(string username, string securityAnswer, string newPassword)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_ResetPassword", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@SecurityAnswer", securityAnswer);
                cmd.Parameters.AddWithValue("@NewPassword", newPassword);
                await cmd.ExecuteNonQueryAsync();
                return (true, "Password reset successfully.");
            }
            catch (SqlException ex)
            {
                return (false, ex.Message);
            }
        }

        // ── PROCESS RETURN ───────────────────────────────────────────────────────────

        public static async Task<(decimal FinalAmount, string ReturnStatus)>
            ProcessReturn(string rentalId, int actualHours)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_ProcessReturn", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@RentalId", rentalId);
                cmd.Parameters.AddWithValue("@ActualHours", actualHours);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return (
                        Convert.ToDecimal(reader["FinalAmount"]),
                        reader["ReturnStatus"].ToString() ?? "Returned"
                    );
                }
                return (0, "Returned");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ProcessReturn failed: " + ex.Message);
                return (0, "Error");
            }
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
        //public static void AddCar(CarModel car)
        //{
        //    Cars.Add(car);
        //}

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