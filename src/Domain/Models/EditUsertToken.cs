namespace Domain.Models
{
    public class EditUsertToken
    {
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool TokenUsed { get; set; } = false;
    }
}
