using AutoFluentValidation;
using AutoFluentValidation.Attributes;
using FluentValidation;

namespace Validation.WebApi.Request_Validator;

[Validator(typeof(TestRequestValidator))]
public record TestRequest(int Num, string Length) : IValidatorBase;

public class TestRequestValidator : AbstractValidator<TestRequest>
{
    public TestRequestValidator()
    {
        RuleFor(e => e.Num).GreaterThan(0).WithMessage("Num 必须大于 0");
        RuleFor(e => e.Length).NotEmpty().MaximumLength(20).MinimumLength(2);
    }
}
