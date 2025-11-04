using Domain.DTOs.Filtration;
using Domain.DTOs.PaginationResponses;
using Domain.Models;


namespace Domain.Repos
{
    public interface IRepairRepo
    {
        Task<Repair> CreateRepair(Repair newRepair);
        Task<(List<Repair>,int)> GetAllRepairs(RepairFiltration filter, int page,string userId = null, Guid id = new Guid());
        Task<List<Repair>> GetRepairsByCar_InternalUse(Guid carId);
        Task<Repair> RetrieveRepairById(Guid repairId, bool isEditing = false, string userId=null);
        //Task<PagData<Repair>> RepairsForCertainUser(List<Guid> carsOwnedByUser, RepairFiltration filter, int page);
        //Task<PagData<Repair>> RepairsForCertainUser(string userId, RepairFiltration filter, int page);

        Task<bool> AddPhotosToReapair(Repair repair, List<(string,Guid)> pathsAndIds);
        Task<bool> DeletePhotosFromRepair(List<Guid> ids);
        Task<bool> DeleteRepair(Repair repToDel);
        Task EditRepairById();
    }
}
