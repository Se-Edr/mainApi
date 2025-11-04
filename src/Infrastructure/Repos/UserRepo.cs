using Domain.Constants;
using Domain.Models;
using Domain.Repos;
using Infrastructure.DataContext;
using Infrastructure.GeneralSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace Infrastructure.Repos
{
    internal class UserRepo(GarageContext _context,RoleManager _roleManager,IOptions<CommonSettings> options):IUserRepo
    {
        CommonSettings _settings = options.Value;

        public async Task<AppUser> AddUser(string email, string userName, string userPhone)
        {
            AppUser newUser = new AppUser()
            {
                UserId = Guid.NewGuid(),
                Email = email.ToLower(),
                Name = userName,
                UserPhone = userPhone
            };
            await _context.UsersTable.AddAsync(newUser);
            // add client Role
            await _context.SaveChangesAsync();
            return newUser;
        }

        public async Task<AppUser> RegisterUser(string email,string hashedPass,string userName,string userPhone)
        {
            AppUser newUser=new AppUser()
            {
                UserId=Guid.NewGuid(),
                Email = email.ToLower(),
                Name = userName,
                UserPhone = userPhone,
                HashedPass=hashedPass
            };

            await _context.UsersTable.AddAsync(newUser);

            // add client Role
            bool res = await _roleManager.AddRoleToUser(AppRoles.Client,newUser);
            
            await _context.SaveChangesAsync();

            return newUser;
        }

        public async Task<string> SetRefreshTokenForUser(string userId,int activeForDays)
        {
            string refreshToken = Guid.NewGuid().ToString();

            if (String.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            string cryptedToken = await CryptToken(refreshToken);
            UserRefreshTokens tokenForUser = new UserRefreshTokens()
            {
                RefreshTokenId = Guid.NewGuid(),
                TokenForUser = userId,
                Token = refreshToken,
                Expires = DateOnly.FromDateTime(DateTime.Now.AddDays(activeForDays))
            };

            await _context.TokensTable.AddAsync(tokenForUser);
            await _context.SaveChangesAsync();

            return cryptedToken;
        }

        public async Task<string> GetUserFromRefreshToken(string TokenBody)
        {
            string token = await DecryptToken(TokenBody);

            UserRefreshTokens tokenForUser =
                await _context.TokensTable.Where(tok=>tok.Token.Equals(TokenBody)).FirstAsync();

            if (tokenForUser == null)
            {
                return null;
            }


            return tokenForUser.TokenForUser;
        }

        public async Task<bool> DeleteRefreshToken(string TokenBody, string userId)
        {
            string token = await DecryptToken(TokenBody);
            if (token == null)
            {
                return false;
            }

            UserRefreshTokens tokenToDel =
                await _context.TokensTable.FirstOrDefaultAsync(tok => tok.Token.Equals(token) && tok.TokenForUser.Equals(userId));
            if (tokenToDel == null)
            {
                return false;
            }
            _context.TokensTable.Remove(tokenToDel);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> CryptToken(string rfshToken)
        {
            //byte[] bytes=System.Text.Encoding.UTF8.GetBytes(rfshToken);
            //string cryptedToken=System.Convert.ToBase64String(bytes);
            //return cryptedToken;
            return rfshToken;
        }
        private async Task<string> DecryptToken(string TokenBody)
        {
            //if (TokenBody == null)
            //{
            //    return null;
            //}
            //byte[] cryptedbytes = System.Convert.FromBase64String(TokenBody);
            //string decryptedToken=System.Text.Encoding.UTF8.GetString(cryptedbytes);
            //return decryptedToken;
            return TokenBody;
        }

        

       

        public async Task<AppUser?> FinduserByEmail(string email)
        {
            AppUser? assumedUser =
               await _context.UsersTable.Include(us => us.RolesOfUser).ThenInclude(rl => rl.Ro)
               .FirstOrDefaultAsync(x => x.Email.ToUpper().Equals(email.ToUpper()));
            return assumedUser;
        }

        public async Task<AppUser?> FindUserById(string userid)
        {
            AppUser? assumedUser =
               await _context.UsersTable.Include(us => us.RolesOfUser).ThenInclude(rl => rl.Ro)
               .FirstOrDefaultAsync(user=>user.UserId.Equals(userid));

            return assumedUser;
        }

        public  async Task<List<string>> GetUsersRoleNames(AppUser user)
        {
            List<string> userRoles = user.RolesOfUser.Select(x => x.Ro.RoleName).ToList();
            return userRoles;
        }

       

        public async Task<string> CreateVerificateUserToken(AppUser user, string token)
        {
            EditUsertToken eut = new EditUsertToken()
            {
                Token = token,
                UserId = user.UserId,
                ExpiryDate = DateTime.UtcNow.AddMinutes(_settings.VerificationTokenLifeTime)
            };

            await _context.EditUserTokenTable.AddAsync(eut);
            await _context.SaveChangesAsync();

            return eut.Token;
        }
        public async Task<string> CreateChangePassUserToken(AppUser user, string token)
        {
            EditUsertToken eut = new EditUsertToken()
            {
                Token = token,
                UserId = user.UserId,
                ExpiryDate = DateTime.UtcNow.AddMinutes(_settings.ResetPasswordTokenLifeTime)
            };

            await _context.EditUserTokenTable.AddAsync(eut);
            await _context.SaveChangesAsync();

            return eut.Token;
        }

        public async Task<bool> DeactivateChangePassToken(AppUser user, string token)
        {
            EditUsertToken tokenObj = 
                await _context.EditUserTokenTable.FirstOrDefaultAsync(tok=>tok.UserId.Equals(user.UserId)&&tok.Token.Equals(token));
            if (tokenObj == null|| tokenObj.ExpiryDate<DateTime.UtcNow)
            {
                return false;
            }
            tokenObj.TokenUsed= true;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> ChangePasswordForUser(string token, string newPass,AppUser user)
        {
            EditUsertToken tokenObj = await _context.EditUserTokenTable.FirstOrDefaultAsync(t=>t.Token.Equals(token));
            if (tokenObj == null || tokenObj.ExpiryDate<DateTime.UtcNow)
            {
                return false;
            }
            user.HashedPass = newPass;
            _context.EditUserTokenTable.Remove(tokenObj);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task ChangePasswordManualy(AppUser user,string newPass)
        {
            user.HashedPass = newPass;
            await _context.SaveChangesAsync();
        }

        public async Task<EditUsertToken> FindEditToken(string token)
        {
            EditUsertToken? assumedToken =
                await _context.EditUserTokenTable.FirstOrDefaultAsync
                (t => t.Token.Equals(token));
            return assumedToken;
        }

        public async Task<bool> ConfirmEmail(AppUser user, EditUsertToken token)
        {
            user.Verificated = true;
            _context.EditUserTokenTable.Remove(token);
            await _context.SaveChangesAsync();   
            
            return true;
        }

       
    }
}
