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
    internal class GargeRepo:IGarageRepo
    {
        private readonly GarageContext _garageCont;
        private readonly PagService _pagSer;

        public GargeRepo(GarageContext context,PagService pagSer)
        {
            _garageCont = context;
            _pagSer = pagSer;
        }

        public async Task<Car> GetCarBySPZ(string carSpz)
        {
            
            Car? car = await _garageCont.CarsTable.FirstOrDefaultAsync(c=>c.CarSpz.ToLower().Equals(carSpz.ToLower()));
            return null;
        }

        public async Task<PagData<Car>> GetCars(CarFiltration filter, int page, string ownerId = null)
        {
            
            int totalPages = 0;
            int totalCars = 0;
            List<Car> cars = new List<Car>();
            PaginationSpecs pagSpecs = new PaginationSpecs();

            if(!String.IsNullOrEmpty(ownerId))
            {
                totalCars=await _garageCont.CarsTable.CarFilter(filter).Where(car=>car.OwnerId.Equals(ownerId)).CountAsync();
                totalPages = totalCars % Pagination.ItemsPerPage > 0 ?(totalCars/Pagination.ItemsPerPage)+1 :totalCars/Pagination.ItemsPerPage;

                cars = await _garageCont.CarsTable.Where(car => car.OwnerId.Equals(ownerId))
                    .AsNoTracking()
                    .CarFilter(filter)
                    .Skip((page-1)*Pagination.ItemsPerPage)
                    .Take(Pagination.ItemsPerPage)
                    .ToListAsync();
            }
            else
            {
                totalCars = await _garageCont.CarsTable.CarFilter(filter).CountAsync();
                totalPages = totalCars % Pagination.ItemsPerPage > 0 ? (totalCars / Pagination.ItemsPerPage) + 1 : totalCars / Pagination.ItemsPerPage;


                cars = await _garageCont.CarsTable
                    .AsNoTracking()
                    .CarFilter(filter)
                    .Skip((page - 1) * Pagination.ItemsPerPage)
                    .Take(Pagination.ItemsPerPage)
                    .ToListAsync();
            }
            pagSpecs = new PaginationSpecs()
            {
                page = page,
                pageSize=Pagination.ItemsPerPage,
                totalPages = totalPages
            };

            return _pagSer.SetPaginationData<Car>(cars,pagSpecs);
        }
        public async Task<Car> RetriveCar(Guid carId,bool forEditing=false,string userId = null)
        {
            Car certainCar;
            if (userId != null)
            {
                certainCar = await GetCarById(carId,forEditing,userid:userId);
                return certainCar;
            }
            else
            {
                certainCar = 
                    //await _garageCont.CarsTable.Include(car=>car.Owner).FirstOrDefaultAsync(car=>car.CarId.Equals(carId));
                     await GetCarById(carId,forEditing);
                return certainCar;
            }
        }

        public async Task<Guid> CreateCar(Car newCar)
        {
            Car? assumedCar = await _garageCont.CarsTable.FirstOrDefaultAsync(
            c => c.CarSpz.Equals(newCar.CarSpz));

            if (assumedCar != null)
            {
                return new Guid();
            }
            await _garageCont.CarsTable.AddAsync(newCar);
            await _garageCont.SaveChangesAsync();
            return newCar.CarId;
        }

        public async Task<bool> DeleteCar(Guid carId)
        {
            Car car=await GetCarById(carId);
            if(car==null)
            {
                return false;
            }
            _garageCont.Remove(car);
            await _garageCont.SaveChangesAsync();
            return true;
        }

        public async Task EditCar(Car certainCar)
        {
            var originalValue = _garageCont.CarsTable.Entry(certainCar).Property(c => c.CarSpz).OriginalValue;

            if (originalValue != certainCar.CarSpz)
            {
                certainCar.CarRepairs = await GetCarWithRepairs(certainCar.CarId);
                foreach (Repair rep in certainCar.CarRepairs)
                {
                    rep.CarSpz=certainCar.CarSpz;
                }
            }
            await _garageCont.SaveChangesAsync();
        }


        public async Task<Car> AddQRCodeForCar(Guid carId,string qrCode)
        {
            Car car=await GetCarById(carId,forEdit:true);

            if(car == null)
            {
                throw new Exception();
            }
            car.CarQRCode = qrCode;
            await _garageCont.SaveChangesAsync();
            return car;
        }
       

       

        private async Task<ICollection<Repair>> GetCarWithRepairs(Guid id)
        {
            Car? car = await _garageCont.CarsTable.Include(car=>car.CarRepairs).FirstOrDefaultAsync(car=>car.CarId.Equals(id));
            if (car == null &&car.CarRepairs.Count>0 &&car.CarRepairs is not null)
            {
                return null;
            }
            return car.CarRepairs;
        }
        private async Task<Car> GetCarById(Guid id,bool forEdit=false,string userid=null)
        {

            IQueryable<Car> requestedCar = _garageCont.CarsTable.AsQueryable();

            if (!forEdit)
            {
                requestedCar = requestedCar.AsNoTracking();
            }

            if (userid!=null)
            {
                requestedCar = 
                    requestedCar.Include(car=>car.Owner)
                    .Where(car => car.CarId.Equals(id)&&car.OwnerId.Equals(userid));
            }
            else
            {
                requestedCar = requestedCar
                    .Include(car => car.Owner)
                    .Where(car => car.CarId.Equals(id));
            }
           
            if (requestedCar==null)
            {
                return null;
            }
            Car car = requestedCar.FirstOrDefault();
            return car;
        }

    }
}
