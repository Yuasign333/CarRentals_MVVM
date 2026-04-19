using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class MyAccountViewModel : ObservableObject
    {
        private readonly string _userId;

        // Initialized strings to avoid Nullable Reference warnings
        private string _userLabel = string.Empty;
        public string UserLabel
        {
            get => _userLabel;
            set { _userLabel = value; OnPropertyChanged(); }
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string FullName { get; }
        public string CustomerId { get; }

        private string _contact = "Loading...";
        public string Contact
        {
            get => _contact;
            set { _contact = value; OnPropertyChanged(); }
        }

        private string _license = "Loading...";
        public string License
        {
            get => _license;
            set { _license = value; OnPropertyChanged(); }
        }

        private string _newUsername = string.Empty;
        public string NewUsername
        {
            get => _newUsername;
            set { _newUsername = value; OnPropertyChanged(); }
        }

        // Toggle state for the inline username text box
        private bool _isEditingUsername;
        public bool IsEditingUsername
        {
            get => _isEditingUsername;
            set { _isEditingUsername = value; OnPropertyChanged(); }
        }

        private string _profilePicturePath = string.Empty;
        public string ProfilePicturePath
        {
            get => _profilePicturePath;
            set { _profilePicturePath = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _errorVisible = false;
        public bool ErrorVisible
        {
            get => _errorVisible;
            set { _errorVisible = value; OnPropertyChanged(); }
        }

        public ObservableCollection<RentalModel> Rentals { get; } = new();

        private bool _hasRentals = false;
        public bool HasRentals
        {
            get => _hasRentals;
            set { _hasRentals = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand PickPictureCommand { get; }
        public ICommand EditUsernameCommand { get; }
        public ICommand SaveUsernameCommand { get; }
        public ICommand CancelEditUsernameCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand BrowseCarsCommand { get; }

        public MyAccountViewModel(string userId)
        {
            _userId = userId;
            CustomerId = userId;
            FullName = !string.IsNullOrEmpty(UserSession.FullName) ? UserSession.FullName : userId;
            Username = !string.IsNullOrEmpty(UserSession.Username) ? UserSession.Username : userId;
            UserLabel = $"Customer: {Username}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.CustomerDashboard(_userId)));

            BrowseCarsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId)));

            // FIXED: Using a dedicated method instead of an unawaited Task.Run to fix the CS4014 warning
            _ = LoadUserDataAsync(userId);

            // 1. CHANGE PROFILE PICTURE LOGIC
            PickPictureCommand = new RelayCommand(async _ =>
            {
                var result = MessageBox.Show("Do you want to change your profile picture?",
                                             "Change Photo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;

                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                    Title = "Select Profile Picture"
                };

                if (dialog.ShowDialog() == true)
                {
                    ProfilePicturePath = dialog.FileName;

                    // Instantly save to Database using existing Username so it doesn't get overwritten
                    await CarDataService.UpdateCustomerProfile(_userId, Username, ProfilePicturePath);

                    MessageBox.Show("Profile picture updated successfully!",
                                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });

            // 2. INLINE USERNAME EDITING LOGIC
            EditUsernameCommand = new RelayCommand(_ =>
            {
                NewUsername = Username; // Prefill the textbox with the current username
                IsEditingUsername = true;
                ErrorVisible = false;
            });

            CancelEditUsernameCommand = new RelayCommand(_ =>
            {
                IsEditingUsername = false;
                ErrorVisible = false;
            });

            SaveUsernameCommand = new RelayCommand(async _ =>
            {
                var result = MessageBox.Show("Are you sure you want to change your username?",
                                             "Confirm Change", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;

                ErrorVisible = false;

                if (string.IsNullOrWhiteSpace(NewUsername))
                {
                    ErrorMessage = "Please enter a new username.";
                    ErrorVisible = true;
                    return;
                }
                if (NewUsername.Contains(" "))
                {
                    ErrorMessage = "Username cannot contain spaces.";
                    ErrorVisible = true;
                    return;
                }
                if (NewUsername != Username && await CarDataService.UsernameExistsExcept(NewUsername, _userId))
                {
                    ErrorMessage = $"Username '{NewUsername}' is already taken.";
                    ErrorVisible = true;
                    return;
                }

                // Instantly save to Database
                await CarDataService.UpdateCustomerProfile(_userId, NewUsername, ProfilePicturePath);

                // Update session and local properties
                Username = NewUsername;
                UserSession.Username = NewUsername;
                UserLabel = $"Customer: {Username}";

                IsEditingUsername = false;
                MessageBox.Show("Username updated successfully!",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        // FIXED: The new Async method to handle DB loads properly
        private async Task LoadUserDataAsync(string userId)
        {
            try
            {
                string queryId = !string.IsNullOrEmpty(UserSession.UserId) ? UserSession.UserId : userId;
                var customer = await CarDataService.GetCustomerByUsername(UserSession.Username ?? userId);
                var rentals = await CarDataService.GetRentalsByCustomer(queryId);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (customer != null)
                    {
                        Contact = customer.ContactNumber;
                        License = customer.LicenseNumber;

                        // FIXED: Uncommented to restore the profile picture
                        ProfilePicturePath = customer.ProfilePicturePath;

                        // FIXED: Sync UI with database truth just in case the session got out of sync
                        Username = customer.Username;

                        // Keep the static UserSession synced with the real DB value
                        if (UserSession.Username != customer.Username)
                        {
                            UserSession.Username = customer.Username;
                        }

                        UserLabel = $"Customer: {Username}";
                    }

                    Rentals.Clear();
                    foreach (var r in rentals) Rentals.Add(r);
                    HasRentals = Rentals.Count > 0;
                });
            }
            catch (Exception)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = "Failed to load account data from the server.";
                    ErrorVisible = true;
                });
            }
        }
    }
}