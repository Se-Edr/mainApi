using Application.DataServices;
using Application.ResultHandler;
using Domain.Constants;
using Domain.DTOs;
using Domain.DTOs.Filtration;
using Domain.DTOs.PaginationResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AvantimeApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        
        private readonly GarageService _garSer;
        

        public CarsController(GarageService garSer)
        {
            _garSer = garSer;
        }


        [Authorize]
        [HttpGet("AllCars")]
        public async Task<ActionResult> GetAllCars([FromQuery] CarFiltration filter,int page = 1)
        {
            string ownerId = null;
            ResultHandler<PagData<CarDTO>> result;
            if (HttpContext.User.IsInRole(AppRoles.Admin))
            {
                ownerId = null;
            }
            if (HttpContext.User.IsInRole(AppRoles.Client)&&!User.IsInRole(AppRoles.Admin))
            {
                ownerId=User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
           
            if (!string.IsNullOrEmpty(ownerId))
            {
               result = await _garSer.GetAllCars(filter,page,ownerId);
            }
            else
            {
                result=await _garSer.GetAllCars(filter, page);
            }
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result.Response);
        }


        [Authorize]
        [HttpGet("carById/{carId}")]
        public async Task<ActionResult> GetCarByid(string carId)
        {
            Guid.TryParse(carId, out Guid parsed);

            if (parsed.Equals(new Guid()))
            {
                return BadRequest();
            }

            if (HttpContext.User.IsInRole(AppRoles.Admin))
            {
                ResultHandler<CarDTO> result = await _garSer.GetCarById(parsed);
                if (!result.IsSuccess)
                {
                    return NotFound();
                }
                return Ok(result.Response);
            }
            else
            {
                string? userId = HttpContext.Items["userId"] as string;
                ResultHandler<CarDTO> result = await _garSer.GetCarById(parsed, userId);
                if (!result.IsSuccess)
                {
                    return NotFound();
                }
                return Ok(result.Response);
            }
        }


        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost("CreateCar")]
        public async Task<ActionResult<Guid>> CareateCar([FromBody] CarDTO newCar)
        {
            ResultHandler<Guid> result;
            result = await _garSer.CreateCar(newCar);
            if(!result.IsSuccess)
            {
                var errs = result.ErrorResponse as Dictionary<string, string>;
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Validation fails",
                    Detail = "one or more erroes of validation occured"
                };
                problemDetails.Extensions["message"] = "Car with this SPZ already exists";
                if (errs != null)
                {
                    problemDetails.Extensions["errors"] = errs;
                }
                return UnprocessableEntity(problemDetails);
            }
            return Ok(result.Response);
        }


        [Authorize(Roles = AppRoles.Admin)]
        [HttpDelete("DeleteCar")]
        public async Task<ActionResult<bool>> DeleteCar(string carId)
        {
            Guid.TryParse(carId, out Guid parsed);

            if (parsed.Equals(new Guid()))
            {
                return BadRequest();
            }

            ResultHandler<bool> result = await _garSer.DeleteCar(parsed);

            if (!result.IsSuccess)
            {
                return NotFound();
            }

            return Ok(result.Response);
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPatch("EditCar")]
        public async Task<ActionResult<bool>> EditCar(string id,[FromBody] CarDTO editedCar)
        {
            if(!Guid.TryParse(id,out Guid res)){
                return BadRequest();
            }
            var operationResult = await _garSer.EditCar(res, editedCar);
            if (!operationResult.IsSuccess)
            {
                var errs = operationResult.ErrorResponse as Dictionary<string, string>;
                var problemDetails = new ProblemDetails
                {
                    Status=StatusCodes.Status422UnprocessableEntity,
                    Title="Validation fails",
                    Detail="one or more erroes of validation occures"
                };
                if (errs != null)
                {
                    problemDetails.Extensions["errors"] = errs;
                }

                return UnprocessableEntity(problemDetails);
            }
            return Ok();
        }



        [Authorize(Roles = AppRoles.Admin)]
        [HttpPatch("AddQrCodeForCar")]
        public async Task<ActionResult<CarDTO>> AddQrCode(string carId)
        {
            Guid.TryParse(carId, out Guid parsed);

            if (parsed.Equals(new Guid()))
            {
                return BadRequest();
            }

            ResultHandler<CarDTO> result=await _garSer.AddQrCodeToCar(parsed);
            if(!result.IsSuccess)
            {
                return NotFound();
            }

            return Ok(result.Response);
        }
    }
}
