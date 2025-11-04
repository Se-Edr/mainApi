

namespace Domain.Models
{
    public class Repair
    {
        public Guid RepairId { get; set; }
        public string? CarSpz { get; set; }
        public bool OilServis { get; set; }
        public string? RepairDesc { get; set; }
        public string? Kilometres { get; set; }
        public DateOnly? DateofRepair { get; set; }
        public int? RepairPrice { get; set; }

        public Guid? RepairForCarId { get; set; }
        public Car? RepairForCar { get; set; }

        public ICollection<ImageRepair>? ImagesForThisRepair { get; set; }=new List<ImageRepair>();
    }
}
