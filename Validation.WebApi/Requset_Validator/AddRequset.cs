using AutoFluentValidation;
using AutoFluentValidation.Attributes;
using FluentValidation;

namespace Validation.WebApi.Requset_Validator;

[Validator(typeof(AddRequsetValidator))]
public record AddRequset(int Num,string Name,int Age):IValidatorBase;
public class AddRequsetValidator : AbstractValidator<AddRequset>
{
    public AddRequsetValidator()
    {
        RuleFor(e => e.Num).NotNull().NotEmpty().LessThanOrEqualTo(0);
        RuleFor(e => e.Name).NotEmpty().NotEmpty().MaximumLength(20).MinimumLength(2);
        RuleFor(e=>e.Age).LessThanOrEqualTo(20).GreaterThanOrEqualTo(0);
    }
}