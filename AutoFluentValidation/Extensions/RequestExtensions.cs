using FluentValidation;

namespace AutoFluentValidation.Extensions;

public static class RequestExtensions
{
    public static async Task<ValidatorResult> RequestValidateAsync<T>(this IValidator validator, T request) where T : IValidatorBase
    {
        if (validator == null)
            throw new ApplicationException(" 'validator' Not Registered ");

        var val = await validator.ValidateAsync(new ValidationContext<T>(request));

        var result = new ValidatorResult();
        if (!val.IsValid)
            return result.SetErrorMessage(val.Errors);
        return result;
    }
}
