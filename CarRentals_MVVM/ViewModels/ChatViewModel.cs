using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        private readonly string _userId;
        private readonly string _role;

        public string UserLabel { get; }
        public ObservableCollection<ChatMessage> Messages { get; } = new();

        private string _currentMessage = string.Empty;
        public string CurrentMessage
        {
            get => _currentMessage;
            set { _currentMessage = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }

        public ICommand BackCommand { get; }

        public ChatViewModel(string userId, string role)
        {

            _userId = userId;
            UserLabel = !string.IsNullOrEmpty(UserSession.Username)
       ? $"Customer: {UserSession.Username}"
       : $"Customer: {userId}";


            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.CustomerDashboard(_userId)));


            _userId = userId;
            _role = role;
            UserLabel = role == "Admin" ? $"Agent: {userId}" : $"Customer: {UserSession.Username ?? userId}";

            // Welcome message from support
            Messages.Add(new ChatMessage
            {
                Text = role == "Admin"
                    ? "Welcome, Admin! How can we assist you today?"
                    : $"Hello {UserSession.FullName ?? userId}! Welcome to Rental Rev. support. How can I help you?",
                Time = DateTime.Now.ToString("HH:mm"),
                IsFromUser = false
            });

            SendCommand = new RelayCommand(_ =>
            {
                if (string.IsNullOrWhiteSpace(CurrentMessage)) return;

                // Add user message
                var userMsg = CurrentMessage.Trim();
                Messages.Add(new ChatMessage
                {
                    Text = userMsg,
                    Time = DateTime.Now.ToString("HH:mm"),
                    IsFromUser = true
                });

                // Auto-reply logic
                string reply = GetAutoReply(userMsg.ToLower());
                Messages.Add(new ChatMessage
                {
                    Text = reply,
                    Time = DateTime.Now.ToString("HH:mm"),
                    IsFromUser = false
                });

                CurrentMessage = string.Empty;
            });
        }

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