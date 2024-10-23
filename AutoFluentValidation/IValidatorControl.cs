using FluentValidation;
using System.Threading.Tasks;

namespace AutoFluentValidation
{
    public interface IValidatorControl
    {
        Task <IValidator<T>> GetValidatorAsync<T>(T tType);
        Task<ValidatorResult> RequestValidateAsync<T>(T request) where T : IValidatorBase;
    }
}