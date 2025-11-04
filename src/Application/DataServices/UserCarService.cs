using Application.ResultHandler;
using Domain.DTOs.CarDTOs;
using Domain.Repos;


namespace Application.DataServices
{
    public class UserCarService
    {
        private readonly IUserCarsRepo userCarRepo;

        public UserCarService(IUserCarsRepo userCarRepo)
        {
            this.userCarRepo = userCarRepo;
        }

        public async Task<ResultHandler<bool>> AddCarToUser(CarUserRelation carbelongsTouser)
        {
            carbelongsTouser = new CarUserRelation()
            {
                CarId= carbelongsTouser.CarId,
                UserEmail=new string(carbelongsTouser.UserEmail.Where(st=>!char.IsWhiteSpace(st)).ToArray())
            };
            bool added = await userCarRepo.AddCarToUser(carbelongsTouser);

            if(!added) 
            {
               return ResultHandler<bool>.Fail("Some problem");
            }


            return ResultHandler<bool>.Success(added);
        }

        public async Task<ResultHandler<bool>> RemoveCarFromUser(CarUserRelation removeThisCarFromUser)
        {
            removeThisCarFromUser = new CarUserRelation()
            {
                CarId = removeThisCarFromUser.CarId,
                UserEmail = new string(removeThisCarFromUser.UserEmail.Where(st => !char.IsWhiteSpace(st)).ToArray()),
            };

            bool removed = await userCarRepo.RemoveCarFromUser(removeThisCarFromUser);

            if(!removed)
            {
                return ResultHandler<bool>.Fail("Some problem");
            }

            return ResultHandler<bool>.Success(removed);
        }
    }
}
