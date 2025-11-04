using Domain.Constants;
using Domain.DTOs.Filtration;
using Domain.DTOs.PaginationResponses;
using Domain.Models;
using Domain.Repos;
using Infrastructure.DataContext;
using Infrastructure.MethodExtensions;
using Infrastructure.PaginationServices;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Repos
{
    internal class RepairRepo : IRepairRepo
    {
        private readonly GarageContext _garageContext;
        private readonly PagService _pagService;

        public RepairRepo(GarageContext garageContext,PagService pagService)
        {
            _garageContext = garageContext;
            _pagService = pagService;
        }

        public async Task<(List<Repair>,int)> GetAllRepairs(RepairFiltration filter, int page,string userId = null,Guid carId=new Guid())
        {
            int totalPages = 0;
            int totalReps = 0;
            List<Repair> repairs=new List<Repair>();

            IQueryable<Repair> repairQuery = _garageContext.RepairsTable.AsQueryable();

            if (carId != Guid.Empty)
            {
                repairQuery = repairQuery.Where(rep => rep.RepairForCarId.Equals(carId));
            }

            if (!string.IsNullOrEmpty(userId))
            {
                repairQuery = repairQuery.Where(rep => rep.RepairForCar.OwnerId.Equals(userId));
            }

            repairQuery = repairQuery.RepairFilter(filter);

            totalReps = await repairQuery.CountAsync();            
            totalPages = (totalReps + Pagination.ItemsPerPage - 1) / Pagination.ItemsPerPage;

            repairs = await repairQuery
                .AsNoTracking()
                .Skip((page - 1) * Pagination.ItemsPerPage)
                .Take(Pagination.ItemsPerPage)
                .ToListAsync();

            PaginationSpecs pagSpecs = new PaginationSpecs()
            {
                page = page,
                pageSize = Pagination.ItemsPerPage,
                totalPages = totalPages
            };


            return (repairs,totalPages);
        }


        public async Task<Repair> RetrieveRepairById(Guid repairId,bool isEditing=false,string userId=null)
        {

            Repair certainRepair = userId==null?await GetRepairById(repairId,isEditing):await GetRepairById(repairId,isEditing,userId);

            if (certainRepair == null)
            {
                return null;
            }
            return certainRepair;
        }
        public async Task<List<Repair>> GetRepairsByCar_InternalUse(Guid carId)
        {
            List<Repair> reps = await _garageContext.RepairsTable
                .Include(x=>x.ImagesForThisRepair)
                .Where(rep=>rep.RepairForCarId.Equals(carId)).ToListAsync();

            return reps;
        }

        public async Task<Repair> CreateRepair(Repair newRepair)
        {
            await _garageContext.RepairsTable.AddAsync(newRepair);
            await _garageContext.SaveChangesAsync();
            return newRepair;
        }

        private async Task<Repair> GetRepairById(Guid repairId,bool isEditing=false,string userId=null)
        {
            Repair? certainRepair;
            IQueryable<Repair> req = _garageContext.RepairsTable.AsQueryable();

            if (!isEditing)
            {
                req = req.AsNoTracking();
            }
            if (!string.IsNullOrEmpty(userId))
            {
                Guid.TryParse(userId,out Guid res);
                req = req.Where(r => r.RepairForCar.OwnerId.Equals(res));
            }
            req = req.AsSplitQuery().Include(rep => rep.ImagesForThisRepair);
            req = req.Where(rep => rep.RepairId.Equals(repairId));

            certainRepair = req.FirstOrDefault();
            return certainRepair;
        }

        public async Task EditRepairById()
        {
            await _garageContext.SaveChangesAsync();
        }

        public async Task<bool> AddPhotosToReapair(Repair repair,List<(string,Guid)> pathsAndIds)
        {
            List<ImageRepair> imgs= new List<ImageRepair>();

            foreach ((string,Guid) link in pathsAndIds)
            {
                imgs.Add(new ImageRepair()
                {
                    ImageId=link.Item2,
                    ImagePath=link.Item1,
                    ForRepairId=repair.RepairId
                });
            }

            
            await _garageContext.AddRangeAsync(imgs);
            await _garageContext.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DeletePhotosFromRepair(List<Guid> ids)
        {
            try
            {
                var imgsToDel = new List<ImageRepair>();
                foreach (Guid id in ids)
                {
                    var img = await _garageContext.ImagesTable.FirstOrDefaultAsync(x => x.ImageId.Equals(id));
                    imgsToDel.Add(img);
                }

                _garageContext.ImagesTable.RemoveRange(imgsToDel);
                await _garageContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public async Task<bool> DeleteRepair(Repair repToDel)
        {
            try
            {
                _garageContext.RepairsTable.Remove(repToDel);
                await _garageContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
