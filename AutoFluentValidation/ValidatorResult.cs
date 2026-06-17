using FluentValidation.Results;
using System.Collections.Generic;

namespace AutoFluentValidation
{
    public class ValidatorResult
    {
        public bool IsValid { get; private set; } = true;

        public int ErrorCount { get; private set; } = 0;

        public List<ErrorMessage> ErrorMessages { get; private set; } = new List<ErrorMessage>();

        public ValidatorResult()
        {
        }

        /// <summary>
        /// 设置验证错误信息（重复调用会覆盖之前的结果）
        /// </summary>
        public ValidatorResult SetErrorMessage(List<ValidationFailure> failures)
        {
            // 重置状态，确保幂等性
            ErrorMessages = new List<ErrorMessage>();
            ErrorCount = failures?.Count ?? 0;
            IsValid = ErrorCount == 0;

            if (!IsValid && failures != null)
            {
                foreach (var failure in failures)
                {
                    ErrorMessages.Add(new ErrorMessage(failure.PropertyName, failure.ErrorMessage));
                }
            }

            return this;
        }
    }

    public record ErrorMessage(string? PropertyName, string? Message);
}
