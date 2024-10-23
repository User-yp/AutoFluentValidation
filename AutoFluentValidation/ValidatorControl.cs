using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AutoFluentValidation
{
    public class ValidatorControl : IValidatorControl
    {
        public readonly ServiceProvider _service;
        public ValidatorControl(ServiceProvider service)
        {
            _service = service;
        }

        public Task<IValidator<T>> GetValidatorAsync<T>(T tType)
        {
            var validator= _service.GetService<IValidator<T>>() 
                ?? throw new ApplicationException(" 'validator' Not Registered ");
            return Task.FromResult(validator);
        }

        public async Task<ValidatorResult> RequestValidateAsync<T>(T request) where T : IValidatorBase
        {
            var validator =await GetValidatorAsync(request);
            var val = await validator.ValidateAsync(new ValidationContext<T>(request));
            var result = new ValidatorResult();
            if (!val.IsValid)
                return result.SetErrorMessage(val.Errors);
            return result;
        }
    }
}
