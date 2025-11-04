
namespace Application.Services
{
    public class PasswordService
    {
        public string HashPassword(string pass)
        {
            return BCrypt.Net.BCrypt.HashPassword(pass);
        }
        public bool VerifyPassword(string pass, string hashedPass)
        {
            return BCrypt.Net.BCrypt.Verify(pass,hashedPass);
        }
    }
}
