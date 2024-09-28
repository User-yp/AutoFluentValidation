using AutoFluentValidation;

namespace Validation.WebApi.Requset_Validator;

public record TestNoRequset(int Num, string Length) : IValidatorBase;
