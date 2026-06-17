using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace AutoFluentValidation.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ValidatorAttribute : Attribute
    {
        /// <summary>
        /// 服务生命周期，默认为 Scoped
        /// </summary>
        public ServiceLifetime LifeTime { get; set; }

        /// <summary>
        /// 验证器类型，必须继承自 AbstractValidator&lt;&gt;
        /// </summary>
        public Type ServiceType { get; init; }

        public ValidatorAttribute(Type serviceType, ServiceLifetime lifeTime = ServiceLifetime.Scoped)
        {
            if (!CheckValidator(serviceType))
                throw new ArgumentException(
                    $"类型 {serviceType.FullName} 必须继承自 AbstractValidator<>。", nameof(serviceType));

            ServiceType = serviceType;
            LifeTime = lifeTime;
        }

        private static bool CheckValidator(Type type)
        {
            if (type is null) return false;

            var abstractValidatorType = typeof(AbstractValidator<>);

            // 检查自身是否是 AbstractValidator<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == abstractValidatorType)
                return true;

            // 检查基类是否是 AbstractValidator<T>
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == abstractValidatorType)
                    return true;
                baseType = baseType.BaseType;
            }

            // 检查实现的接口
            return type.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == abstractValidatorType);
        }
    }
}
