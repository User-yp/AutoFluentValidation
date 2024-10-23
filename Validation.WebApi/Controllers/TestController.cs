using AutoFluentValidation;
using Microsoft.AspNetCore.Mvc;
using Validation.WebApi.Requset_Validator;

namespace Validation.WebApi.Controllers;

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

    [HttpPost]
    public async Task<IActionResult> TestNoRequsetAsync([FromBody] TestNoRequset request)
    {
        var ves = new ValidatorResult();
        try
        {
             ves = await validator.RequestValidateAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Ok(ex);
        }
        return Ok(ves);
    }
}
