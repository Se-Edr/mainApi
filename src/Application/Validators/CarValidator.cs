using Domain.DTOs;
using FluentValidation;

namespace Application.Validators;

public class CarValidator: AbstractValidator<CarDTO>
{
    public CarValidator()
    {
        RuleFor(car => car.CarSpz).Length(3, 10);
        RuleFor(car=>car.CarBrand).Length(3, 100);
        RuleFor(car=>car.CarOwnerName).Length(3, 100);
        RuleFor(car=>car.CarOwnerPhone).Length(3, 100);
        RuleFor(car=>car.CarVIN).Length(17);
    }

}
