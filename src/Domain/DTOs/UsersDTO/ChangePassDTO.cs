

namespace Domain.DTOs.UsersDTO
{
    public class ChangePassDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ResetToken { get; set; }  
    }
}
