using AutoFluentValidation.Attributes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoFluentValidation.Extensions
{
    public static class ValidatorExtensions
    {
        /// <summary>
        /// 按特性中的生命周期注入验证器组件
        /// </summary>
        public static IServiceCollection AddFluentValidation(this IServiceCollection service, IEnumerable<Assembly> assemblies)
        {
            if (assemblies is null)
                throw new ArgumentNullException(nameof(assemblies));

            return service.AddFluentValidation(assemblies.ToArray());
        }

        /// <summary>
        /// 按特性中的生命周期注入验证器组件
        /// </summary>
        public static IServiceCollection AddFluentValidation(this IServiceCollection service, params Assembly[] assemblies)
        {
            if (assemblies is null || assemblies.Length == 0)
                throw new ArgumentException("Assemblies cannot be null or empty", nameof(assemblies));

            service.InitValidatorService(assemblies);
            service.AddValidatorControl();
            return service;
        }

        /// <summary>
        /// 扫描指定程序集中标记了 [Validator] 特性的类，并按配置的生命周期注册验证器
        /// </summary>
        private static IServiceCollection InitValidatorService(this IServiceCollection service, params Assembly[] assemblies)
        {
            if (assemblies is null || assemblies.Length == 0)
                throw new ArgumentException("Assemblies cannot be null or empty", nameof(assemblies));

            // 获取标记了 ValidatorAttribute 特性的所有非抽象类
            List<Type> typeAttribute = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract
                        && t.IsDefined(typeof(ValidatorAttribute), false))
                    .ToList();

            if (typeAttribute.Count == 0)
                return service;

            foreach (var impl in typeAttribute)
            {
                // 只调用一次 GetCustomAttribute
                var attr = impl.GetCustomAttribute<ValidatorAttribute>()!;
                var lifetime = attr.LifeTime;
                var serviceType = attr.ServiceType;

                // 验证 ServiceType 的泛型参数与目标类型匹配
                ValidateValidatorType(serviceType, impl);

                // 构建 IValidator<T> 类型
                var validatorType = typeof(IValidator<>).MakeGenericType(impl);

                // 根据生命周期注册服务
                switch (lifetime)
                {
                    case ServiceLifetime.Singleton:
                        service.AddSingleton(validatorType, serviceType);
                        break;
                    case ServiceLifetime.Scoped:
                        service.AddScoped(validatorType, serviceType);
                        break;
                    case ServiceLifetime.Transient:
                        service.AddTransient(validatorType, serviceType);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(lifetime), $"不支持的服务生命周期: {lifetime}");
                }
            }

            return service;
        }

        /// <summary>
        /// 验证 ServiceType 的 AbstractValidator&lt;T&gt; 泛型参数是否与目标类型匹配
        /// </summary>
        private static void ValidateValidatorType(Type serviceType, Type targetType)
        {
            var abstractValidatorType = typeof(AbstractValidator<>);

            // 遍历基类链，找到 AbstractValidator<T>
            var baseType = serviceType;
            while (baseType != null)
            {
                if (baseType.IsGenericType
                    && baseType.GetGenericTypeDefinition() == abstractValidatorType)
                {
                    var validatedType = baseType.GetGenericArguments()[0];
                    if (validatedType != targetType)
                    {
                        throw new InvalidOperationException(
                            $"验证器 {serviceType.FullName} 验证的类型是 {validatedType.FullName}，" +
                            $"但它被标记在了 {targetType.FullName} 上。");
                    }
                    return;
                }
                baseType = baseType.BaseType;
            }

            // 不应该走到这里，因为 ValidatorAttribute 的构造函数已经验证过
            throw new InvalidOperationException(
                $"类型 {serviceType.FullName} 未继承 AbstractValidator<>。");
        }

        /// <summary>
        /// 注册 ValidatorControl 作为单例服务
        /// </summary>
        private static void AddValidatorControl(this IServiceCollection service)
        {
            service.AddSingleton<IValidatorControl>(provider =>
                new ValidatorControl(provider));
        }
    }
}
