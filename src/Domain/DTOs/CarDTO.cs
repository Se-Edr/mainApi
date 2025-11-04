
namespace Domain.DTOs;

public class CarDTO
{
    public Guid? CarId { get; set; }
    public string? CarSpz { get; set; }
    public string? CarBrand { get; set; }
    public string? CarOwnerName { get; set; }
    public string? CarOwnerEmail { get; set; } 
    public string? CarOwnerPhone { get; set; }
    public string? DefaultOwnerPhone { get; set; }
    public string? CarVIN { get; set; }
    public string? SomeParts { get; set; }

    public List<RepairDTO>? carRepairs=new List<RepairDTO>();


    public string? CarownerId { get; set; }

    public string? CarQrCode { get; set; }

    //public ICollection<RepairDTO>? CarRepairs { get; set; } = new List<RepairDTO>();
}
