using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for the User Registration (Sign Up) screen.
    /// Handles comprehensive form validation, profile picture selection, 
    /// and multi-step confirmation before saving to the database.
    /// </summary>
    public class SignUpViewModel : ObservableObject
    {
        // ── Backing fields ─────────────────────────────────────────────────────
        private string _fullName = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _confirmPass = string.Empty;
        private string _contact = string.Empty;
        private string _license = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _errorVisible = false;
        private bool _isLoading = false;
        private string _securityQuestion = "What is your pet name?";
        private string _securityAnswer = string.Empty;
        private string _profilePicturePath = string.Empty;
        private bool _userConfirmed = false; // Prevents double tap and triggers confirmation dialog

        /// <summary>Helper property for the UI to determine if a profile image preview should be shown.</summary>
        public bool HasProfilePicture => !string.IsNullOrEmpty(_profilePicturePath);

        // ── Properties — Logic is kept inside setters for real-time validation feedback ─

        public string FullName
        {
            get => _fullName;
            set
            {
                // Logic: Block numbers or symbols in the name field
                if (!string.IsNullOrEmpty(value) && value.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c)))
                {
                    ShowError("Full Name must be letters and spaces only. Example: 'Juan Dela Cruz'");
                    return;
                }
                _fullName = value;
                OnPropertyChanged();
                ClearErrorIfAny();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                // Logic: Enforce no-space policy for usernames
                if (!string.IsNullOrEmpty(value) && value.Contains(" "))
                {
                    ShowError("Username cannot contain spaces. Example: 'juandc'");
                    return;
                }
                _username = value;
                OnPropertyChanged();
                ClearErrorIfAny();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value; OnPropertyChanged();
            }
        }

        public string ConfirmPass
        {
            get => _confirmPass;
            set
            {
                _confirmPass = value; OnPropertyChanged();
            }
        }

        public string Contact
        {
            get => _contact;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _contact = value;
                    OnPropertyChanged();
                    return;
                }
                // Logic: Ensure contact field only contains numeric digits
                if (value.Any(c => !char.IsDigit(c)))
                {
                    ShowError("Contact Number must be digits only. Example: '09171234567'");
                    return;
                }
                // Logic: Enforce standard PH mobile number length
                if (value.Length > 11)
                {
                    ShowError("Contact Number must be exactly 11 digits. Example: '09171234567'");
                    return;
                }
                _contact = value;
                OnPropertyChanged();
                ClearErrorIfAny();
            }
        }

        public string License
        {
            get => _license;
            set
            {
                _license = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(value) && value.Trim().Length < 4)
                {
                    ShowError("License must be at least 4 characters. Example: 'N01-23-456789'");
                }
                else
                {
                    ClearErrorIfAny();
                }
            }
        }

        /// <summary>The text message displayed in the UI error banner.</summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        /// <summary>Controls the visibility of the red error panel in the XAML.</summary>
        public bool ErrorVisible
        {
            get => _errorVisible;
            set { _errorVisible = value; OnPropertyChanged(); }
        }

        /// <summary>Indicates if the registration task is currently running.</summary>
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string SecurityQuestion
        {
            get => _securityQuestion;
            set { _securityQuestion = value; OnPropertyChanged(); }
        }

        public string SecurityAnswer
        {
            get => _securityAnswer;
            set { _securityAnswer = value; OnPropertyChanged(); }
        }

        public string ProfilePicturePath
        {
            get => _profilePicturePath;
            set
            {
                _profilePicturePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasProfilePicture));
            }
        }

        // ── Commands ───────────────────────────────────────────────────────────

        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand PickProfilePictureCommand { get; }

        public SignUpViewModel()
        {
            // Back navigation to login page
            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.CustomerLogin()));

            // Command to trigger the Windows File Explorer for image selection
            PickProfilePictureCommand = new RelayCommand(_ =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                    Title = "Select Profile Picture"
                };
                if (dialog.ShowDialog() == true)
                {
                    ProfilePicturePath = dialog.FileName;
                    _userConfirmed = false; // Reset confirmation if picture changes
                }
            });

            // Main registration logic
            RegisterCommand = new AsyncRelayCommand(async _ =>
            {
                if (IsLoading) return;

                ErrorVisible = false;

                // ── 1. Mandatory Field Presence Checks ─────────────────────────
                if (string.IsNullOrWhiteSpace(FullName)) { ShowError("Full Name is required."); return; }
                if (string.IsNullOrWhiteSpace(Username)) { ShowError("Username is required."); return; }
                if (string.IsNullOrWhiteSpace(Password)) { ShowError("Password is required."); return; }
                if (string.IsNullOrWhiteSpace(ConfirmPass)) { ShowError("Please confirm your password."); return; }
                if (string.IsNullOrWhiteSpace(Contact)) { ShowError("Contact Number is required."); return; }
                if (string.IsNullOrWhiteSpace(License)) { ShowError("License Number is required."); return; }
                if (string.IsNullOrWhiteSpace(SecurityAnswer)) { ShowError("Security Answer is required."); return; }

                // ── 2. Data Integrity/Format Checks ─────────────────────────────
                if (Password.Length < 6) { ShowError($"Password must be at least 6 characters (you entered {Password.Length})."); return; }
                if (Password.Contains(" ")) { ShowError("Password cannot contain spaces."); return; }
                if (Password != ConfirmPass) { ShowError("Passwords do not match. Please re-enter."); return; }
                if (Contact.Length != 11 || !Contact.StartsWith("09") || !Contact.All(char.IsDigit)) { ShowError("Contact must be 11 digits starting with 09."); return; }
                if (License.Trim().Length < 4) { ShowError("License Number must be at least 4 characters."); return; }

                // ── 3. UX: Confirmation Step ────────────────────────────────────
                // Before talking to DB, show the user their data for final verification.
                if (!_userConfirmed)
                {
                    var confirm = MessageBox.Show(
                        $"Please confirm your account details:\n\n" +
                        $"Name: {FullName}\n" +
                        $"Username: {Username}\n" +
                        $"Contact: {Contact}\n" +
                        $"License: {License}\n\n" +
                        "Are you sure you want to create this account?",
                        "Confirm Account Creation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirm == MessageBoxResult.Yes)
                    {
                        _userConfirmed = true;
                    }
                    else
                    {
                        return; // User cancelled
                    }
                }

                // ── 4. DB Communication (Background Thread) ─────────────────────
                IsLoading = true;

                try
                {
                    // Check for existing users to avoid SQL Constraint violations
                    if (await CarDataService.UsernameExists(Username))
                    { ShowError($"Username '{Username}' is already taken."); return; }

                    if (await CarDataService.ContactExists(Contact))
                    { ShowError("That contact number is already registered."); return; }

                    if (await CarDataService.LicenseExists(License.Trim()))
                    { ShowError("That license number is already registered."); return; }

                    // Generate Next Customer ID (e.g. C007)
                    string newId = await CarDataService.GetNextCustomerId();

                    // Construct the data model
                    var customer = new CustomerModel
                    {
                        CustomerId = newId,
                        FullName = FullName.Trim(),
                        Username = Username.Trim(),
                        Password = Password,
                        ContactNumber = Contact.Trim(),
                        LicenseNumber = License.Trim(),
                        SecurityQuestion = SecurityQuestion,
                        SecurityAnswer = SecurityAnswer.Trim(),
                        ProfilePicturePath = ProfilePicturePath
                    };

                    // Execute the combined registration procedure
                    var (success, alreadyExists, finalId) = await CarDataService.RegisterCustomer(customer);

                    if (success)
                    {
                        // Handle profile picture update if one was chosen
                        if (!string.IsNullOrEmpty(ProfilePicturePath))
                        {
                            await CarDataService.UpdateCustomerProfile(finalId, Username.Trim(), ProfilePicturePath);
                        }

                        ShowSuccessAndNavigate();
                    }
                }
                catch (Exception ex)
                {
                    // Generic error catch-all for DB connection issues
                    ShowError(ex.Message);
                }
                finally
                {
                    IsLoading = false;
                    _userConfirmed = false;
                }
            });
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void ShowSuccessAndNavigate()
        {
            MessageBox.Show(
                $"Account created successfully!\n\n" +
                $"Username: {Username}\n\n" +
                "You can now log in using your username.",
                "Registration Successful",
                MessageBoxButton.OK, MessageBoxImage.Information);

            NavigationService.Navigate(new View.CustomerLogin());
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisible = true;
            IsLoading = false;
        }

        private void ClearErrorIfAny()
        {
            if (_errorVisible)
            {
                ErrorVisible = false;
                ErrorMessage = string.Empty;
            }
        }
    }
}