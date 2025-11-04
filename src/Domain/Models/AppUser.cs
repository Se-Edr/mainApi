

namespace Domain.Models
{
    public class AppUser
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }
        public string Name { get; set; }
        public string? HashedPass { get; set; }
        public string? UserPhone { get; set; }
        public bool? Verificated { get; set; } = false;

        public ICollection<Car> CarsOwnedByUser = new List<Car>();

        public ICollection<UserRole> RolesOfUser = new List<UserRole>();

    }
}
