
using Domain.DTOs;
using Domain.DTOs.Filtration;
using Domain.DTOs.PaginationResponses;
using Domain.Models;

namespace Domain.Repos
{
    public interface IGarageRepo
    {
        Task<PagData<Car>> GetCars(CarFiltration filterint,int page, string ownerId = null);
        Task<Car> RetriveCar(Guid carId, bool forEditing = false, string userId = null);
        Task<Car> GetCarBySPZ(string carSpz);
        
        Task<Guid> CreateCar(Car newCar);
        Task EditCar(Car someCar);
        Task<bool> DeleteCar(Guid carId);
        Task<Car> AddQRCodeForCar(Guid carId, string qrCode);
    }
}
