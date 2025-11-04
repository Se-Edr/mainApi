

namespace Domain.DTOs;

public  class RepairDTO
{
    public Guid? RepairId { get; set; }
    public string? CarSpz { get; set; }
    public bool OilServis { get; set; }

    public string? RepairDesc { get; set; }
    
    public string? Kilometres { get; set; }
    public DateOnly? DateofRepair { get; set; }

    public int? RepairPrice { get; set; }

    public Guid? RepairForCarId { get; set; }
    public ICollection<ImageDTO>? ImagesForThisRepair { get; set; }
}

public record ImageDTO(Guid id,string link);
