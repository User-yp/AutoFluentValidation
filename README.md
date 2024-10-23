# AutoFluentValidation

为FluentValidation实现批量手动注册的简单框架。

#### Before

1.你需要在Program.cs中注册所有你需要校验类的服务

```c#
builder.Services.AddScoped<IValidator<AddRequset>, AddRequsetValidator>();
//
//....
//
builder.Services.AddScoped<IValidator<TestRequset>, TestRequsetValidator>();
```

2.并且在Controller中注入每一个你需要校验类的服务

```c#
[Route("[controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly IValidator<AddRequset> addValidator;
    //
    //....
    //
    private readonly IValidator<TestRequset> testValidator;

    public TestController(IValidator<AddRequset> addValidator,IValidator<TestRequset> testValidator)
    {
        this.addValidator = addValidator;
        //
        //....
        ///
        this.testValidator = testValidator;
    }
}
```

显然，这么做会显得十分臃肿

#### Now

1.你只需要在编写校验类时加一个特性便签

```c#
[Validator(typeof(AddRequsetValidator))]//特性标签
public record AddRequset(int Num,string Name,int Age):IValidatorBase;//待校验类
public class AddRequsetValidator : AbstractValidator<AddRequset>//校验规则类
{
    public AddRequsetValidator()
    {
        RuleFor(e => e.Num).NotNull().NotEmpty().LessThanOrEqualTo(0);
        RuleFor(e => e.Name).NotEmpty().NotEmpty().MaximumLength(20).MinimumLength(2);
        RuleFor(e=>e.Age).LessThanOrEqualTo(20).GreaterThanOrEqualTo(0);
    }
}
```

2.在Program.cs中注册自动校验服务

```c#
//注册自动注入服务
builder.Services.AddFluentValidation(Assembly.GetExecutingAssembly());
```

3.在Controller中注入一次ValidatorControl服务

```c#
[Route("[controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly IValidatorControl validator;

    public TestController(IValidatorControl validator)
    {
        this.validator = validator;
    }
}
```

此时， 就可以获取所有你标记过的校验结果

```c#
[HttpPost]
public async Task<IActionResult> TestRequsetAsync([FromBody] TestRequset request)
{
    var res = await validator.RequestValidateAsync(request);
    return Ok(res);
}
//return
/*{
  "isValid": false,
  "errorCount": 1,
  "errorMessage": [
    {
      "propertyName": "Num",
      "message": "'Num' 不能为空。"
    }
  ]
}*/
```

以下是使用方法和相关介绍。

#### 使用方法

首先添加一个待校验类和他的校验规则类，这一步与常规的做法一样。大多数的校验类都来自前端不需要构造函数实例化，因此这里写作record类型，当然你也可以对实体类校验，写作class类型。

```c#
public record AddRequset(int Num,string Name,int Age);
public class AddRequsetValidator : AbstractValidator<AddRequset>
{
    public AddRequsetValidator()
    {
        RuleFor(e => e.Num).NotNull().NotEmpty().LessThanOrEqualTo(0);
        RuleFor(e => e.Name).NotEmpty().NotEmpty().MaximumLength(20).MinimumLength(2);
        RuleFor(e=>e.Age).LessThanOrEqualTo(20).GreaterThanOrEqualTo(0);
    }
}
```

然后在待校验类中添加特性标记Validator,并传入参数，他的校验类类型typeof(AddRequsetValidator)。然后让这个类继承自IValidatorBase。IValidatorBase是一个空接口，只是做标记用，后续会做介绍。

```c#
[Validator(typeof(AddRequsetValidator))]
public record AddRequset(int Num,string Name,int Age):IValidatorBase;
public class AddRequsetValidator : AbstractValidator<AddRequset>
{
    public AddRequsetValidator()
    {
        RuleFor(e => e.Num).NotNull().NotEmpty().LessThanOrEqualTo(0);
        RuleFor(e => e.Name).NotEmpty().NotEmpty().MaximumLength(20).MinimumLength(2);
        RuleFor(e=>e.Age).LessThanOrEqualTo(20).GreaterThanOrEqualTo(0);
    }
}
```

接下来就像[NOW]中的第二步和第三步一样，注册服务并在Controller中注入并使用。

#### 相关介绍

1.特性Validator

```c#
[AttributeUsage(AttributeTargets.Class)]
public class ValidatorAttribute : Attribute
{
    public ServiceLifetime LifeTime { get; set; }
    public Type ServiceType { get; init; }
    public ValidatorAttribute(Type ServiceType, ServiceLifetime LifeTime = ServiceLifetime.Scoped)
    {
        //验证ServiceType是否继承自AbstractValidator<>
        if (!CheckValidator(ServiceType))
            throw new ArgumentException($"The type {ServiceType.FullName} must implement AbstractValidator<>.");

        this.LifeTime = LifeTime;
        this.ServiceType = ServiceType;
    }
}
```

他有两个属性，服务生命周期ServiceLifetime和校验规则类类型ServiceType，构造函数中必须传入校验规则类类型ServiceType，如果你不标明生命周期则默认为Scoped。构造函数中还有一个方法CheckValidator()，用来校验你传入的类型是否继承自AbstractValidator<>，确保正确的使用。

2.扩展方法InitValidatorService()

