

using Domain.DTOs.UsersDTO;
using FluentValidation;

namespace Application.Validators
{
    public class UserValidator:AbstractValidator<RegisterUser>
    {
        public UserValidator()
        {
            RuleFor(user=>user.Password).MinimumLength(3)
                //.Matches(@"[0-9]+")
                //.Matches(@"[A-Z]+")
                //.Matches(@"[a-z]+")
                ;

            RuleFor(user => user.UserEmail).EmailAddress();
        }
    }
}
