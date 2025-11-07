using Domain.Models;
using Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos
{
    internal class RoleManager(GarageContext _context)
    {
        public async Task<bool> AddRoleToUser(string roleToAdd,AppUser affectedUser)
        {
            var userRoles = _context.UserRoleTable.Where(r=>r.UsId.Equals(affectedUser.UserId));
            
            foreach (var r in userRoles)
            {
                if(r.Ro.NormalizedRoleName.Equals(roleToAdd.ToUpper()))
                {
                    throw new Exception(); // userAlready in role, after testing change to return true
                }
            }

            AppRole role =await _context.RolesTable.FirstOrDefaultAsync(r=>r.NormalizedRoleName.Equals(roleToAdd.ToUpper()));
            if (role==null)
            {
                throw new ArgumentNullException("role not Found"); 
            }

            //Guid.Parse();
            UserRole newUserRole= new UserRole() 
            {
                Ro = role,
                RoId=role.RoleId,
                Us=affectedUser,
                UsId=affectedUser.UserId
            };

            await _context.UserRoleTable.AddAsync(newUserRole);
            return true;
        }
    }
}
