using Application.ConfigCalsses;
using Application.DataServices;
using Application.ResultHandler;
using Domain.DTOs.UsersDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace AvantimeApi.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authSer;
        //private readonly ILogger<AuthController> _logger;
        JWTSettings settings;
        ApiSettings apiSettings;

        

        public AuthController(AuthService authSer, IOptions<JWTSettings> options,IOptions<ApiSettings> apiopts/*,ILogger<AuthController> logger*/)
        {
            _authSer = authSer;
            //_logger = logger;
            settings = options.Value;
            apiSettings=apiopts.Value;  
        }
       

        [HttpPost("Register")]
        public async Task<ActionResult> RegisterNewUser([FromBody] RegisterUser userDto)
        {
            if (!userDto.Password.Equals(userDto.PsswordOneMore))
            {
                return BadRequest("Passwords are different");
            }

            ResultHandler<string> result =
            await _authSer.RegisterUser(userDto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Response);

        }

        // login with identity

        [HttpPost("LoginuserAndgetToken")]
        public async Task<ActionResult> LoginUser([FromBody] LoginUserDTO assumedUser)
        {
            ResultHandler<(LoginResponseDTO, string)> result = await _authSer.Loginuser(assumedUser.UserEmail, assumedUser.Password, settings.TimeForCookies);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }
            var cookieOpts = new CookieOptions
            {
                HttpOnly = true,
                //Secure = false,
                //SameSite = SameSiteMode.Lax,
                Secure = true,
                SameSite = SameSiteMode.None,

                Expires = DateTime.Now.AddDays(settings.TimeForCookies)
            };

            string tok = result.Response.Item2;

            Response.Cookies.Append("refreshToken", tok, cookieOpts);

            return Ok(result.Response.Item1);
        }

        [Authorize]
        [HttpGet("UpdateTokens")]
        public async Task<ActionResult> UpdateTokens()
        {
            string refreshToken = HttpContext.Request.Cookies["refreshToken"];
            string userId = HttpContext.Items["userId"] as string;

            ResultHandler<(LoginResponseDTO, string)> result =
                await _authSer.GenerateNewTokens(refreshToken, userId, settings.TimeForCookies);

            if (!result.IsSuccess)
            {
                return BadRequest($"{result.ErrorMessage}");
            }

            var cookieOpts = new CookieOptions
            {
                HttpOnly = true,
                //Secure = false,
                //SameSite = SameSiteMode.Lax,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(settings.TimeForCookies)
            };

            string reftok = result.Response.Item2;
            Response.Cookies.Append("refreshToken", reftok, cookieOpts);

            return Ok(result.Response.Item1);
        }

        [HttpPost("manualVerification")]
        public async Task<ActionResult> ManualVerification([FromBody] LoginUserDTO assumedUser)
        {
            ResultHandler<string> rsult = await _authSer.ManualVerification(assumedUser.UserEmail, assumedUser.Password);

            if (!rsult.IsSuccess)
            {
                return BadRequest(rsult.ErrorMessage);
            }

            return Ok(rsult.Response);
        }

        //confirm account by email
        [HttpGet("Verification")]
        public async Task<ActionResult> SendVerificationToEmail(string email, string token)
        {
            ResultHandler<bool> res = await _authSer.ConfirmEmail(email, token);
            if (!res.IsSuccess)
            {
                return BadRequest(res.Response);
            }
            return Redirect($"{apiSettings.FrontEnd}/Login");
        }

        //reset password with email

        [HttpPost("ChangePass")]
        public async Task<ActionResult> ForgotPassword([FromBody] string email)
        {
            //if email is ok -> send message with token
            var url = await _authSer.ChangePasswordRequest(email);
            if (!url.IsSuccess)
            {
                return BadRequest(url.Response);
            }

            return Ok(url.Response);
            //                                                    || 
            // on message click navigate us to ChangePassContinue \/
        }

        [HttpGet("ChangePassContinue")]
        public async Task<ActionResult> ChangePassword(string email, string token)
        {
            if (String.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid or expired token.");
            }

            // need to deactivate token 
            ResultHandler<bool> result = await _authSer.DeactivateToken(email, token);
            if (!result.IsSuccess)
            {
                return BadRequest("Token time out");
            }

            return Redirect($"{apiSettings.FrontEnd}/resetPassword?email={email}&token={token}");
        }

        [HttpPost("SetNewPass")]
        public async Task<ActionResult> SetNewPass([FromBody] ChangePassDTO changePas)
        {
            ResultHandler<bool> myResult = await _authSer.ChangePass(changePas);

            if (!myResult.IsSuccess)
            {
                return BadRequest(myResult.Response);
            }
            return Ok();
        }

        [Authorize]
        [HttpPatch("EditUser")]
        public async Task<ActionResult> EditUser()
        {

            return null;
        }

        [Authorize]
        [HttpPatch("ChangePasswordManually")]
        public async Task<ActionResult> EditPass([FromBody] ManualChangePassDTO pair)
        {
            string userId = HttpContext.Items["userId"] as string;
            var a = await _authSer.ChangePasswordManualy(userId, pair.OldPass, pair.NewPass);
            if (!a.IsSuccess)
            {
                return BadRequest(a.Response);
            }
            return Ok();
        }


        [HttpPost("LogOut")]
        public async Task<ActionResult> LogoutUser()
        {
            string refreshToken = HttpContext.Request.Cookies["refreshToken"];
            string userId = HttpContext.Items["userId"] as string;
            return Ok();

        }
    }
}
