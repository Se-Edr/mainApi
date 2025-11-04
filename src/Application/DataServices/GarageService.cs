using Application.ResultHandler;
using Application.Services;
using Domain.DTOs;
using Domain.DTOs.Filtration;
using Domain.DTOs.PaginationResponses;
using Domain.Models;
using Domain.Repos;
using FluentValidation;
using Infrastructure.Repos.UOW;
using Microsoft.Extensions.Primitives;

namespace Application.DataServices;

public  class GarageService
{
    private readonly IGarageRepo _garageRepo;
    private readonly IValidator<CarDTO> _validator;
    private readonly QRCodeService _qrCodeService;
    private readonly CryptingService _cryptService;
    private readonly IUnitOfWork _uow;
    private readonly IRepairRepo _repairRepo;
    private readonly MinioService _minioService;
    

    public GarageService(
        IGarageRepo garageRepo,
        IValidator<CarDTO> validator,
        QRCodeService qrCodeService,
        CryptingService crypt,
        IUnitOfWork unitOfWork,
        IRepairRepo repairRepo,
        MinioService minioService)
    {
        _garageRepo = garageRepo;
        _validator = validator;
        _qrCodeService = qrCodeService;
        _cryptService = crypt;
        _uow = unitOfWork;
        _repairRepo = repairRepo;
        _minioService = minioService;
    }

    public async Task<ResultHandler<PagData<CarDTO>>> GetAllCars(CarFiltration filter, int page,string ownerId=null)
    {

        
        List<CarDTO> carDTOs = new List<CarDTO>();
        PagData<Car> pagData = new PagData<Car>();

        if(!String.IsNullOrEmpty(ownerId))
        {
             pagData = await _garageRepo.GetCars(filter,page,ownerId);
        }
        else
        {
            pagData = await _garageRepo.GetCars(filter,page);
        }

        if(pagData.MyData==null) 
        {
            return ResultHandler<PagData<CarDTO>>.Fail("Empty List of Cars"); 
        }

        

        carDTOs = pagData.MyData.Select(carD => new CarDTO
        {
            CarId = carD.CarId,
            CarBrand = carD.CarBrand,
            CarOwnerName = carD.CarOwnerName,
            //CarOwnerPhone = returnPhone(carD),
            CarOwnerPhone = carD.CarOwnerPhone is null?
            carD.DefaultOwnerPhone is null?"No Phone Provided": _cryptService.DecryptText(carD.DefaultOwnerPhone) : _cryptService.DecryptText(carD.CarOwnerPhone),
            CarSpz=carD.CarSpz,
            CarVIN = carD.CarVIN
        }).ToList();

        PagData<CarDTO> pagDtatDTO= new PagData<CarDTO>()
        {
            MyData = carDTOs,
            PagSpesc=pagData.PagSpesc
        };

        string returnPhone(Car car)
        {
            string defCarNum = car.DefaultOwnerPhone == null ? null : _cryptService.DecryptText(car.DefaultOwnerPhone);
            string usersPhone = car.CarOwnerPhone == null ? null : _cryptService.DecryptText(car.CarOwnerPhone);

            if (defCarNum != usersPhone)
            {
                return "see details";
            }
            return defCarNum==null ? usersPhone==null?"no phone provided":usersPhone:defCarNum;
        }

        return ResultHandler<PagData<CarDTO>>.Success(pagDtatDTO);
    }

    public async Task<ResultHandler<Guid>> CreateCar(CarDTO newCarDTO)
    {
        FluentValidation.Results.ValidationResult res=await _validator.ValidateAsync(newCarDTO);

        if(!res.IsValid)
        {
            var errors = res.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage);
            return ResultHandler<Guid>.Fail("Validation errors", errors);
        }
        //change to default ownerphone
        string ph= new string(newCarDTO.CarOwnerPhone.Where(st => !char.IsWhiteSpace(st)).ToArray());
        ph = await _cryptService.CryptText(ph);

