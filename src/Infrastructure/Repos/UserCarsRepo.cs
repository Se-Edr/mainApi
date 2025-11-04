using Domain.DTOs.CarDTOs;
using Domain.Models;
using Domain.Repos;
using Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos
{
    internal class UserCarsRepo : IUserCarsRepo
    {
        private readonly GarageContext wholeDb;

        public UserCarsRepo(GarageContext wholeDb)
        {
            this.wholeDb = wholeDb;
        }


        public async Task<bool> AddCarToUser(CarUserRelation carUser)
        {
            AppUser? affectedUser = 
                await wholeDb.UsersTable.Include(user=>user.CarsOwnedByUser)
                .FirstOrDefaultAsync(user=>user.Email.Equals(carUser.UserEmail.ToLower()));

            Car? carToAdd = await wholeDb.CarsTable.FirstOrDefaultAsync(car => car.CarId.Equals(carUser.CarId));

            if (affectedUser==null|| carToAdd==null || affectedUser.CarsOwnedByUser.Any(car=>car.CarId.Equals(carUser.CarId)))
            {
                return false;
            }
            carToAdd.CarOwnerPhone = affectedUser.UserPhone;
            affectedUser.CarsOwnedByUser.Add(carToAdd);
            await wholeDb.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveCarFromUser(CarUserRelation carUser)
        {
            AppUser? affectedUser =
                await wholeDb.UsersTable.Include(user => user.CarsOwnedByUser)
                .FirstOrDefaultAsync(user => user.Email.Equals(carUser.UserEmail.ToLower()));

            Car? carToRemove = await wholeDb.CarsTable.FirstOrDefaultAsync(car => car.CarId.Equals(carUser.CarId));

            if (affectedUser == null || carToRemove == null)
            {
                return false;
            }

            affectedUser.CarsOwnedByUser.Remove(carToRemove);
            carToRemove.Owner=null;
            await wholeDb.SaveChangesAsync();

            return true;
        }


    }
}
