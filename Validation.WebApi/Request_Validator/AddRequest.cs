using AutoFluentValidation;
using AutoFluentValidation.Attributes;
using FluentValidation;

namespace Validation.WebApi.Request_Validator;

[Validator(typeof(AddRequestValidator))]
public record AddRequest(int Num, string Name, int Age) : IValidatorBase;

public class AddRequestValidator : AbstractValidator<AddRequest>
{
    public AddRequestValidator()
    {
        RuleFor(e => e.Num).GreaterThan(0).WithMessage("Num 必须大于 0");
        RuleFor(e => e.Name).NotEmpty().MaximumLength(20).MinimumLength(2);
        RuleFor(e => e.Age).LessThanOrEqualTo(120).GreaterThanOrEqualTo(0);
    }
}
