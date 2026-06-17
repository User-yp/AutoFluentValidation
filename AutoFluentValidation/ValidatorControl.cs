using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AutoFluentValidation
{
    public class ValidatorControl : IValidatorControl
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidatorControl(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IValidator<T> GetValidator<T>()
        {
            return _serviceProvider.GetService<IValidator<T>>()
                ?? throw new InvalidOperationException(
                    $"未注册 IValidator<{typeof(T).Name}> 类型的验证器。请确保已正确标记 [Validator] 特性并调用 AddFluentValidation。");
        }

        public async Task<ValidatorResult> RequestValidateAsync<T>(T request)
        {
            var validator = _serviceProvider.GetService<IValidator<T>>()
                ?? throw new InvalidOperationException(
                    $"未注册 IValidator<{typeof(T).Name}> 类型的验证器。请确保已正确标记 [Validator] 特性并调用 AddFluentValidation。");

            var val = await validator.ValidateAsync(new ValidationContext<T>(request));
            var result = new ValidatorResult();
            if (!val.IsValid)
                return result.SetErrorMessage(val.Errors);
            return result;
        }
    }
}
