// ─────────────────────────────────────────────────────────────────────────────
// FILE: CarDataService.cs
// Connected to: All ViewModels that need database access.
// Purpose: The single SQL Server data access layer for the entire application.
//          All CRUD operations for Cars, Rentals, Maintenance, Customers,
//          Chat Messages, and file-based receipts/reports live here.
//          Every method is async (awaitable) so the UI never freezes.
//          Uses parameterized queries throughout to prevent SQL injection.
// Database: RENTAL_REVS_DATABASE (SQL Server)
// Connection: Auto-detects school PC vs laptop on startup (2s timeout).
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CarRentals_MVVM.Models;
using Microsoft.Data.SqlClient;

namespace CarRentals_MVVM.Services
{
    public static class CarDataService
    {
        // ── AUTO CONNECTION STRING ────────────────────────────────────────────
        // Tries the school PC connection first with a 2-second timeout.
        // If the PC server is unreachable, falls back to the laptop string.
        // Result is cached in _cachedConn so the detection only runs once
        // per session — subsequent calls return instantly.

        private static string? _cachedConn = null;

        // Laptop connection — Windows Authentication (Trusted_Connection)
        private static readonly string _laptopConn =
            @"Server=DESKTOP-8P1VJSE;Database=RENTAL_REVS_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;";

        // School PC connection — SQL Server Auth (sa / ccl2)
        private static readonly string _pcConn =
            @"Server=CCL2-12\MSSQLSERVER01;Database=RENTAL_REVS_DATABASE;User Id=sa;Password=ccl2;TrustServerCertificate=True;";

         //  SSMS Database Code (Car Rental Rev)
        //https://pastecode.io/s/9a66yf84


        /// <summary>
        /// Returns the correct connection string for the current machine.
        /// Tries the school PC with Connect Timeout=2 to avoid long hangs.
        /// Falls back to the laptop string if the PC is unreachable.
        /// </summary>
        private static string _conn
        {
            get
            {
                if (_cachedConn != null) return _cachedConn;
                try
                {
                    // Short 2s timeout — don't block the UI if PC is offline
                    using var test = new SqlConnection(_pcConn + "Connect Timeout=2;");
                    test.Open();
                    _cachedConn = _pcConn;
                    return _cachedConn;
                }
                catch { }
                // PC unreachable — use laptop string
                _cachedConn = _laptopConn;
                return _cachedConn;
            }
        }

        // ── LEGACY IN-MEMORY LISTS (kept for backward compatibility) ──────────
        // These are no longer the primary data source — SQL Server is.
        // They are only used by old GetByCustomer() and AddRental() methods
        // that haven't been fully migrated to SQL yet.

        /// <summary>
        /// Legacy in-memory car list. Not used for display — GetAll() hits SQL.
        /// </summary>
        public static List<CarModel> Cars { get; } = new();

        /// <summary>
        /// Legacy in-memory rental list. Not used for display — GetRentalsByCustomer() hits SQL.
        /// </summary>
        public static List<RentalModel> Rentals { get; } = new();

        // ── CHAT ─────────────────────────────────────────────────────────────
        // Calls stored procedures in SQL Server for all chat operations.
        // ChatMessages table stores messages between Customer ↔ Admin.

        /// <summary>
        /// Saves a single chat message to the ChatMessages table via sp_SaveChatMessage.
        /// Called by ChatViewModel.SendCommand after the user taps Send.
        /// </summary>
        /// <param name="senderId">UserID of the sender (e.g. "C001" or "A001").</param>
        /// <param name="receiverId">UserID of the recipient.</param>
        /// <param name="message">The message text to save.</param>
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

