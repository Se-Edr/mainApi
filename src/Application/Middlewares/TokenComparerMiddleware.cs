using Application.ConfigCalsses;
using Domain.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Middlewares
{
    public class TokenComparerMiddleware
    {
        private readonly RequestDelegate next;
        private JWTSettings opts;

        public TokenComparerMiddleware(RequestDelegate next,IOptions<JWTSettings> settings)
        {
            opts = settings.Value;
            this.next = next; 
        }

        public async Task InvokeAsync(HttpContext httpcont)
        {
            
            using (IServiceScope scope = httpcont.RequestServices.CreateScope())
            {
                var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepo>();
                
                
                string accesToken = httpcont.Request.Headers["Authorization"].ToString().Replace("Bearer ", ""); 
                string refreshToken;

                if (httpcont.Request.Cookies.ContainsKey("refreshToken"))
                {
                    refreshToken = httpcont.Request.Cookies["refreshToken"];

                    if (string.IsNullOrEmpty(refreshToken))
                    {
                        httpcont.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;     
                    }
                }
                else 
                {
                    httpcont.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var tokenHandler = new JwtSecurityTokenHandler();

                bool isUpdate = false;
                if (httpcont.Request.Path.StartsWithSegments("/api/Auth/UpdateTokens", StringComparison.OrdinalIgnoreCase))
                {
                  isUpdate = true;
                }

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = isUpdate?false:true,
                    //ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = opts.Issuer,
                    ValidAudience = opts.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Key)),
                    //ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                try
                {
                    var principal = tokenHandler.ValidateToken(accesToken, tokenValidationParameters, out validatedToken);
                    string? userIdFromAccToken = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (userIdFromAccToken == null)
                    {
                        httpcont.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }

                    string userFromRfshToken = await userRepo.GetUserFromRefreshToken(refreshToken);

                    if (userFromRfshToken == null || !userFromRfshToken.Equals(userIdFromAccToken))
                    {
                        httpcont.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }

                    httpcont.Items["userId"] = userFromRfshToken;

                }
                catch (Exception ex)
                {
                    httpcont.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    return;
                }

                await next(httpcont);

            }
        } 
    }
}
