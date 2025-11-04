
using Domain.Models;

namespace Domain.Repos
{
    public interface IUserRepo
    {
        Task<AppUser> AddUser(string email, string userName, string userPhone);
        Task<AppUser> RegisterUser(string email, string pass, string UserName, string userPhone);
        
        Task<AppUser?> FinduserByEmail(string email);
        Task<AppUser?> FindUserById(string userid);
        Task<string> SetRefreshTokenForUser(string userId, int activeForDays);
        Task<string> GetUserFromRefreshToken(string TokenBody);
        Task<bool> DeleteRefreshToken(string TokenBody, string userId);
        Task<List<string>> GetUsersRoleNames(AppUser user);
      
        Task<string> CreateVerificateUserToken(AppUser user, string token);
        Task<string> CreateChangePassUserToken(AppUser user, string token);
        Task<bool> DeactivateChangePassToken(AppUser user, string token);
        Task<bool> ConfirmEmail(AppUser user, EditUsertToken token);
        Task<EditUsertToken> FindEditToken(string token);

        Task<bool> ChangePasswordForUser(string token, string newPass, AppUser user);
        Task ChangePasswordManualy(AppUser user, string newPass);

    }
}
