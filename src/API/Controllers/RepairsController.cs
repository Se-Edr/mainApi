using Application.DataServices;
using Application.ResultHandler;
using Domain.DTOs;
using Domain.DTOs.Filtration;
using Domain.DTOs.PaginationResponses;
using Domain.DTOs.PhotosDelete;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AvantimeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairsController(RepairService _repService) : ControllerBase
    {

        [Authorize]
        [HttpGet("GetAllRepairs")]
        public async Task<ActionResult> GetAllRepairs([FromQuery] RepairFiltration filter, int page = 1)
        {
            
            if (User.IsInRole("Admin"))
            {
                PagData<RepairDTO> repairs = await _repService.GetRepairs(filter, page);
                return repairs.MyData.Count != 0 || repairs == null ? Ok(repairs) : Ok(repairs);
            }
            if (User.IsInRole("Admin") || !User.IsInRole("Client"))
                return BadRequest();

            string userId = User.FindFirst(ClaimTypes.NameIdentifier.ToString()).Value;
            PagData<RepairDTO> repairs1 = await _repService.GetRepairs(filter, page, userId);

            return repairs1.MyData.Count != 0 || repairs1 == null ? Ok(repairs1) : Ok(repairs1);
        }

        [Authorize]
        [HttpGet("GetRepairsForCarId")]
        public async Task<ActionResult> GetRepairsForCar([FromQuery] RepairFiltration filter,string carId,int page = 1)
        {

            Guid result;
            Guid.TryParse(carId, out result);
            if (result.Equals(new Guid()))
                return BadRequest();
            PagData<RepairDTO> repairs;
            if (User.IsInRole("Client") && !User.IsInRole("Admin"))
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                repairs = await _repService.GetRepairs(filter, page, userId, result);
            }
            else
                repairs = await _repService.GetRepairs(filter, page, carId: result);
            return Ok(repairs);
        }

        [Authorize]
        [HttpGet("GetCertainRepair")]
        public async Task<ActionResult<RepairDTO>> GetCertainRepair(string id)
        {
            Guid result;
            if (!Guid.TryParse(id, out result))
                return BadRequest("Error while parsing ID");

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrEmpty(id))
                return BadRequest((object)"No provided Id");

            string userId = null;
            if (!User.IsInRole("Admin") && User.IsInRole("Client"))
                userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            ResultHandler<RepairDTO> certainRepair;

            if (userId == null)
                certainRepair = await _repService.GetCertainRepair(result);
            else
                certainRepair = await _repService.GetCertainRepair(result, userId); 

            return certainRepair.IsSuccess ? Ok(certainRepair.Response) : BadRequest(certainRepair.ErrorMessage);
        }

       

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateRepairForCarId")]
        public async Task<ActionResult<RepairDTO>> CreateReapair(string carId,[FromBody] RepairDTO newRepair)
        {
            Guid result;
            if (!Guid.TryParse(carId, out result))
                return BadRequest("Error while parsing ID");
            newRepair.RepairForCarId = result;

            ResultHandler<Guid> reapirId = await _repService.CreateRepairForCar(newRepair);

            return reapirId.IsSuccess ? Ok(reapirId.Response) : BadRequest(reapirId.ErrorMessage);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateReapirForCarIdWithPhotos")]
        [RequestSizeLimit(9_000_000_000_000_000_000)]
        public async Task<ActionResult<RepairDTO>> CreateRepairWithPhotos(string carId, [FromForm] RepairDTO newRepair, [FromForm] List<IFormFile> files)
        {
            Guid result;
            if (!Guid.TryParse(carId, out result))
                return BadRequest("Error while parsing ID");
            newRepair.RepairForCarId = result;

            ResultHandler<Guid> repairId= await _repService.CreateRepairForCar(newRepair);


            ResultHandler<(List<string>, Repair)> repair = await _repService.AddPhotosToRepair(repairId.Response, (IEnumerable<IFormFile>)files);
            return repair.IsSuccess ? Ok(repair.Response.Item1) : BadRequest(repair.ErrorMessage);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("UploadPhotosForRepair")]
        [RequestSizeLimit(9_000_000_000_000_000_000)]
        public async Task<ActionResult> UploadPhotosForRepair(string id, [FromForm] List<IFormFile> files)
        {
            
            Guid result;
            if (!Guid.TryParse(id, out result))
                return BadRequest();

            ResultHandler<(List<string>, Repair)> repair = await _repService.AddPhotosToRepair(result, (IEnumerable<IFormFile>)files);
            return repair.IsSuccess ? Ok(repair.Response.Item1) : BadRequest(repair.ErrorMessage);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("EditRepairById")]
        public async Task<ActionResult> EditExistingRepair(string id, [FromBody] EditRepairWithPhotos editedRepair)
        {
            var d = Guid.TryParse(id, out  Guid resultId);

            var res= await _repService.EditRepair(resultId, editedRepair.repair);

            if (!res.IsSuccess)
            {
                return BadRequest();
            }

            if (editedRepair.photosTodel.Count > 0)
            {
                List<Guid> imgLinks = editedRepair.photosTodel.Select(x => x.id).ToList(); 
                await _repService.DeletePhotosFromRepair(imgLinks);
            }

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeletePhotosFromCetrainRepair")]
        public async Task<ActionResult> DeletePhotosFromRepair(string id, [FromBody] DeleteImgs links)
        {
            await _repService.DeletePhotosFromRepair(links.ImgsToDelete);
            return Ok();
        }

        [Authorize(Roles ="Admin")]
        [HttpDelete("DeleteRepair")]
        public async Task<ActionResult> DeleteRepair(string id)
        {
            Guid.TryParse(id, out Guid gId);
            var res=await _repService.DeleteCertainRepair(gId);
            if (!res.IsSuccess)
            {
                return BadRequest(res.ErrorMessage);
            }
            return Ok();
        }

    }
    public record EditRepairWithPhotos(RepairDTO repair, List<ImageDTO> photosTodel);
}
