namespace Projet.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SenderType { get; set; } = "Client"; // Client / Agent / Bot
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}