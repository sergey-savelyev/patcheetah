using Patcheetah.Patching;
using Patcheetah.TestApp472.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Patcheetah.TestApp472.Controllers
{
    public class WeatherForecastController : ApiController
    {
        public IEnumerable<WeatherForecast> Patch([FromBody] PatchObject<WeatherForecast> patch)
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

            return new WeatherForecast[] { newModel, model };
        }
    }
}
