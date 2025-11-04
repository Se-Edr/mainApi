using Domain.DTOs;
using Domain.DTOs.CarDTOs;
using Domain.Models;


namespace Domain.Repos
{
    public interface IUserCarsRepo { 
    
        Task<bool> AddCarToUser(CarUserRelation usercar);
        Task<bool> RemoveCarFromUser(CarUserRelation usercar);
    
    
    }
}
