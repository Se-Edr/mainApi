using Domain.DTOs;
using Application.ResultHandler;
using Domain.DTOs.Filtration;
using Domain.DTOs.PaginationResponses;
using Domain.Models;
using Domain.Repos;
using Microsoft.AspNetCore.Http;
using Application.Services;
using Infrastructure.Repos.UOW;
using Domain.Constants;

namespace Application.DataServices
{
    public class RepairService
    {
        private readonly IRepairRepo _repRepo;
        private readonly IGarageRepo _garageRepo;
        private readonly IUnitOfWork _uow;
        private readonly MinioService _minioService;
        

        public RepairService(IRepairRepo repRepo,IUnitOfWork uow,IGarageRepo garageRepo,MinioService minioService)
        {
            _repRepo = repRepo;
            _garageRepo = garageRepo;
            _uow = uow;
            _minioService = minioService;
        }


        public async Task<PagData<RepairDTO>> GetRepairs(RepairFiltration filter, int page, string userId = null, Guid carId = new Guid())
        {

            List<RepairDTO> repairsDTO = new List<RepairDTO>();
            List<Repair> repairs = [];

            PaginationSpecs pagSpecs = new PaginationSpecs()
            {
                page = page,
                pageSize = Pagination.ItemsPerPage
            };

            if (userId==null)
            {
                if(carId == new Guid())
                {
                    var data = await _repRepo.GetAllRepairs(filter, page);
                    repairs = data.Item1;
                    pagSpecs.totalPages = data.Item2;
                }
                else 
                {
                    var data = await _repRepo.GetAllRepairs(filter, page, null, carId);
                    repairs = data.Item1;
                    pagSpecs.totalPages = data.Item2;
                }
                
                //for admin
                //datasAndList =carId==new Guid()?await _repRepo.GetAllRepairs(filter,page): await _repRepo.GetAllRepairs(filter,page,null,carId);
            }
            else
            {
                var data = await _repRepo.GetAllRepairs(filter, page, userId, carId);
                pagSpecs.totalPages = data.Item2;

                //for user
                repairs = data.Item1;
            }


            if (repairs.Count > 0)
            {
                repairsDTO = repairs.Select(repDom => new RepairDTO
                {
                    RepairId = repDom.RepairId,
                    RepairDesc = repDom.RepairDesc,
                    RepairPrice = repDom.RepairPrice,
                    DateofRepair = repDom.DateofRepair == null ? DateOnly.FromDateTime(new DateTime()) : repDom.DateofRepair,
                    OilServis = repDom.OilServis,
                    CarSpz = repDom.CarSpz,
                    RepairForCarId = repDom.RepairForCarId,
                    Kilometres = repDom.Kilometres
                }).ToList();
            }

            PagData<RepairDTO> dataAndDtoList = new PagData<RepairDTO>() {
                MyData=repairsDTO,
                PagSpesc=pagSpecs
            };
            return dataAndDtoList;
        }



        public async Task<ResultHandler<RepairDTO>> GetCertainRepair(Guid id, string userId = null)
        {
            Repair repair;
            if (userId != null)
            {
                repair = await _repRepo.RetrieveRepairById(id,false,userId);
            }
            else
            {
                 repair = await _repRepo.RetrieveRepairById(id,false);
            }

            if (repair == null)
            {
                return ResultHandler<RepairDTO>.Fail("No access");
            }

            //List<string>


            RepairDTO repairDTO = new RepairDTO()
            {
                RepairId = repair.RepairId,
                RepairDesc = repair.RepairDesc,
                RepairPrice = repair.RepairPrice,
                DateofRepair = repair.DateofRepair == null ? DateOnly.FromDateTime(new DateTime()) : repair.DateofRepair,
                OilServis = repair.OilServis,
                CarSpz = repair.CarSpz,
                RepairForCarId = repair.RepairForCarId,
                Kilometres = repair.Kilometres,
                //ImagesForThisRepair = repair.ImagesForThisRepair.Select(x => x.ImagePath).ToList()

                ImagesForThisRepair = await Task.WhenAll(repair.ImagesForThisRepair
                .Select(async img=> new ImageDTO(img.ImageId, await _minioService.GenerateUrl(img.ImageId.ToString()))))


                //await _minioService.GenerateUrl(img.ImageId.ToString()
            };



            return ResultHandler<RepairDTO>.Success(repairDTO);

        }

