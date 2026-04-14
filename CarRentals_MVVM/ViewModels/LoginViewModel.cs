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
            // Trusted_Connection=True replacement on user id and pasword if using laptop

            string connectionString = @"Server=CCL2-12\MSSQLSERVER01;Database=RENTAL_REVS_DATABASE; 
        User Id=sa;Password=ccl2;TrustServerCertificate=True;";

            // Validate credentials based on the user's role
            bool isValid = false;


            // Extract the password from the PasswordBox if it was passed in

            if (parameter is PasswordBox passwordBox)
            {
                CurrentUser.Password = passwordBox.Password;
            }

            // Validation: If there are spaces, fail immediately without calling the DB

            if (CurrentUser.UserID.Contains(" ") || CurrentUser.Password.Contains(" "))
            {
                ErrorMessage = "Spaces are not allowed in credentials.";
                ErrorVisible = true;
                return;
            }

            //' OR '1'='1  || ' OR '1'='1' -- One of the text to bypass username or password

            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                {
                    string query = "SELECT * FROM Users WHERE UserID = @username AND Password = @password"; // Pass parameters to ensure bypass prevention

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //prevent injection attacks
                        command.Parameters.AddWithValue("@username", CurrentUser.UserID);
                        command.Parameters.AddWithValue("@password", CurrentUser.Password);
                        await connection.OpenAsync();

                        using (SqlDataReader reader =  await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                isValid = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection failed: " + ex.Message);
            }

          

            //if (CurrentUser.Role == "Admin")
            //{
            //    isValid = CurrentUser.UserID.Trim() == "A001"
            //           && CurrentUser.Password.Trim() == "admin123";
            //}
            //else
            //{
            //    isValid = CurrentUser.UserID.Trim() == "C001"
            //           && CurrentUser.Password.Trim() == "customer123";
            //}

            if (isValid)
            {
                // Clear any previous error and navigate to the correct dashboard
                ErrorVisible = false;

                if (CurrentUser.Role == "Admin")
                {
                    var dashboard = new View.AdminDashboard(CurrentUser.UserID);
                    NavigationService.Navigate(dashboard);
                }
                else
                {
                    var dashboard = new View.CustomerDashboard(CurrentUser.UserID);
                    NavigationService.Navigate(dashboard);
                }
            }
            else
            {
                // Show a role-appropriate error message with credential hint
                if (CurrentUser.Role == "Admin")
                {
                    ErrorMessage = "Invalid Agent ID or password. Hint: A001 / admin123";
                }
                else
                {
                    ErrorMessage = "Invalid Customer ID or password. Hint: C001 / customer123";
                }

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
            MessageBox.Show(
                "Instructions sent to your campus email.",
                "Forgot Password",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}