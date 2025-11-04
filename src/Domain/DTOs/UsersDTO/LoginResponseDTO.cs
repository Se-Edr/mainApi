

namespace Domain.DTOs.UsersDTO
{
    public class LoginResponseDTO
    {
        public string accessToken { get; set; }
        public string email { get; set; }
        public int expiresIn { get; set; }
        public bool isAdmin{ get; set; }

    }
}
