using AutoFluentValidation;
using Microsoft.AspNetCore.Mvc;
using Validation.WebApi.Request_Validator;

namespace Validation.WebApi.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly IValidatorControl _validator;

    public TestController(IValidatorControl validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> TestRequestAsync([FromBody] TestRequest request)
    {
        var result = await _validator.RequestValidateAsync(request);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> TestNoRequestAsync([FromBody] TestNoRequest request)
    {
        ValidatorResult result;
        try
        {
            result = await _validator.RequestValidateAsync(request);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        return Ok(result);
    }
}
