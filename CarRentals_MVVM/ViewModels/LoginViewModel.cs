using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;
using Microsoft.Data.SqlClient;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel shared by both AdminLogin.xaml and CustomerLogin.xaml.
    /// Handles credential validation and navigates to the appropriate dashboard
    /// based on the user's role.
    /// Connected to: AdminLogin.xaml.cs (role = "Admin"),
    /// CustomerLogin.xaml.cs (role = "Customer"),
    /// AdminDashboard.xaml and CustomerDashboard.xaml (next windows after login),
    /// UserModel (holds the credentials being validated).
    /// </summary>
    public class LoginViewModel : ObservableObject
    {
        /// <summary>
        /// Holds the current user's ID, password, and role during login.
        /// The UserID and Role are bound to fields in the login XAML.
        /// </summary>
        public UserModel CurrentUser { get; set; }

        /// <summary>
        /// Triggers credential validation when the Login button is clicked.
        /// Receives the PasswordBox as a parameter to extract the password securely.
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// Navigates back to the ChooseRole screen.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Shows a placeholder "forgot password" message.
        /// </summary>
        public ICommand ForgotPasswordCommand { get; }

        public ICommand SignUpCommand { get; }

        // ── Error display properties ───────────────────────────────────────────

        private bool _errorVisible = false;

        /// <summary>
        /// Controls whether the error message panel is visible in the UI.
        /// Set to true when login fails, false when login succeeds.
        /// Bound to the error Border's Visibility in the login XAML.
        /// </summary>
        public bool ErrorVisible
        {
            get => _errorVisible;
            set
            {
                _errorVisible = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage = string.Empty;

        /// <summary>
        /// The error text shown when credentials are invalid.
        /// Includes a hint for the default credentials during the prototype stage.
        /// Bound to the error TextBlock in the login XAML.
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the LoginViewModel for the given role.
        /// </summary>
        /// <param name="role">"Admin" or "Customer" — passed in from the code-behind.</param>
        public LoginViewModel(string role)
        {
            // Create the user model with the role pre-set
            CurrentUser = new UserModel { Role = role };

            // Assign commands to their handler methods
            LoginCommand = new RelayCommand(ExecuteLogin);
            BackCommand = new RelayCommand(ExecuteBack);
            ForgotPasswordCommand = new RelayCommand(ExecuteForgotPassword);
            SignUpCommand = new RelayCommand(_ => NavigationService.Navigate(new View.SignUpWindow()));
        }

        /// <summary>
        /// Validates the entered credentials against the expected values.
        /// On success: navigates to the appropriate dashboard.
        /// On failure: shows an error message with a login hint.
        /// </summary>
        /// <param name="parameter">
        /// The PasswordBox from the XAML, passed via CommandParameter binding.
        /// This is the only acceptable use of a UI element in ViewModel —
        /// PasswordBox cannot be bound directly due to security restrictions.
        /// </param>
        private async void ExecuteLogin(object? parameter)
        {
            string connectionString = @"Server=DESKTOP-8P1VJSE;Database=RENTAL_REVS_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;";
            // school pc: @"Server=CCL2-12\MSSQLSERVER01;Database=RENTAL_REVS_DATABASE;User Id=sa;Password=ccl2;TrustServerCertificate=True;"

            bool isValid = false;
            string matchedUserId = string.Empty;
            string matchedFullName = string.Empty;
            string matchedUsername = string.Empty;

            if (parameter is PasswordBox passwordBox)
                CurrentUser.Password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(CurrentUser.UserID) ||
                CurrentUser.UserID.Contains(" ") ||
                CurrentUser.Password.Contains(" "))
            {
                ErrorMessage = "Username cannot be empty or contain spaces.";
                ErrorVisible = true;
                return;
            }

            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                if (CurrentUser.Role == "Admin")
                {
                    string query = "SELECT * FROM Users WHERE UserID = @username AND Password = @password";
                    using var cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@username", CurrentUser.UserID);
                    cmd.Parameters.AddWithValue("@password", CurrentUser.Password);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        isValid = true;
                        matchedUserId = CurrentUser.UserID;
                    }
                }
                else
                {
                    // Customer: join Customers + Users tables, login by Username
                    string query = @"
                SELECT c.CustomerID, c.FullName, c.Username
                FROM Customers c
                INNER JOIN Users u ON u.UserID = c.CustomerID
                WHERE c.Username = @username AND u.Password = @password";
                    using var cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@username", CurrentUser.UserID);
                    cmd.Parameters.AddWithValue("@password", CurrentUser.Password);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        isValid = true;
                        matchedUserId = reader["CustomerID"].ToString() ?? "";
                        matchedFullName = reader["FullName"].ToString() ?? "";
                        matchedUsername = reader["Username"].ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection failed: " + ex.Message);
                return;
            }

            if (isValid)
            {
                ErrorVisible = false;

                if (CurrentUser.Role == "Admin")
                {
                    UserSession.UserId = matchedUserId;
                    UserSession.Username = matchedUserId;
                    UserSession.FullName = "Admin";
                    UserSession.Role = "Admin";
                    NavigationService.Navigate(new View.AdminDashboard(matchedUserId));
                }
                else
                {
                    // Populate global session so all windows can access it
                    UserSession.UserId = matchedUserId;
                    UserSession.Username = matchedUsername;
                    UserSession.FullName = matchedFullName;
                    UserSession.Role = "Customer";
                    NavigationService.Navigate(new View.CustomerDashboard(matchedUserId));
                }
            }
            else
            {
                ErrorMessage = CurrentUser.Role == "Admin"
                    ? "Invalid Agent ID or password."
                    : "Invalid username or password. Hint: use your registered Username.";
                ErrorVisible = true;
            }
        }

        /// <summary>
        /// Navigates back to the role selection screen.
        /// </summary>
        private void ExecuteBack(object? parameter)
        {
            NavigationService.Navigate(new View.ChooseRole());
        }

        /// <summary>
        /// Shows a placeholder forgot password message.
        /// In a real system this would send a reset email.
        /// </summary>
        private void ExecuteForgotPassword(object? parameter)
        {
            NavigationService.Navigate(new View.ForgotPasswordWindow(CurrentUser.Role));
        }
    }
}