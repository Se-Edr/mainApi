

namespace Domain.Models
{
    public class UserRefreshTokens
    {
        public Guid RefreshTokenId { get; set; }
        public string TokenForUser { get; set; }
        public string Token { get; set; }

        public DateOnly Expires { get; set; }
    }
}
