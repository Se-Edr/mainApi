using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataContext
{
    public class GarageContext:DbContext
    {
        public GarageContext(DbContextOptions<GarageContext> opts):base(opts)
        {
        }

        public DbSet<Car> CarsTable { get; set; }
        public DbSet<Repair> RepairsTable { get; set; }

        public DbSet<ImageRepair> ImagesTable { get; set; }

        public DbSet<AppUser> UsersTable { get; set; }
        public DbSet<AppRole> RolesTable { get; set; }

        public DbSet<UserRole> UserRoleTable { get; set; }

        public DbSet<EditUsertToken> EditUserTokenTable { get; set; }
        public DbSet<UserRefreshTokens> TokensTable { get; set; }
        //TokensTable

        




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRefreshTokens>().HasKey(ut=>ut.RefreshTokenId);
            

            modelBuilder.Entity<EditUsertToken>().HasKey(eut=>eut.Token);

            modelBuilder.Entity<AppUser>()
                 .HasMany(user => user.CarsOwnedByUser)
                 .WithOne(car=>car.Owner)
                 .HasForeignKey(car=>car.OwnerId)
                 .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<AppUser>().HasKey(apu => apu.UserId);
            modelBuilder.Entity<AppRole>().HasKey(apr => apr.RoleId);

            //satrt
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UsId, ur.RoId });


            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Ro)
                .WithMany(role => role.UsersInThisRole)
                .HasForeignKey(ur=>ur.RoId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Us)
                .WithMany(user => user.RolesOfUser)
                .HasForeignKey(ur=>ur.UsId);
            // end


            modelBuilder.Entity<Car>(ent =>
            {
                ent.ToTable("CarsTable");
                ent.HasKey(c => c.CarId);
                ent.Property(c => c.CarSpz).HasColumnName("CarSpz");
                ent.Property(c => c.CarBrand).HasColumnName("CarBrand");
                ent.Property(c => c.CarOwnerName).HasColumnName("CarOwnerName");
                ent.Property(c => c.CarOwnerPhone).HasColumnName("CarOwnerPhone");
                ent.Property(c => c.CarVIN).HasColumnName("CarVIN");
            });

            modelBuilder.Entity<Repair>(ent =>
            {
                ent.ToTable("RepairsTable");
                ent.HasKey(r => r.RepairId);
                ent.Property(r => r.CarSpz).HasColumnName("CarSpz");
                ent.Property(r => r.OilServis).HasColumnName("OilServis");
                ent.Property(r => r.RepairDesc).HasColumnName("RepairDesc");
                ent.Property(r => r.Kilometres).HasColumnName("Kilometres");
                ent.Property(r => r.DateofRepair).HasColumnName("DateofRepair");
                ent.Property(r => r.RepairPrice).HasColumnName("RepairPrice");
                ent.Property(r => r.RepairForCarId).HasColumnName("RepairForCarId");

            });

            modelBuilder.Entity<Repair>()
                .HasOne(r=>r.RepairForCar)
                .WithMany(c=>c.CarRepairs)
                .HasForeignKey(r => r.RepairForCarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ImageRepair>().HasKey(img => img.ImageId);

            modelBuilder.Entity<Repair>()
                .HasMany(r=>r.ImagesForThisRepair)
                .WithOne(img=>img.ForRepair)
                .HasForeignKey(img=>img.ForRepairId)
                .OnDelete(DeleteBehavior.Cascade);

            


            base.OnModelCreating(modelBuilder);
        }


    }
}
