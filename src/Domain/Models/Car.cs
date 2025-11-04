

namespace Domain.Models
{
    public class Car
    {
        public Guid CarId { get; set; }
        public string? CarSpz { get; set; }
        public string? CarBrand { get; set; }
        public string? CarOwnerName { get; set; }
        public string? CarOwnerPhone { get; set; }
        public string? DefaultOwnerPhone { get; set;}
        public string? CarVIN { get; set; }
        public string? SomeParts { get; set;}
        public ICollection<Repair>? CarRepairs { get; set; } = new List<Repair>();
        public Guid? OwnerId { get; set; }
        public AppUser? Owner { get; set; }
        public string? CarQRCode { get; set; }
    }
}
