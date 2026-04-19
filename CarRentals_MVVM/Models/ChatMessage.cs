namespace CarRentals_MVVM.Models
{
    public class ChatMessage
    {
        public string Text { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public bool IsFromUser { get; set; }
        public string Alignment => IsFromUser ? "Right" : "Left";
    }
}