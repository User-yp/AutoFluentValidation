# AutoFluentValidation

[![NuGet](https://img.shields.io/nuget/v/AutoFluentValidation.svg)](https://www.nuget.org/packages/AutoFluentValidation)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

为 **FluentValidation** 提供基于特性的自动注册与手动调用的轻量级验证框架。告别在 `Program.cs` 中逐条注册验证器的繁琐，也无需在 Controller 中逐个注入 `IValidator<T>`。

---

## 解决的问题

使用 FluentValidation 的传统做法需要在两个地方重复维护验证器列表：

**Program.cs** — 逐条注册：

```csharp
builder.Services.AddScoped<IValidator<AddRequest>, AddRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateRequest>, UpdateRequestValidator>();
builder.Services.AddScoped<IValidator<DeleteRequest>, DeleteRequestValidator>();
// ... 随着项目增长，这里会越来越长
```

**Controller** — 逐个注入：

```csharp
public class TestController : ControllerBase
{
    private readonly IValidator<AddRequest> _addValidator;
    private readonly IValidator<UpdateRequest> _updateValidator;
    private readonly IValidator<DeleteRequest> _deleteValidator;

    public TestController(
        IValidator<AddRequest> addValidator,
        IValidator<UpdateRequest> updateValidator,
        IValidator<DeleteRequest> deleteValidator)
    {
        // ... 构造函数也会越来越臃肿
    }
}
```

**AutoFluentValidation** 将这一切简化为：一个特性 + 一行注册 + 一个注入。

---

## 快速开始

### 1. 安装

```bash
dotnet add package AutoFluentValidation
```

### 2. 定义请求类和验证器

```csharp
using AutoFluentValidation;
using AutoFluentValidation.Attributes;
using FluentValidation;

// 为请求类添加 [Validator] 特性，传入验证器类型
[Validator(typeof(CreateOrderRequestValidator))]
public record CreateOrderRequest(string Name, int Quantity, decimal Price);

// 按常规方式编写验证规则
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(e => e.Name).NotEmpty().MaximumLength(100);
        RuleFor(e => e.Quantity).GreaterThan(0);
        RuleFor(e => e.Price).GreaterThan(0);
    }
}
```

### 3. 注册服务（Program.cs）

```csharp
using AutoFluentValidation.Extensions;
using System.Reflection;

builder.Services.AddFluentValidation(Assembly.GetExecutingAssembly());
```

### 4. 在 Controller 中使用

```csharp
[Route("[controller]/[action]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IValidatorControl _validator;

    public OrderController(IValidatorControl validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderRequest request)
    {
        var result = await _validator.RequestValidateAsync(request);
        if (!result.IsValid)
            return BadRequest(result);

        // 业务逻辑...
        return Ok();
    }
}
```

---

## API 参考

### ValidatorAttribute 特性

标记在需要验证的类上，告知框架该类的验证器类型及服务生命周期。

| 属性 | 类型 | 说明 |
|------|------|------|
| `ServiceType` | `Type` | 验证器类型，必须继承自 `AbstractValidator<T>` |
| `LifeTime` | `ServiceLifetime` | 服务注册生命周期，默认为 `Scoped` |

```csharp
[Validator(typeof(MyValidator), LifeTime = ServiceLifetime.Singleton)]
public record MyRequest(string Field);
```

### IValidatorControl 接口

| 方法 | 说明 |
|------|------|
| `GetValidator<T>()` | 从 DI 容器获取 `IValidator<T>` 实例 |
| `RequestValidateAsync<T>(T request)` | 直接验证请求对象并返回 `ValidatorResult` |

### ValidatorResult 验证结果

| 属性 | 类型 | 说明 |
|------|------|------|
| `IsValid` | `bool` | 验证是否通过 |
| `ErrorCount` | `int` | 错误数量 |
| `ErrorMessages` | `List<ErrorMessage>` | 错误信息列表 |

`ErrorMessage` 为 record 类型，包含 `PropertyName` 和 `Message` 两个属性。

### AddFluentValidation 扩展方法

```csharp
// 传入单个程序集
builder.Services.AddFluentValidation(Assembly.GetExecutingAssembly());

// 传入多个程序集
builder.Services.AddFluentValidation(assembly1, assembly2, assembly3);

// 传入 IEnumerable<Assembly>
builder.Services.AddFluentValidation(assemblies);
```

---

## 验证结果示例

请求体 `{}`（空对象）：

```json
{
  "isValid": false,
  "errorCount": 2,
  "errorMessages": [
    {
      "propertyName": "Name",
      "message": "'Name' 不能为空。"
    },
    {
      "propertyName": "Quantity",
      "message": "'Quantity' 必须大于 0。"
    }
  ]
}
```

---

## 高级用法

### 自定义生命周期

```csharp
// 无状态验证器可注册为 Singleton
[Validator(typeof(MyValidator), LifeTime = ServiceLifetime.Singleton)]
public record MyRequest(string Data);

// 多数情况下 Scoped 是最佳选择（默认值）
[Validator(typeof(AnotherValidator), LifeTime = ServiceLifetime.Scoped)]
public record AnotherRequest(string Data);
```

### IValidatorBase 标记接口（可选）

框架提供 `IValidatorBase` 空接口，实现它可提升代码可读性：

```csharp
[Validator(typeof(MyValidator))]
public record MyRequest : IValidatorBase;  // 可选，仅作标记用途
```

> **注意**：从 v1.1.0 开始，`RequestValidateAsync<T>` 不再强制要求泛型参数 `T` 实现 `IValidatorBase`。此接口现为可选的语义标记。

### 手动获取验证器

如果你只需要验证器实例而不立即执行验证：

```csharp
var validator = _validator.GetValidator<CreateOrderRequest>();
// 自行调用验证逻辑...
```

---

## 最佳实践

1. **验证规则写在 Validator 中**，与 FluentValidation 原生用法完全一致
2. **将请求类及其 Validator 放在同一文件**，便于维护
3. **对无状态的验证器使用 `Singleton` 生命周期**以减少内存分配
4. **拥有 DbContext 依赖的验证器使用 `Scoped`**（默认值）
5. **轻量验证器使用 `Transient`**

---

## 依赖项

| 包 | 最低版本 |
|----|----------|
| FluentValidation | 10.4.0 |
| Microsoft.Extensions.DependencyInjection | 5.0.0+ |

支持目标框架：`net5.0` `net6.0` `net7.0` `net8.0`

---

## 许可证

[MIT](LICENSE)

---

## 项目地址

[https://github.com/User-yp/AutoFluentValidation](https://github.com/User-yp/AutoFluentValidation)

欢迎 Star、Issue 和 Pull Request！