```c#
public static IServiceCollection InitValidatorService(this IServiceCollection service, params Assembly[] assemblies)
{
    if (assemblies == null || assemblies.Length == 0)
    {
        throw new ArgumentException("Assemblies cannot be null or empty");
    }
    //获取有ServiceAttribute特性的所有类
    List<Type> typeAttribute = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract
            && t.GetCustomAttributes(typeof(ValidatorAttribute), false).Length != 0)
            .ToList();

    if (typeAttribute.Count == 0)
        return service;

    typeAttribute.ForEach(impl =>
    {
        //获取生命周期
        var lifetime = impl.GetCustomAttribute<ValidatorAttribute>()!.LifeTime;
        //获取ServiceType
        var serviceType = impl.GetCustomAttribute<ValidatorAttribute>()!.ServiceType;
        //写入泛型参数，获取IValidator<>类型
        var validatorType = typeof(IValidator<>).MakeGenericType(impl);

        //var res=typeof()
        //获取该类注入的生命周期
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
        }
    });
    return service;
}
```

它是这个组件的核心，他会获取指定程序集中所有你标记了ValidatorAttribute特性的类，再获取相对应的校验规则类serviceType类型，

遍历整个集合根据标记的LifeTime，写入泛型参数，获取IValidator<>类型，向IServiceCollection中注入服务。这一步相当于批量实现了。

```c#
builder.Services.AddScoped<IValidator<AddRequset>, AddRequsetValidator>();
//
//....
//
builder.Services.AddScoped<IValidator<TestRequset>, TestRequsetValidator>();
```

3.扩展方法ValidatorControl()

```c#
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

    public async Task<ValidatorResult> RequestValidateAsync<T>(T tValidator) where T : IValidatorBase
    {
        var validator =await GetValidatorAsync(request);
        var val = await validator.ValidateAsync(new ValidationContext<T>(request));
        var result = new ValidatorResult();
        if (!val.IsValid)
            return result.SetErrorMessage(val.Errors);
        return result;
    }
}
```

ValidatorContro中有两个重要的方法GetValidatorAsync和RequestValidateAsync，他们的作用分别是从ServiceProvider中获取你已经注册过的服务IValidator<T>和根据这个IValidator<T>服务去校验待校验类并且返回校验结果ValidatorResult()，因此ValidatorContro需要一个ServiceProvider类型的参数_service，会在注册服务的时候传入它。

上文中提到的IValidatorBase在这里起到了作用，它是一个空接口，仅仅起标记作用，确保你传入的泛型参数tValidator继承自IValidatorBase。

```c#
public interface IValidatorBase { }
```

AddValidatorControl方法用于注册Singleton类型的服务ValidatorControl，并在这时构建ServiceProvider并传入，并且将它作为IValidatorControl类型注册，IValidatorControl中仅有两个方法，用来注入服务并使用。

```c#
public static void AddValidatorControl(this IServiceCollection service)
{
    service.AddSingleton(pro =>
    {
        return new ValidatorControl(service.BuildServiceProvider()) as IValidatorControl;
    });
}
```

4.注册服务扩展方法AddFluentValidation

```c#
public static IServiceCollection AddFluentValidation(this IServiceCollection service, IEnumerable<Assembly> assemblies)
{
    return service.AddFluentValidation(assemblies?.ToArray() ?? Array.Empty<Assembly>());
}
public static IServiceCollection AddFluentValidation(this IServiceCollection service, params Assembly[] assemblies)
{
    service.InitValidatorService(assemblies);
    service.AddValidatorControl();
    return service;
}
```

AddFluentValidation接收IEnumerable<Assembly> 和params Assembly[] assemblies两种参数，用来指定哪些程序集中的校验类你想要校验。他们最终都会调用InitValidatorService和AddValidatorControl方法。

5.注册服务并使用

```c#
//Program.cs
builder.Services.AddFluentValidation(Assembly.GetExecutingAssembly());
//Controller.cs
[Route("[controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly IValidatorControl validator;

    public TestController(IValidatorControl validator)
    {
        this.validator = validator;
    }
    [HttpPost]
    public async Task<IActionResult> TestRequsetAsync([FromBody] TestRequset request)
    {

        var ves = await validator.RequestValidateAsync(request);
        return Ok(ves);
    }
}
```

6.校验结果类ValidatorResult

```c#
public class ValidatorResult
{
    public bool IsValid { get; private set; } = true;
    public int ErrorCount { get; private set; } = 0;

    public List<ErrorMessage> ErrorMessage { get; private set; } = new List<ErrorMessage>();
    public ValidatorResult()
    {

    }
    public ValidatorResult SetErrorMessage(List<ValidationFailure> failures)
    {
        ErrorCount = failures.Count;
        IsValid = ErrorCount == 0;
        if (!IsValid)
            failures.ForEach(failure =>
            {
                ErrorMessage.Add(new ErrorMessage(failure.PropertyName, failure.ErrorMessage));
            });

        return this;
    }
}
public record ErrorMessage(string? PropertyName, string? Message);
```

FluentValidation返回的校验结果List<ValidationFailure>往往有一些是我们不需要的，因此我对他进行了简单的包装，只返回IsValid、ErrorCount和ErrorMessage。他的格式如下，会显得简单明了。

```json
{
  "isValid": false,
  "errorCount": 4,
  "errorMessage": [
    {
      "propertyName": "Num",
      "message": "'Num' 不能为空。"
    },
    {
      "propertyName": "Length",
      "message": "'Length' 不能为空。"
    },
    {
      "propertyName": "Length",
      "message": "'Length' 不能为空。"
    },
    {
      "propertyName": "Length",
      "message": "'Length' 必须大于或等于2个字符。您输入了0个字符。"
    }
  ]
}
```

项目地址:

```
https://github.com/User-yp/AutoFluentValidation
```

希望能为你提供帮助，若有需要改进的地方，欢迎fork。