        Car newCar= new Car()
        {
            CarBrand=newCarDTO.CarBrand,
            CarId=Guid.NewGuid(),
            CarOwnerName=newCarDTO.CarOwnerName,
            DefaultOwnerPhone=ph,
            CarSpz=newCarDTO.CarSpz.ToUpper().Replace(" ", ""),
            CarVIN=newCarDTO.CarVIN.ToUpper().Replace(" ", ""),
        };
        //need to return guid
        Guid newCarId=await _garageRepo.CreateCar(newCar);
        if (newCarId.Equals(new Guid()))
        {
            return ResultHandler<Guid>.Fail($"Error during saving");
        }
        //new code
        await AddQrCodeToCar(newCarId);
        return ResultHandler<Guid>.Success(newCarId);
    }

    public async Task<ResultHandler<bool>> DeleteCar(Guid carId)
    {
        List<Repair> repairs = await _repairRepo.GetRepairsByCar_InternalUse(carId);

        List<Guid> imgToDel = repairs
            .SelectMany(x => x.ImagesForThisRepair)
            .Select(i => i.ImageId)
            .ToList();


        await _uow.BeginTransactionAsync();
        try
        {
            if(imgToDel.Count>0&&imgToDel.Count != null)
            {
                var res = await _minioService.DeleteFilesFromMinio(imgToDel);
                if (!res)
                {
                    await _uow.RollbackAsync();
                    return ResultHandler<bool>.Fail("Error deleting images from MinIO");
                }
            }
            var res2 = await _garageRepo.DeleteCar(carId);
            if (!res2)
            {
                await _uow.RollbackAsync();
                return ResultHandler<bool>.Fail("Error while delete car");
            }
            await _uow.CommitAsync();
            return ResultHandler<bool>.Success(true);

        }
        catch
        {
            await _uow.RollbackAsync();
            return ResultHandler<bool>.Fail("some error in GarageService");
        }
    }

    public async Task<ResultHandler<bool>> EditCar(Guid Id, CarDTO newData)
    {

        FluentValidation.Results.ValidationResult res = await _validator.ValidateAsync(newData);

        if (!res.IsValid)
        {
            var errors = res.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage);
            return ResultHandler<bool>.Fail("Validation errors occurred.", errors);
        } 

        Car certainCar = await _garageRepo.RetriveCar(Id,true);
        if (certainCar == null)
        {
            ResultHandler<bool>.Fail("Car not found");
        }

        

        certainCar.CarBrand = newData.CarBrand == null ?certainCar.CarBrand:newData.CarBrand;
        certainCar.CarSpz = newData.CarSpz== null ?certainCar.CarSpz:newData.CarSpz;
        certainCar.CarVIN = newData.CarVIN == null ?certainCar.CarVIN:newData.CarVIN;
        certainCar.SomeParts = newData.SomeParts == null ? certainCar.SomeParts : newData.SomeParts;

        //string phoneCrypted = await _cryptService.CryptText(newData.DefaultOwnerPhone);
        certainCar.DefaultOwnerPhone = newData.DefaultOwnerPhone == null ? certainCar.DefaultOwnerPhone: await _cryptService.CryptText(newData.DefaultOwnerPhone);

        await _garageRepo.EditCar(certainCar);
        return ResultHandler<bool>.Success(true);
    }
    public async Task<ResultHandler<CarDTO>> GetCarById(Guid carId, string userId = null)
    {

        Car certainCar=userId==null?await _garageRepo.RetriveCar(carId):
            await _garageRepo.RetriveCar(carId,userId:userId);

        if (certainCar == null)
        {
            return ResultHandler<CarDTO>.Fail("Car not found");
        }


        CarDTO carToReturn = new CarDTO()
        {
            CarId = certainCar.CarId,
            CarSpz = certainCar.CarSpz,
            CarBrand = certainCar.CarBrand,
            CarOwnerName = certainCar.CarOwnerName,
            CarOwnerEmail = certainCar.Owner == null ? null : certainCar.Owner.Email,
            SomeParts = certainCar.SomeParts,

            CarOwnerPhone = certainCar.CarOwnerPhone == null ? null : _cryptService.DecryptText(certainCar.CarOwnerPhone),
            DefaultOwnerPhone = certainCar.DefaultOwnerPhone == null ? "No default phone" : _cryptService.DecryptText(certainCar.DefaultOwnerPhone),

            CarVIN = certainCar.CarVIN,
            CarQrCode = certainCar.CarQRCode,
            CarownerId = certainCar.OwnerId.ToString()
        };

        

        return ResultHandler<CarDTO>.Success(carToReturn);
    }


    public async Task<ResultHandler<CarDTO>> AddQrCodeToCar(Guid CarId)
    {
        // return true if car exists
        Car foundedCar = await _garageRepo.RetriveCar(CarId,forEditing:true);
        if(foundedCar == null|| !String.IsNullOrEmpty(foundedCar.CarQRCode))
        {
            return ResultHandler<CarDTO>.Fail("Error with the car : its null, or QR code not empty");
        }

        string string64QR = await _qrCodeService.CreteQRCode(CarId);
        if(String.IsNullOrEmpty(string64QR))
        {
            return ResultHandler<CarDTO>.Fail("Qr Code is empty");
        }

        Car car = await _garageRepo.AddQRCodeForCar(CarId, string64QR);
        CarDTO carDTO = Convert(car);

        return ResultHandler<CarDTO>.Success(carDTO);

    }

   

    private CarDTO Convert(Car certainCar)
    {
        CarDTO carToReturn = new CarDTO()
        {
            CarId = certainCar.CarId,
            CarSpz = certainCar.CarSpz,
            CarBrand = certainCar.CarBrand,
            CarOwnerName = certainCar.CarOwnerName,
            CarOwnerPhone = certainCar.CarOwnerPhone,
            CarVIN = certainCar.CarVIN,
            CarQrCode= certainCar.CarQRCode
        };
        return carToReturn;
    }
} 