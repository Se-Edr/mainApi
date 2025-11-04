using Application.ConfigCalsses;
using Application.ResultHandler;
using Application.Services;
using Domain.Constants;
using Domain.DTOs.UsersDTO;
using Domain.Models;
using Domain.Repos;
using FluentValidation;
using Microsoft.Extensions.Options;



namespace Application.DataServices
{
   

    public class AuthService(
        IUserRepo _userRepo
        ,JWTTokenService _jwtService
        ,EmailSender _emailSender
        ,IOptions<JWTSettings> options
        ,CryptingService _cryptSer
        ,PasswordService _passwordService,
        IValidator<RegisterUser> _validator)
    {
        JWTSettings settings = options.Value;


      


        public async Task<ResultHandler<string>> RegisterUser(RegisterUser userDTO)
        {
            //try to find user
            AppUser? assumedUser = await _userRepo.FinduserByEmail(userDTO.UserEmail);
            if (assumedUser != null) 
            {
                return ResultHandler<string>.Fail("User already registered");
            }

            FluentValidation.Results.ValidationResult res = await _validator.ValidateAsync(userDTO);

            if (!res.IsValid)
            {
                var errors = res.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage);
                return ResultHandler<string>.Fail("Validation errors", errors);
            }
            string userPhone = userDTO.UserPhone;

            userPhone = new string(userPhone.Where(st => !char.IsWhiteSpace(st)).ToArray());
            userPhone = await _cryptSer.CryptText(userPhone);
            string hashedPassword=_passwordService.HashPassword(userDTO.Password);

            assumedUser = await _userRepo.RegisterUser(userDTO.UserEmail, hashedPassword, userDTO.UserName,userPhone);

            //here generate token and save it, after return TokenId
            Guid token=Guid.NewGuid();
            string tokenRes = await _userRepo.CreateVerificateUserToken(assumedUser,token.ToString());
            // send this token to Email sender

            string url = await _emailSender.SendTokenUrl(assumedUser.Email,tokenRes, "Verification",false);
            //_emailSender.

            //after go to specific url with token, check if it fits date, user etc and login again
            return ResultHandler<string>.Success($"registarion succeeded, please confirm your email {assumedUser.Email}");
        }

        public async Task<ResultHandler<(LoginResponseDTO,string)>> Loginuser(string email, string pass, int activeForDays)
        {
            AppUser? user=await _userRepo.FinduserByEmail(email);
            if(user == null)
            {
                return ResultHandler<(LoginResponseDTO, string)>.Fail("User not registered");
            }
            bool d = _passwordService.VerifyPassword(pass, user.HashedPass);

            //bool correctPass = await _userRepo.VerifyPassword(user, hashedPass);

            if (!d)
            {
                return ResultHandler<(LoginResponseDTO, string)>.Fail("wrong pass");
            }
            if (!user.Verificated.Value )
            {
                return ResultHandler<(LoginResponseDTO, string)>.Fail("User was not vefificated");
            }

            List<string> roles = await _userRepo.GetUsersRoleNames(user);
            string Token = await _jwtService.GenerateToken(user.UserId.ToString(), user.Email, user.Verificated.Value, roles);

            string addToken = await _userRepo.SetRefreshTokenForUser(user.UserId.ToString(), activeForDays);
            if(String.IsNullOrEmpty(addToken))
            {
                return ResultHandler<(LoginResponseDTO, string)>.Fail("error while instantiating refresh token");
            }

            LoginResponseDTO response = new LoginResponseDTO() {
            accessToken=Token,
            email=email,
            expiresIn=settings.Time,
            isAdmin=roles.Contains(AppRoles.Admin)?true:false,
            };

            return ResultHandler<(LoginResponseDTO, string)>.Success((response,addToken));
        }

        public async Task<ResultHandler<string>> ManualVerification(string email, string pass)
        {
            AppUser? user = await _userRepo.FinduserByEmail(email);
            if (user == null)
            {
                return ResultHandler<string>.Fail("User not registered");
            }
            bool correctPass = _passwordService.VerifyPassword(pass,user.HashedPass);
            //bool correctPass = await _userRepo.VerifyPassword(user, pass);

            if (!correctPass)
            {
                return ResultHandler<string>.Fail("wrong pass");
            }
            if (user.Verificated.Value)
            {
                return ResultHandler<string>.Fail("user already virificated");
            }

            Guid token = Guid.NewGuid();
            string tokenRes = await _userRepo.CreateVerificateUserToken(user, token.ToString());
            // send this token to Email sender

            string url = await _emailSender.SendTokenUrl(user.Email, tokenRes, "Verification", false);

            return ResultHandler<string>.Success($"Please check your email {user.Email} for verification message");
        }


