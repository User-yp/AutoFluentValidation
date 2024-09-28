using AutoFluentValidation;
using AutoFluentValidation.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Validation.WebApi.Requset_Validator;

namespace Validation.WebApi.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> TestRequsetAsync([FromBody] TestRequset request)
    {

        var ves = await ValidatorControl.TestRequset.RequestValidateAsync(request);
        return Ok(ves);
    }

    [HttpPost]
    public async Task<IActionResult> TestNoRequsetAsync([FromBody] TestNoRequset request)
    {
        var ves = new ValidatorResult();
        try
        {
             ves = await ValidatorControl.TestNoRequset.RequestValidateAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        return Ok(ves);
    }
}
