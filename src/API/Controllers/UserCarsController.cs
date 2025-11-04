using Domain.DTOs.CarDTOs;
using Microsoft.AspNetCore.Mvc;
using Application.DataServices;
using Application.ResultHandler;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace AvantimeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserCarsController : ControllerBase
    {
        private readonly UserCarService userCar;

        public UserCarsController(UserCarService userCar)
        {
            this.userCar = userCar;
        }


        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost]
        public async Task<ActionResult<Guid>> AddCarToUser([FromBody] CarUserRelation carBelongsToUser)
        {
            ResultHandler<bool> result=await userCar.AddCarToUser(carBelongsToUser);
            if(!result.IsSuccess)
            {
                return NotFound(result);
            }
            
            return carBelongsToUser.CarId;
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpDelete]
        public async Task<ActionResult<Guid>> RemoveCarFromUser([FromBody] CarUserRelation removeThisCarFromUser)
        {
            ResultHandler<bool> result = await userCar.RemoveCarFromUser(removeThisCarFromUser);
            if(!result.IsSuccess) 
            {
                return NotFound(result);
            }
            return removeThisCarFromUser.CarId;
        }
    }
}