        public async Task<ResultHandler<(LoginResponseDTO, string)>> GenerateNewTokens(string refreshToken,string userId, int activeForDays)
        {
            if(String.IsNullOrEmpty(refreshToken)|| String.IsNullOrEmpty(userId))
            {
                string errMess = refreshToken == null ? userId==null? "No data at all":"empty refresh token":"empty user";
                return ResultHandler<(LoginResponseDTO, string)>.Fail(errMess);
            }
            
            bool deletedToken = await _userRepo.DeleteRefreshToken(refreshToken,userId);
            if(!deletedToken)
            {
                return ResultHandler<(LoginResponseDTO, string)>.Fail("no token found");
            }
            AppUser user= await _userRepo.FindUserById(userId);
            if(user == null)
            {
                return ResultHandler<(LoginResponseDTO, string)>.Fail("Error while fetching user");
            }

            List<string> roles = await _userRepo.GetUsersRoleNames(user);
            string newAccessToken = await _jwtService.GenerateToken(userId, user.Email, user.Verificated.Value, roles);
            string newRefreshToken=await _userRepo.SetRefreshTokenForUser(user.UserId.ToString(),activeForDays);

            LoginResponseDTO responseDTO = new LoginResponseDTO()
            {
                accessToken = newAccessToken,
                email = user.Email,
                expiresIn = 1,
                isAdmin = roles.Contains(AppRoles.Admin) ? true : false
            };

            return ResultHandler<(LoginResponseDTO, string)>.Success((responseDTO,newRefreshToken));
        }


        //return url

        public async Task<ResultHandler<bool>> ConfirmEmail(string email, string tokenId)
        {
            AppUser? user = await _userRepo.FinduserByEmail(email);
            if(user == null)
            {
                return ResultHandler<bool>.Fail("User not Found");
            }
            EditUsertToken tok = await _userRepo.FindEditToken(tokenId);
            if(tok == null)
            {
                return ResultHandler<bool>.Fail("No token");
            }
            if (!user.UserId.Equals(tok.UserId))
            {
                return ResultHandler<bool>.Fail("Error with token");
            }
            if (DateTime.UtcNow > tok.ExpiryDate)
            {
                return ResultHandler<bool>.Fail("Outdated token");
            }

            bool updateuserVerification = await _userRepo.ConfirmEmail(user,tok);
            return ResultHandler<bool>.Success(updateuserVerification);
        }

        public async Task<ResultHandler<bool>> ChangePasswordManualy(string userId,string oldPassword, string newPassword)
        {
            AppUser assumedUser= await _userRepo.FindUserById(userId);
            if (assumedUser == null)
            {
                return ResultHandler<bool>.Fail("User not Found");
            }
            oldPassword=_passwordService.HashPassword(oldPassword);
            bool correctPass = _passwordService.VerifyPassword(oldPassword, assumedUser.HashedPass);
            //bool passes = await _userRepo.VerifyPassword(assumedUser,oldPassword);
            if (!correctPass)
            {
                return ResultHandler<bool>.Fail("password is wrong");
            }

            newPassword=_passwordService.HashPassword(newPassword);
            await _userRepo.ChangePasswordManualy(assumedUser,newPassword);
            return ResultHandler<bool>.Success(true);
        }

        public async Task<ResultHandler<string>> ChangePasswordRequest(string email)
        {
            AppUser? user=await _userRepo.FinduserByEmail(email);
            if (user == null)
            {
                // negative result
                return ResultHandler<string>.Fail("no such user, register please");
            }

            Guid tokenId=Guid.NewGuid();
            string tokenRes = await _userRepo.CreateChangePassUserToken(user!,tokenId.ToString());
            string body = await _emailSender.SendTokenUrl(user.Email,tokenRes, "ChangePassContinue",true);
            return ResultHandler<string>.Success(body);
        }
        public async Task<ResultHandler<bool>> DeactivateToken(string email, string token)
        {
            AppUser? user = await _userRepo.FinduserByEmail(email);
            if (user == null)
            {
                return ResultHandler<bool>.Fail("Empty user");
            }
            bool isDeactivated = await _userRepo.DeactivateChangePassToken(user, token);
            if (!isDeactivated)
            {
                return ResultHandler<bool>.Fail("token is not valid");
            }
            return ResultHandler<bool>.Success(true);;
        }

        public async Task<ResultHandler<bool>> ChangePass(ChangePassDTO newPassForUser)
        {
            AppUser? user = await _userRepo.FinduserByEmail(newPassForUser.Email);
            if (user == null)
            {
                return ResultHandler<bool>.Fail("No user founded");
            }
            string hashedPass=_passwordService.HashPassword(newPassForUser.Password);

            var res = 
                await _userRepo.ChangePasswordForUser(newPassForUser.ResetToken,hashedPass,user);


            return ResultHandler<bool>.Success(res);
        }
       
    }
}
