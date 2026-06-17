namespace AutoFluentValidation
{
    /// <summary>
    /// 标记接口，用于标识需要自动验证的类。
    /// 实现此接口为可选项 — 用于提升代码可读性和意图表达。
    /// 注意：从 1.1.0 开始，RequestValidateAsync 不再强制要求此接口。
    /// </summary>
    public interface IValidatorBase { }
}
