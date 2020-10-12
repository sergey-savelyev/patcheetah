using System;
using Microsoft.AspNetCore.Mvc;
using Patcheetah.Patching;

namespace Patcheetah.TestApp31.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpPatch]
        public IActionResult Patch(PatchObject<WeatherForecast> patch)
        {
            var model = new WeatherForecast
            {
                Date = DateTime.Now,
                TemperatureC = 12,
                Summary = "summary",
                Ignored = "I was already here"
            };

            patch.ApplyTo(model);
            var newModel = patch.CreateNewEntity();

            return Ok(new WeatherForecast[] { newModel, model });
        }
    }
}
