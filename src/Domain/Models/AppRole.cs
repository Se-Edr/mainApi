

namespace Domain.Models
{
    public class AppRole
    {
        public Guid RoleId { get; set; }
        public string RoleName { get =>_roleName; 
            set 
            {
                _roleName = value;
                NormalizedRoleName = _roleName.ToUpper();
            } }

        private string _roleName;

        public string NormalizedRoleName { get; private set; }

        public ICollection<UserRole> UsersInThisRole = new List<UserRole>();

    }
}
