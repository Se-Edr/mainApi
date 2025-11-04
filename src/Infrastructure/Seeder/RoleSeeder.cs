using Domain.Constants;
using Domain.Models;
using Infrastructure.DataContext;



namespace Infrastructure.Seeder
{
    internal class RoleSeeder(GarageContext _dbContext) : IRoleSeeder
    {
        public async Task Seed()
        {
            if (await _dbContext.Database.CanConnectAsync())
            {
                if (!_dbContext.RolesTable.Any())
                {
                    await _dbContext.RolesTable.AddRangeAsync(GetRoles());
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        private IEnumerable<AppRole> GetRoles()
        {
            List<AppRole> roles = new List<AppRole>()
            {
                new AppRole()
                {RoleId=Guid.NewGuid(),RoleName=AppRoles.Admin
                }
                ,new AppRole(){
                    RoleId=Guid.NewGuid(),RoleName=AppRoles.Client
                }
            };
            return roles;
        }

    }
}
