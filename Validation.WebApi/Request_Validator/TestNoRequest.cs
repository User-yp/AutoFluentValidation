using AutoFluentValidation;

namespace Validation.WebApi.Request_Validator;

/// <summary>
/// 没有注册验证器的请求类 — 用于演示缺少验证器时的错误处理
/// </summary>
public record TestNoRequest(int Num, string Length) : IValidatorBase;
