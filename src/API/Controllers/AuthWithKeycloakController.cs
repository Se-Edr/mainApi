using Application.DataServices;
using Application.ResultHandler;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Avantime.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthWithKeycloakController
        (
        AuthWithKeycIoakService _authSer
        ) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> RegisterUserWithCar([FromBody] CreateUserWithCarDTO dto)
        {
            var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(ownerId, out Guid resid);
            var email = User.FindFirst(ClaimTypes.Email)?.Value;


            ResultHandler<string> result =await _authSer.RegisterUserKeycloak(dto,email,resid);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }
            return null;
        }
    }

    
}
