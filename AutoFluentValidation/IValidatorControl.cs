using FluentValidation;
using System.Threading.Tasks;

namespace AutoFluentValidation
{
    public interface IValidatorControl
    {
        /// <summary>
        /// 获取指定类型的验证器实例
        /// </summary>
        IValidator<T> GetValidator<T>();

        /// <summary>
        /// 验证请求对象并返回验证结果
        /// </summary>
        Task<ValidatorResult> RequestValidateAsync<T>(T request);
    }
}