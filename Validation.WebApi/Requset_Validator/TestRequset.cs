using AutoFluentValidation.Attributes;
using FluentValidation;
using AutoFluentValidation;

namespace Validation.WebApi.Requset_Validator;

[Validator(typeof(TestRequsetValidator))]
public record TestRequset(int Num, string Length) : IValidatorBase;
public class TestRequsetValidator : AbstractValidator<TestRequset>
{
    public TestRequsetValidator()
    {
        RuleFor(e => e.Num).NotNull().NotEmpty().LessThanOrEqualTo(0);
        RuleFor(e => e.Length).NotEmpty().NotEmpty().MaximumLength(20).MinimumLength(2);
    }
}
