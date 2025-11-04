using Application.ResultHandler;
using Application.Services;
using Domain.DTOs;
using Domain.Models;
using Domain.Repos;

namespace Application.DataServices
{
    public record CreateUserWithCarDTO(string Name, string UserPhone, string CarSpz, string CarVIN, string CarBrand);

    public class AuthWithKeycIoakService(
        IGarageRepo _garageRepo,
        IUserRepo _userRepo,
        CryptingService _cryptService
        )
    {

        public async Task<ResultHandler<string>> RegisterUserKeycloak
          (
            CreateUserWithCarDTO dto, string userEmail, Guid userId
          )
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return ResultHandler<string>.Fail("empty email");
            }

            AppUser? assumedUser = await _userRepo.FinduserByEmail(userEmail);
            if (assumedUser != null)
            {
                return ResultHandler<string>.Fail("User already registered");
            }
            string ph = new string(dto.UserPhone.Where(st => !char.IsWhiteSpace(st)).ToArray());
            ph = await _cryptService.CryptText(ph);

            assumedUser = await _userRepo.AddUser(userEmail,dto.Name,ph);

            Car? car = await _garageRepo.GetCarBySPZ(dto.CarSpz);
            if (car == null)
            {
                Car newCar = new Car
                {
                    CarBrand = dto.CarBrand,
                    OwnerId=userId,
                    CarId = Guid.NewGuid(),
                    CarOwnerName = dto.Name,
                    DefaultOwnerPhone = ph,
                    CarSpz = dto.CarSpz.ToUpper().Replace(" ", ""),
                    CarVIN = dto.CarVIN.ToUpper().Replace(" ", ""),
                };
                var cas = await _garageRepo.CreateCar(newCar);
                return ResultHandler<string>.Success("car created and added to user");
            }

            return ResultHandler<string>.Success("user created");
        }
    }
}
