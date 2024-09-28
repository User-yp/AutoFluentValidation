using AutoFluentValidation.Attributes;
using FluentValidation;
using Validation.WebApi.Requset_Validator;

namespace Validation.WebApi;

[ValidatorControl]
public static class ValidatorControl
{
    public static IValidator<TestRequset> TestRequset;
    public static IValidator<AddRequset> AddOrderRequset;
    public static IValidator<TestNoRequset> TestNoRequset;

    public static void Init(ServiceProvider service)
    {
        TestRequset = service.GetService<IValidator<TestRequset>>();
        AddOrderRequset = service.GetService<IValidator<AddRequset>>();
        TestNoRequset = service.GetService<IValidator<TestNoRequset>>();
    }
}
