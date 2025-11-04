

namespace Domain.Models
{
    public class UserRole
    {
       
        public Guid UsId {  get; set; }
        public AppUser Us { get; set; }
        public Guid RoId { get; set; }
        public AppRole Ro { get; set; } 
    }
}
