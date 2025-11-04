

namespace Domain.Models
{
    public class ImageRepair
    {
        public Guid ImageId { get; set; }

        public string ImagePath { get; set; }

        public Guid ForRepairId { get; set; }
        public Repair ForRepair { get; set; }
    }
}
