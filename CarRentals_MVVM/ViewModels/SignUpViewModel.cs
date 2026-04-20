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

        public bool HasProfilePicture => !string.IsNullOrEmpty(_profilePicturePath);

        // ── Properties — NO blocking MessageBox in setters, inline errors only ─

        public string FullName
        {
            get => _fullName;
            set
            {
                // Reject digits/symbols inline, don't block with popup
                if (!string.IsNullOrEmpty(value) &&
                    value.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c)))
                {
                    ShowError("Full Name must be letters and spaces only. Example: 'Juan Dela Cruz'");
                    return; // don't update backing field
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

        // Password is set from code-behind (PasswordBox), not from XAML binding
        public string Password
        {
            get => _password;
            set {

                if(!string.IsNullOrEmpty(value) && value.Length < 6)
                {
                    ShowError("Password must be at least 6 characters. Example: 'P@ssw0rd'");
                }
                else if (!string.IsNullOrEmpty(value) && value.Contains(" "))
                {
                    ShowError("Password cannot contain spaces. Example: 'P@ssw0rd'");
                }
                else
                {
                    _password = value; OnPropertyChanged();
            
            
                }
           
}
        }

        public string ConfirmPass
        {
            get => _confirmPass;
            set {

                if (!string.IsNullOrEmpty(value) && value.Length < 6)
                {

                    ShowError("Confirm Password must be at least 6 characters. Example: 'P@ssw0rd'");
                }
                else if (!string.IsNullOrEmpty(value) && value.Contains(" "))
                { 
                    ShowError("Confirm Password cannot contain spaces. Example: 'P@ssw0rd'");
                }
                else

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
                // Only digits — reject invalid chars immediately
                if (value.Any(c => !char.IsDigit(c)))
                {
                    ShowError("Contact Number must be digits only. Example: '09171234567'");
                    return;
                }
                // Cap at 11 digits
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
                // Only inline hint — never block
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        // ErrorVisible NEVER hides the button — only the error banner uses this
        public bool ErrorVisible
        {
            get => _errorVisible;
            set { _errorVisible = value; OnPropertyChanged(); }
        }

        // IsLoading disables the button while async ops run
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
                OnPropertyChanged(nameof(HasProfilePicture)); // ← notify so image shows
            }
        }

        // ── Commands ───────────────────────────────────────────────────────────

        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand PickProfilePictureCommand { get; }

        public SignUpViewModel()
        {
            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.CustomerLogin()));

            PickProfilePictureCommand = new RelayCommand(_ =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                    Title = "Select Profile Picture"
                };
                if (dialog.ShowDialog() == true)
                    ProfilePicturePath = dialog.FileName;
            });

            RegisterCommand = new RelayCommand(async _ =>
            {
                // RegisterCommand receives passwords pushed from code-behind
                // before this executes (see SignUpWindow.xaml.cs RegisterBtn_Click)

                ErrorVisible = false;
                IsLoading = true;

                try
                {
                    // ── 1. Empty field check ───────────────────────────────────
                    if (string.IsNullOrWhiteSpace(FullName))
                    { ShowError("Full Name is required."); return; }

                    if (string.IsNullOrWhiteSpace(Username))
                    { ShowError("Username is required."); return; }

                    if (string.IsNullOrWhiteSpace(Password))
                    { ShowError("Password is required."); return; }

                    if (string.IsNullOrWhiteSpace(ConfirmPass))
                    { ShowError("Please confirm your password."); return; }

                    if (string.IsNullOrWhiteSpace(Contact))
                    { ShowError("Contact Number is required."); return; }

                    if (string.IsNullOrWhiteSpace(License))
                    { ShowError("License Number is required."); return; }

                    if (string.IsNullOrWhiteSpace(SecurityAnswer))
                    { ShowError("Security Answer is required."); return; }

                    // ── 2. Format checks ──────────────────────────────────────
                    if (Password.Length < 6)
                    { ShowError($"Password must be at least 6 characters (you entered {Password.Length})."); return; }

                    if (Password.Contains(" "))
                    { ShowError("Password cannot contain spaces."); return; }

                    if (Password != ConfirmPass)
                    { ShowError("Passwords do not match. Please re-enter."); return; }

                    if (Contact.Length != 11 || !Contact.StartsWith("09") || !Contact.All(char.IsDigit))
                    { ShowError("Contact must be 11 digits starting with 09. Example: 09171234567"); return; }

                    if (License.Trim().Length < 4)
                    { ShowError("License Number must be at least 4 characters."); return; }

                    // ── 3. Duplicate checks (DB calls) ────────────────────────
                    if (await CarDataService.UsernameExists(Username))
                    { ShowError($"Username '{Username}' is already taken. Please choose another."); return; }

                    if (await CarDataService.ContactExists(Contact))
                    { ShowError("That contact number is already registered to another account."); return; }

                    if (await CarDataService.LicenseExists(License.Trim()))
                    { ShowError("That license number is already registered to another account."); return; }

                    // ── 4. Create account ─────────────────────────────────────
                    string newId = await CarDataService.GetNextCustomerId();

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

                    // Save the user data
                    await CarDataService.RegisterCustomer(customer);

                    // FIXED HERE: Immediately save the profile picture to the database!
                    if (!string.IsNullOrEmpty(ProfilePicturePath))
                    {
                        await CarDataService.UpdateCustomerProfile(newId, Username.Trim(), ProfilePicturePath);
                    }

                    // ── 5. Success ─────────────────────────────────────────────
                    MessageBox.Show(
                        $"Account created successfully!\n\nYour Customer ID: {newId}\nUsername: {Username}\n\nYou can now log in using your username.",
                        "Registration Successful",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    NavigationService.Navigate(new View.CustomerLogin());
                }
                catch (Exception ex)
                {
                    ShowError("Registration failed: " + ex.Message);
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        // ── Helpers ────────────────────────────────────────────────────────────

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