        /// <summary>
        /// Loads all chat messages between two users ordered by SentAt ASC.
        /// Called by ChatViewModel.LoadMessages() on window open.
        /// IsFromUser is set based on whether senderId matches userId1.
        /// </summary>
        /// <param name="userId1">The logged-in user's ID (messages from this ID = IsFromUser=true).</param>
        /// <param name="userId2">The other participant's ID.</param>
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
                        // Format the timestamp as HH:mm for display in chat bubble
                        Time = Convert.ToDateTime(reader["SentAt"]).ToString("HH:mm"),
                        // Mark as "from user" if sender matches the logged-in user
                        IsFromUser = senderId == userId1
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("GetChatMessages failed: " + ex.Message); }
            return list;
        }

        /// <summary>
        /// Returns a list of all customers who have an active chat history.
        /// Used by AdminChatListViewModel to populate the admin's inbox.
        /// Calls sp_GetChatCustomers which also returns UnreadCount and LastMessage.
        /// </summary>
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

        // ── PROFILE ───────────────────────────────────────────────────────────

        /// <summary>
        /// Updates the customer's Username and ProfilePicturePath in the Customers table.
        /// Called by MyAccountViewModel when user saves a new username or profile picture.
        /// Uses sp_UpdateCustomerProfile stored procedure.
        /// </summary>
        /// <param name="customerId">The customer's DB ID (e.g. "C001").</param>
        /// <param name="newUsername">The new username to save.</param>
        /// <param name="profilePicPath">Absolute local file path to the selected image.</param>
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

        /// <summary>
        /// Checks if a username already exists in Customers — excluding a specific CustomerID.
        /// Used by MyAccountViewModel.SaveUsernameCommand to allow the current user's own name
        /// but reject names taken by others.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <param name="excludeId">The CustomerID to exclude from the check (the current user).</param>
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
                var result = await cmd.ExecuteScalarAsync();
                return result != null && (int)result > 0;
            }
            catch { return false; }
        }

        // ── DUPLICATE CHECKS (used during Sign Up) ────────────────────────────
        // These run before calling RegisterCustomer to catch constraint violations
        // early and show user-friendly messages instead of raw SQL errors.

        /// <summary>
        /// Returns true if the given contact number already exists in Customers.
        /// Prevents duplicate accounts with the same phone number.
        /// </summary>
        public static async Task<bool> ContactExists(string contact)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Customers WHERE ContactNumber=@c", conn);
                cmd.Parameters.AddWithValue("@c", contact);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && (int)result > 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Returns true if the given license number already exists in Customers.
        /// LicenseNumber has a UNIQUE constraint in SQL — this check prevents
        /// the constraint error from surfacing to the user.
        /// </summary>
        public static async Task<bool> LicenseExists(string license)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Customers WHERE LicenseNumber=@l", conn);
                cmd.Parameters.AddWithValue("@l", license);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && (int)result > 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Returns true if the given password is already in use by another customer.
        /// The Customers table has a CHECK constraint preventing password = username.
        /// </summary>
        public static async Task<bool> PasswordExists(string password)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Customers WHERE Password=@p", conn);
                cmd.Parameters.AddWithValue("@p", password);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && (int)result > 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Fetches a single customer's FullName and ProfilePicturePath by CustomerID.
        /// Used by ChatViewModel to display the customer's name and photo in the admin chat header.
        /// Returns null if the customer is not found or the query fails.
        /// </summary>
        public static async Task<CustomerModel?> GetCustomerById(string customerId)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT FullName, ProfilePicturePath FROM Customers WHERE CustomerID = @id", conn);
                cmd.Parameters.AddWithValue("@id", customerId);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new CustomerModel
                    {
                        FullName = reader["FullName"] != DBNull.Value
                                             ? reader["FullName"].ToString() : "Unknown",
                        ProfilePicturePath = reader["ProfilePicturePath"] != DBNull.Value
                                             ? reader["ProfilePicturePath"].ToString() : string.Empty
                    };
                }
                return null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Fetches the full CustomerModel record by Username.
        /// Used by MyAccountViewModel.LoadUserDataAsync to populate the profile page
        /// and by LoginViewModel to get CustomerID after username-based login.
        /// </summary>
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
                        ProfilePicturePath = reader["ProfilePicturePath"] != DBNull.Value
                                             ? reader["ProfilePicturePath"].ToString() ?? "" : ""
                    };
                }
            }
            catch (Exception ex) { MessageBox.Show("GetCustomerByUsername failed: " + ex.Message); }
            return null;
        }

        /// <summary>
        /// Synchronous check: returns true if the driver name is already used
        /// in an Active rental. Prevents two active rentals with the same driver.
        /// Used by RentCarViewModel.DriverName property setter as a real-time guard.
        /// NOTE: This is intentionally synchronous — it runs inline in the setter.
        /// </summary>
        public static bool IsDriverNameInUse(string driverName)
        {
            // Empty names are never "in use" — let the setter's other validation handle that
            if (string.IsNullOrWhiteSpace(driverName)) return false;

            int count = 0;
            using (SqlConnection conn = new SqlConnection(_conn))
            {
                // Only check ACTIVE rentals — returned rentals don't block new ones
                string query = @"
                    SELECT COUNT(1)
                    FROM Rentals
                    WHERE DriverName = @DriverName
                      AND Status = 'Active'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Parameterized to prevent SQL injection even in a sync context
                    cmd.Parameters.AddWithValue("@DriverName", driverName.Trim());
                    try
                    {
                        conn.Open();
                        count = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Database error: " + ex.Message);
                    }
                }
            }
            return count > 0;
        }

        // ── CARS ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all cars from the Cars table.
        /// AvailableColors is stored as a comma-separated string in SQL
        /// (e.g. "White, Red, Blue") and split back into a string[] on load.
        /// Used by: FleetStatusViewModel, BrowseCarsViewModel, AddCarViewModel.
        /// </summary>
        public static async Task<List<CarModel>> GetAll()
        {
            var cars = new List<CarModel>();
            string query = "SELECT * FROM Cars";
            try
            {
                using (SqlConnection connection = new SqlConnection(_conn))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
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
                                    // Split "White, Gray" → ["White", "Gray"]
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
            catch (Exception ex) { MessageBox.Show("Database connection failed: " + ex.Message); }
            return cars;
        }

        /// <summary>
        /// Inserts a new car into the Cars table.
        /// AvailableColors array is joined back to a comma-separated string for storage.
        /// Called by AddCarViewModel.SaveCommand after all validation passes.
        /// </summary>
        public static async Task AddCar(CarModel car)
        {
            string query = "INSERT INTO Cars (CarId, Name, Category, FuelType, Status, PricePerHour, ImageUrl, AvailableColors) "
                         + "VALUES (@id, @name, @cat, @fuel, @status, @price, @img, @colors)";
            try
            {
                using (SqlConnection connection = new SqlConnection(_conn))
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
                        // Join ["White", "Gray"] → "White, Gray" for SQL storage
                        cmd.Parameters.AddWithValue("@colors", string.Join(", ", car.AvailableColors));
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Database connection failed: " + ex.Message); }
        }

        /// <summary>
        /// Permanently deletes a car from the Cars table by CarId.
        /// Called by AddCarViewModel.DeleteCommand after confirmation.
        /// Business rule guards (Rented/Maintenance) are enforced in the ViewModel before this runs.
        /// </summary>
        public static async Task DeleteCar(string carId)
        {
            string query = "DELETE FROM Cars WHERE CarId = @id";
            try
            {
                using (SqlConnection connection = new SqlConnection(_conn))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", carId);
                        // ExecuteNonQuery for DELETE/INSERT/UPDATE — no rows returned
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Database connection failed: " + ex.Message); }
        }

        /// <summary>
        /// Updates all editable fields of an existing car in the Cars table.
        /// Called by AddCarViewModel.UpdateCommand and by RentCarViewModel/ProcessReturnViewModel
        /// to change Status (Available → Rented → Available).
        /// </summary>
        public static async Task UpdateCar(CarModel car)
        {
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
                using (SqlConnection conn = new SqlConnection(_conn))
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
                        // Re-join colors array to comma string for SQL storage
                        cmd.Parameters.AddWithValue("@colors", string.Join(", ", car.AvailableColors));
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Database connection failed: " + ex.Message); }
        }

        // ── RENTALS ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all rental records from the Rentals table.
        /// Used by RevenueViewModel (admin revenue report) and ProcessReturnViewModel (admin returns).
        /// </summary>
        /// <summary>
        /// Fetches the complete history of all rental transactions from the database.
        /// This data is used by the RevenueViewModel to calculate Gross Revenue,
        /// Average Rental income, and total rental counts.
        /// </summary>
        /// <returns>A list of RentalModel objects representing every row in the Rentals table.</returns>
        public static async Task<List<RentalModel>> GetAllRentals()
        {
            var list = new List<RentalModel>();
            // Logic: Select every column. Ensure your database table 'Rentals' matches these column names exactly.
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
                        // Mapping: Convert Database types to C# Model properties
                        RentalId = reader["RentalId"].ToString() ?? "",
                        CustomerId = reader["CustomerId"].ToString() ?? "",
                        CarId = reader["CarId"].ToString() ?? "",
                        CarName = reader["CarName"].ToString() ?? "",
                        DriverName = reader["DriverName"].ToString() ?? "",
                        Color = reader["Color"].ToString() ?? "",

                        // Numeric conversions (Crucial for Revenue calculations)
                        Hours = Convert.ToInt32(reader["Hours"]),
                        BasePrice = Convert.ToDecimal(reader["BasePrice"]),
                        Deposit = Convert.ToDecimal(reader["Deposit"]),

                        // TotalAmount: This is the field the Revenue Dashboard sums up.
                        // If this isn't updated during the 'Return' process, the revenue won't change.
                        TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),

                        RentalDate = Convert.ToDateTime(reader["RentalDate"]),
                        Status = reader["Status"].ToString() ?? "Active"
                    });
                }
            }
            catch (Exception ex)
            {
                // Note: In a production app, consider logging this to a file instead of showing a MessageBox here.
                MessageBox.Show("GetAllRentals failed: " + ex.Message);
            }

            return list;
        }

        /// <summary>
        /// Returns only the rentals belonging to a specific customer.
        /// Used by MyRentalsViewModel and MyAccountViewModel to show a customer's history.
        /// Filters by CustomerId which corresponds to the Users.UserID (e.g. "C001").
        /// </summary>
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

        /// <summary>
        /// Generates the next RentalId by counting existing rows (e.g. COUNT=5 → "R0006").
        /// Uses COUNT(*) which is safe here because rental rows are never deleted.
        /// Falls back to a timestamp-derived ID if the DB is unreachable.
        /// </summary>
        public static async Task<string> GetNextRentalId()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Rentals", conn);
                var scalar = await cmd.ExecuteScalarAsync();
                int count = scalar != null ? (int)scalar : 0;
                return $"R{(count + 1):D4}";
            }
            catch { return $"R{DateTime.Now.Ticks:D4}"; }
        }

        /// <summary>
        /// Inserts a new rental record into the Rentals table.
        /// Called by RentCarViewModel.ConfirmCommand after the customer confirms booking.
        /// Throws on failure so the ViewModel can catch and show a user-friendly message.
        /// </summary>
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
            // throw; so the caller (ViewModel) can catch and show a message
            catch (Exception ex) { MessageBox.Show("SaveRental failed: " + ex.Message); throw; }
        }

        /// <summary>
        /// Updates the Status column of a single rental record.
        /// Called by ProcessReturnViewModel.ReturnCommand to mark rentals as "Returned".
        /// </summary>
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

        // ── MAINTENANCE ───────────────────────────────────────────────────────

        /// <summary>
        /// Returns all maintenance records from the Maintenance table.
        /// Used by MaintenanceViewModel to populate the admin maintenance list.
        /// EndDate is nullable — DBNull maps to null (C# nullable DateTime).
        /// </summary>
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
                        // EndDate is NULL in SQL while In Progress — map to C# null
                        EndDate = reader["EndDate"] == DBNull.Value
                                         ? null
                                         : Convert.ToDateTime(reader["EndDate"]),
                        Cost = Convert.ToDecimal(reader["Cost"]),
                        Status = reader["Status"].ToString() ?? "In Progress"
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("GetAllMaintenance failed: " + ex.Message); }
            return list;
        }

        /// <summary>
        /// Generates the next MaintenanceId by counting existing rows (e.g. "M0003").
        /// Same strategy as GetNextRentalId — safe because maintenance rows are never deleted.
        /// </summary>
        public static async Task<string> GetNextMaintenanceId()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Maintenance", conn);
                var scalar = await cmd.ExecuteScalarAsync();
                int count = scalar != null ? (int)scalar : 0;
                return $"M{(count + 1):D4}";
            }
            catch { return $"M{DateTime.Now.Ticks:D4}"; }
        }

        /// <summary>
        /// Inserts a new maintenance record into the Maintenance table.
        /// Called by MaintenanceViewModel.SendToMaintCommand.
        /// Throws on failure so the ViewModel can inform the admin.
        /// </summary>
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

        /// <summary>
        /// Marks a maintenance record as Completed, sets its EndDate and final Cost.
        /// Called by MaintenanceViewModel.CompleteCommand.
        /// The car's status is updated to Available separately in the ViewModel.
        /// </summary>
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

        // ── FILE PATH HELPERS ─────────────────────────────────────────────────
        // All text files are saved under RentalRevData/ next to the .exe.
        // Directory.CreateDirectory is called before each path is returned
        // so the folder is always guaranteed to exist.
        //
        // Folder layout:
        //   RentalRevData/
        //     Customers/
        //       C001/Receipts/Receipt_R0001.txt
        //       C002/Receipts/Receipt_R0002.txt
        //     Admin/
        //       ReturnReports/Return_R0001.txt
        //       MaintenanceLogs/Maint_M0001.txt
        //       RevenueReports/Revenue_20260418_143000.txt

        private static readonly string _baseDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RentalRevData");

        /// <summary>Returns the path for a customer's rental receipt file, creating folders as needed.</summary>
        private static string CustomerReceiptPath(string customerId, string rentalId)
        {
            string dir = Path.Combine(_baseDir, "Customers", customerId, "Receipts");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"Receipt_{rentalId}.txt");
        }

        /// <summary>Returns the path for an admin return report file, creating folders as needed.</summary>
        private static string ReturnReportPath(string rentalId)
        {
            string dir = Path.Combine(_baseDir, "Admin", "ReturnReports");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"Return_{rentalId}.txt");
        }

        /// <summary>Returns the path for an admin maintenance log file, creating folders as needed.</summary>
        private static string MaintenanceLogPath(string maintenanceId)
        {
            string dir = Path.Combine(_baseDir, "Admin", "MaintenanceLogs");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"Maint_{maintenanceId}.txt");
        }

        /// <summary>Returns a timestamped path for an admin revenue report, creating folders as needed.</summary>
        private static string RevenueReportPath()
        {
            string dir = Path.Combine(_baseDir, "Admin", "RevenueReports");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"Revenue_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        }

        // ── FILE GENERATORS ───────────────────────────────────────────────────

        /// <summary>
        /// Generates a plain-text receipt for the customer after a successful booking.
        /// Saved to: RentalRevData/Customers/{CustomerId}/Receipts/Receipt_{RentalId}.txt
        /// Called by RentCarViewModel.ConfirmCommand immediately after SaveRental succeeds.
        /// Wrapped in try-catch so a file system error never crashes the rental flow.
        /// </summary>
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

        /// <summary>
        /// Generates a plain-text return report for admin records after a car is returned.
        /// Saved to: RentalRevData/Admin/ReturnReports/Return_{RentalId}.txt
        /// Called by ProcessReturnViewModel.ReturnCommand after UpdateRentalStatus succeeds.
        /// </summary>
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

        /// <summary>
        /// Generates a plain-text maintenance log after a car is sent to or returned from maintenance.
        /// Saved to: RentalRevData/Admin/MaintenanceLogs/Maint_{MaintenanceId}.txt
        /// Called by MaintenanceViewModel after SendToMaintCommand or CompleteCommand.
        /// </summary>
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

        /// <summary>
        /// Generates a timestamped revenue report text file for the admin.
        /// Saved to: RentalRevData/Admin/RevenueReports/Revenue_{timestamp}.txt
        /// Called by RevenueViewModel.ExportReportCommand.
        /// </summary>
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

        // ── CUSTOMER REGISTRATION ─────────────────────────────────────────────

        /// <summary>
        /// Generates the next available CustomerID by checking the MAX number
        /// in BOTH the Users and Customers tables.
        /// This prevents ID collision when a previous registration attempt left an
        /// orphaned row in Users (inserted) but not in Customers (failed on constraint).
        /// Returns "C{max+1:D3}" — e.g. if max is 6, returns "C007".
        /// Falls back to a timestamp-based ID if the DB is unreachable.
        /// </summary>
        public static async Task<string> GetNextCustomerId()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                // UNION ALL both tables to find the absolute highest C-number ever used
                string query = @"
            SELECT ISNULL(MAX(num), 0) FROM (
                SELECT CAST(SUBSTRING(UserID, 2, LEN(UserID)) AS INT) AS num
                FROM Users
                WHERE UserID LIKE 'C[0-9][0-9][0-9]'
                UNION ALL
                SELECT CAST(SUBSTRING(CustomerID, 2, LEN(CustomerID)) AS INT) AS num
                FROM Customers
                WHERE CustomerID LIKE 'C[0-9][0-9][0-9]'
            ) AS all_ids";
                using var cmd = new SqlCommand(query, conn);
                var scalar = await cmd.ExecuteScalarAsync();
                int maxNum = scalar != null ? Convert.ToInt32(scalar) : 0;
                return $"C{(maxNum + 1):D3}";
            }
            catch
            {
                // Absolute fallback — timestamp mod to get a 3-digit number
                return $"C{(DateTime.Now.Ticks % 900 + 100):D3}";
            }
        }

        /// <summary>
        /// Returns true if a username already exists in the Customers table.
        /// Used by SignUpViewModel before registration to give a friendly error
        /// instead of letting the UNIQUE constraint throw a SQL exception.
        /// </summary>
        public static async Task<bool> UsernameExists(string username)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Customers WHERE Username = @u", conn);
                cmd.Parameters.AddWithValue("@u", username);
                var scalar = await cmd.ExecuteScalarAsync();
                int count = scalar != null ? (int)scalar : 0;
                return count > 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Permanently deletes a customer account and all associated data.
        /// Deletes in order: Rentals → Customers → Users (FK dependency chain).
        /// Called by MyAccountViewModel.DeleteAccountCommand after double-confirmation.
        /// Throws on failure so the ViewModel can inform the user.
        /// </summary>
        public static async Task DeleteAccount(string customerId)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                // Step 1: Remove rentals first — FK from Rentals.CustomerId → Users.UserID
                using var cmd1 = new SqlCommand(
                    "DELETE FROM Rentals WHERE CustomerId = @id", conn);
                cmd1.Parameters.AddWithValue("@id", customerId);
                await cmd1.ExecuteNonQueryAsync();

                // Step 2: Remove from Customers — FK from Customers.CustomerID → Users.UserID
                using var cmd2 = new SqlCommand(
                    "DELETE FROM Customers WHERE CustomerID = @id", conn);
                cmd2.Parameters.AddWithValue("@id", customerId);
                await cmd2.ExecuteNonQueryAsync();

                // Step 3: Remove from Users — no remaining FK references
                using var cmd3 = new SqlCommand(
                    "DELETE FROM Users WHERE UserID = @id", conn);
                cmd3.Parameters.AddWithValue("@id", customerId);
                await cmd3.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Account deletion failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Inserts a new customer into both the Users and Customers tables
        /// using the sp_RegisterCustomer stored procedure (which wraps both
        /// inserts in a transaction — if one fails, both roll back).
        ///
        /// Returns a tuple:
        ///   (Success=true, AlreadyExists=false, NewId) — new account created.
        ///   (Success=true, AlreadyExists=true,  NewId) — duplicate detected but
        ///     username already exists in Customers — treat as success (prior attempt succeeded).
        ///
        /// SQL error numbers 2627/2601 = UNIQUE KEY violation — handled silently
        /// to hide raw DB errors from the user and show a friendly message instead.
        /// </summary>
        public static async Task<(bool Success, bool AlreadyExists, string NewId)>
            RegisterCustomer(CustomerModel c)
        {
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
                return (true, false, c.CustomerId);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                // 2627 = UNIQUE KEY violation, 2601 = duplicate key in index
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    // Check if Customers table has the username — if yes, a previous
                    // attempt succeeded and this is a retry → treat as success
                    bool customerExists = await UsernameExists(c.Username);
                    if (customerExists)
                        return (true, true, c.CustomerId);
                }
                throw; // Any other SQL error — rethrow for ViewModel to handle
            }
        }

        // ── FORGOT PASSWORD ───────────────────────────────────────────────────

        /// <summary>
        /// Looks up the security question for a given username via sp_GetSecurityQuestion.
        /// Used by ForgotPasswordViewModel.FindAccountCommand (Step 1 of password reset).
        /// Returns (Found=false, "", "") if the username doesn't exist.
        /// </summary>
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

        /// <summary>
        /// Verifies the security answer and resets the password via sp_ResetPassword.
        /// Used by ForgotPasswordViewModel.ResetPasswordCommand (Step 2 of password reset).
        /// The SP compares answers case-insensitively and updates both Users and Customers.
        /// Returns (false, errorMessage) if the answer is wrong.
        /// </summary>
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
                // SP raises an error if the answer is wrong — surface message to user
                return (false, ex.Message);
            }
        }

        // ── PROCESS RETURN ────────────────────────────────────────────────────

        /// <summary>
        /// Processes a car return via sp_ProcessReturn.
        /// The SP applies early/late/on-time pricing logic, updates the Rentals
        /// table with the final amount and status, and sets the Car back to Available.
        /// Returns the final amount charged and the return status string.
        /// Called by ProcessReturnViewModel.ReturnCommand.
        /// </summary>
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

        // ── CONVENIENCE WRAPPERS (for simpler ViewModel calls) ────────────────

        /// <summary>
        /// Returns only cars with Status = "Available".
        /// BrowseCarsViewModel uses GetAll() + LINQ filter directly for more control,
        /// but this is available for simpler use cases.
        /// </summary>
        public static async Task<List<CarModel>> GetAvailable()
        {
            List<CarModel> allCars = await GetAll();
            List<CarModel> availableCars = new List<CarModel>();
            foreach (var car in allCars)
                if (car.Status == "Available")
                    availableCars.Add(car);
            return availableCars;
        }

        /// <summary>
        /// Legacy in-memory filter — returns rentals from the in-memory Rentals list.
        /// Not used by active windows (MyRentalsViewModel uses GetRentalsByCustomer instead).
        /// Kept for backward compatibility.
        /// </summary>
        public static List<RentalModel> GetByCustomer(string id)
        {
            var result = new List<RentalModel>();
            foreach (var rental in Rentals)
                if (rental.CustomerId == id)
                    result.Add(rental);
            return result;
        }

        /// <summary>
        /// Fetches a single car from SQL by CarId.
        /// Used by ProcessReturnViewModel and MaintenanceViewModel to get
        /// a car object before updating its Status.
        /// Returns null if not found.
        /// </summary>
        public static async Task<CarModel?> GetById(string carId)
        {
            List<CarModel> allCars = await GetAll();
            foreach (var car in allCars)
                if (car.CarId == carId)
                    return car;
            return null;
        }

        /// <summary>
        /// Legacy method — removes a car from the in-memory Cars list.
        /// Not used by active windows (AddCarViewModel calls DeleteCar() for SQL deletion).
        /// Kept for backward compatibility.
        /// </summary>
        public static async void RemoveCar(string carId)
        {
            var car = await GetById(carId);
            if (car != null)
                Cars.Remove(car);
        }

        /// <summary>
        /// Legacy method — adds a rental to the in-memory Rentals list.
        /// Not used by active windows (RentCarViewModel calls SaveRental() for SQL insertion).
        /// Kept for backward compatibility.
        /// </summary>
        public static void AddRental(RentalModel rental)
        {
            Rentals.Add(rental);
        }
    }
}