        public async Task<ResultHandler<Guid>> CreateRepairForCar(RepairDTO newRepairDto)
        {
            Car certainCar;
            if (newRepairDto.RepairForCarId != null)
            {
                certainCar = await _garageRepo.RetriveCar(newRepairDto.RepairForCarId.Value);
            }
            else 
            {
                return ResultHandler<Guid>.Fail("CAr carId was null");
            }

           
            Repair newRepairDomain = new Repair() 
            {
              CarSpz=certainCar.CarSpz,
              OilServis=newRepairDto.OilServis,
              RepairDesc=newRepairDto.RepairDesc,
              Kilometres=newRepairDto.Kilometres,
              DateofRepair=newRepairDto.DateofRepair,
              RepairPrice=newRepairDto.RepairPrice,
              RepairForCarId=newRepairDto.RepairForCarId
            };

            var res=await _repRepo.CreateRepair(newRepairDomain);
            if (res == null)
            {
                return ResultHandler<Guid>.Fail("Error during creating new repair");
            }

            return ResultHandler<Guid>.Success(res.RepairId);
        }

        public async Task<ResultHandler<(List<string>,Repair)>> AddPhotosToRepair(Guid id, IEnumerable<IFormFile> files)
        {
            Repair repair =await _repRepo.RetrieveRepairById(id);
            if (repair == null)
            {
                return ResultHandler<(List<string>, Repair)>.Fail("no such repair...");
            }
           List<(string,Guid)> pathsAndIds=new List<(string, Guid)>();

           List<string> onlyPaths=new List<string>();

           foreach (IFormFile file in files)
            {
                
                (string,Guid) pathAndId = await _minioService.SaveFileToMinio(file);
                pathsAndIds.Add(pathAndId);
                onlyPaths.Add(pathAndId.Item1);
            }

            bool add = await _repRepo.AddPhotosToReapair(repair,pathsAndIds);
            if (!add)
            {
                return ResultHandler<(List<string>, Repair)>.Fail("Some error during saving ...");
            }
            
            return ResultHandler<(List<string>, Repair)>.Success((onlyPaths,null));
        }

        public async Task<ResultHandler<bool>> EditRepair(Guid id,RepairDTO editedData)
        {
            Repair certRepair=await _repRepo.RetrieveRepairById(id,true);

            certRepair.RepairDesc = editedData.RepairDesc != null ? editedData.RepairDesc:certRepair.RepairDesc;
            certRepair.RepairPrice = editedData.RepairPrice!= null ? editedData.RepairPrice:certRepair.RepairPrice;
            certRepair.DateofRepair = editedData.DateofRepair!= null ? editedData.DateofRepair:certRepair.DateofRepair;
            certRepair.Kilometres = editedData.Kilometres!= null ? editedData.Kilometres:certRepair.Kilometres;
            certRepair.OilServis=editedData.OilServis;

            await _repRepo.EditRepairById();

            return ResultHandler<bool>.Success(true);
        }

        public async Task<ResultHandler<bool>> DeletePhotosFromRepair(List<Guid> photosToDel)
        {
          
            await _uow.BeginTransactionAsync();
            try
            {
                if (photosToDel.Count > 0 && photosToDel != null)
                {
                    bool deletedFromMinio = await _minioService.DeleteFilesFromMinio(photosToDel);
                    if (!deletedFromMinio)
                    {
                        await _uow.RollbackAsync();
                        return ResultHandler<bool>.Fail("Error deleting images from MinIO");
                    }
                }
                var res= await _repRepo.DeletePhotosFromRepair(photosToDel);
                if (!res)
                {
                    await _uow.RollbackAsync();
                    return ResultHandler<bool>.Fail("Error deleting images from DB");
                }
                await _uow.CommitAsync();
                return ResultHandler<bool>.Success(true);

            }
            catch
            {

                await _uow.RollbackAsync();
                return ResultHandler<bool>.Fail("some Error during deleting");
            }
        }

        public async Task<ResultHandler<bool>> DeleteCertainRepair(Guid id)
        {
            //alternative approach -+saga
            var repToDel = await _repRepo.RetrieveRepairById(id,true);
            if(repToDel is null)
            {
                return ResultHandler<bool>.Fail("No repair against this id");
            }

            await _uow.BeginTransactionAsync();
            try
            {
                List<Guid> photosIds = repToDel.ImagesForThisRepair.Select(i => i.ImageId).ToList();
                if (photosIds.Count > 0 && photosIds != null)
                {
                    bool deletedFromMinio = await _minioService.DeleteFilesFromMinio(photosIds);
                    if (!deletedFromMinio)
                    {
                        await _uow.RollbackAsync();
                        return ResultHandler<bool>.Fail("Error deleting images from MinIO");
                    }
                }

                bool dbDel = await _repRepo.DeleteRepair(repToDel);
                if (!dbDel)
                {
                    await _uow.RollbackAsync();
                    return ResultHandler<bool>.Fail("Error deleting images from DB");
                }
                await _uow.CommitAsync();
                return ResultHandler<bool>.Success(true);
            }
            catch
            {
                await _uow.RollbackAsync();
                return ResultHandler<bool>.Fail("some Error during deleting");
            }
        }

    }
}
