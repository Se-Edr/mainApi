using Application.ConfigCalsses;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Application.Services
{
    public class JWTTokenService(IOptions<JWTSettings> options)
    {

        public async Task<string> GenerateToken(string userId,string userEmail,bool verificated,IList<string> userRoles)
        {
            JWTSettings settings = options.Value;
            
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
          


            List<Claim> claims=
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, userEmail),
                //somehow add to this token if user confirmed his mail
                new Claim("Verificated", verificated.ToString()),
            ];

            foreach (string role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken tkn = new JwtSecurityToken
                (
                issuer: settings.Issuer,
                audience: settings.Audience,
                claims: claims,
                //expires:DateTime.Now.AddMinutes(int.Parse(settings.Time)),
                expires: DateTime.Now.AddMinutes(settings.Time),
                signingCredentials: creds
                ) ;
            string token=new JwtSecurityTokenHandler().WriteToken(tkn);
            return token;
        }

        
    }
}
