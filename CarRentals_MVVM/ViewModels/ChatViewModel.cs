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
    public class ChatViewModel : ObservableObject
    {
        private readonly string _myId;
        private readonly string _otherId;
        private readonly string _role;

        public string UserLabel { get; }
        public string ChatTitle { get; }
        public ObservableCollection<ChatMessage> Messages { get; } = new();

        private string _currentMessage = string.Empty;
        public string CurrentMessage
        {
            get => _currentMessage;
            set { _currentMessage = value; OnPropertyChanged(); }
        }

        // ==========================================
        // NEW PROPERTIES FOR THE ADMIN HEADER
        // ==========================================
        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        private string _profilePicturePath;
        public string ProfilePicturePath
        {
            get => _profilePicturePath;
            set { _profilePicturePath = value; OnPropertyChanged(); }
        }

        private string _customerId;
        public string CustomerId
        {
            get => _customerId;
            set { _customerId = value; OnPropertyChanged(); }
        }
        // ==========================================

        public ICommand SendCommand { get; }
        public ICommand BackCommand { get; }

        public ChatViewModel(string myId, string otherId, string role)
        {
            _myId = myId;
            _otherId = otherId;
            _role = role;

            UserLabel = role == "Admin"
                ? $"Agent: {myId}"
                : $"Customer: {UserSession.Username ?? myId}";

            ChatTitle = role == "Admin"
                ? $"Chat with {otherId}"
                : "Support Chat";

            // ==========================================
            // REVISED HEADER LOGIC
            // ==========================================
            // Inside ChatViewModel Constructor
            // Inside ChatViewModel Constructor
            if (role == "Admin")
            {
               
                CustomerId = $"Customer | {_otherId}";
                LoadCustomerDetails(_otherId);
            }
            else
            {      FullName = "Live Agent Support";

                CustomerId = "Online - Replies instantly";
            }
        
            // ==========================================

            // Back navigation
            BackCommand = new RelayCommand(_ =>
            {
                if (role == "Admin")
                    NavigationService.Navigate(new View.AdminChatListWindow(myId));
                else
                    NavigationService.Navigate(new View.CustomerDashboard(myId));
            });

            // Send message — save to DB, show it, then trigger AUTO-REPLY
            SendCommand = new RelayCommand(async _ =>
            {
                if (string.IsNullOrWhiteSpace(CurrentMessage)) return;

                string text = CurrentMessage.Trim();
                CurrentMessage = string.Empty;

                // 1. SAVE & SHOW MESSAGE
                await CarDataService.SaveChatMessage(_myId, _otherId, text);
                Messages.Add(new ChatMessage
                {
                    SenderId = _myId,
                    ReceiverId = _otherId,
                    Text = text,
                    Time = DateTime.Now.ToString("HH:mm"),
                    IsFromUser = true
                });

                // 2. TRIGGER AUTO-REPLY (Only if a Customer is sending the message)
                if (_role == "Customer")
                {
                    await Task.Delay(500);

                    string replyText = GetAutoReply(text.ToLower());

                    await CarDataService.SaveChatMessage(_otherId, _myId, replyText);

                    Messages.Add(new ChatMessage
                    {
                        SenderId = _otherId,
                        ReceiverId = _myId,
                        Text = replyText,
                        Time = DateTime.Now.ToString("HH:mm"),
                        IsFromUser = false
                    });
                }
            });

            // Load existing messages from DB
            LoadMessages();
        }

        // ==========================================
        // NEW METHOD TO FETCH CUSTOMER DATA
        // ==========================================
        private void LoadCustomerDetails(string customerId)
        {
            Task.Run(async () =>
            {
                var customer = await CarDataService.GetCustomerById(customerId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (customer != null)
                    {
                        FullName = customer.FullName;
                        ProfilePicturePath = customer.ProfilePicturePath;
                    }
                    else
                    {
                        FullName = "Unknown Customer";
                    }
                });
            });
        }

        private void LoadMessages()
        {
            Task.Run(async () =>
            {
                var msgs = await CarDataService.GetChatMessages(_myId, _otherId);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Clear();
                    foreach (var m in msgs)
                    {
                        m.IsFromUser = m.SenderId == _myId;
                        Messages.Add(m);
                    }
                });
            });
        }

        //AUTO-REPLY LOGIC PLACED HERE
        private string GetAutoReply(string msg)
        {
            if (msg.Contains("rent") || msg.Contains("book"))
                return "To rent a car, go to Browse Cars from your dashboard and select an available vehicle!";
            if (msg.Contains("return") || msg.Contains("back"))
                return "To return a car, the admin can process your return from the Process Return window.";
            if (msg.Contains("price") || msg.Contains("cost") || msg.Contains("rate"))
                return "Pricing varies per vehicle. Sedans start at $35/hr, SUVs from $55/hr, Vans from $80/hr.";
            if (msg.Contains("cancel"))
                return "To cancel a rental, please contact support directly. Active rentals may have cancellation fees.";
            if (msg.Contains("hello") || msg.Contains("hi") || msg.Contains("hey"))
                return "Hello! How can I help you with your rental today?";
            if (msg.Contains("help"))
                return "I can help with: renting cars, pricing, returns, and account questions. What do you need?";
            if (msg.Contains("password") || msg.Contains("forgot"))
                return "To reset your password, go to the login screen and click 'Forgot Password'.";
            if (msg.Contains("maintenance"))
                return "Cars under maintenance are temporarily unavailable. They will return to Available once serviced.";
            if (msg.Contains("hours") || msg.Contains("duration"))
                return "Rental duration is from 1 to 24 hours. Enter your desired hours on the booking form.";
            if (msg.Contains("color"))
                return "Available colors depend on the specific vehicle. You can choose your preferred color during booking!";
            if (msg.Contains("thank"))
                return "You're welcome! Feel free to ask if you need anything else. 😊";

            return "Thank you for your message! Our team will follow up on complex inquiries. For immediate help, please visit your nearest Rental Rev. branch.";
        }
    }
}