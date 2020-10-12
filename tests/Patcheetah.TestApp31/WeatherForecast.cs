using Patcheetah.Attributes;
using System;

namespace Patcheetah.TestApp31
{
    public class WeatherForecast
    {
        public WeatherForecast()
        {
        }

        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        [RequiredOnPatching]
        [JsonAlias("Summ")]
        public string Summary { get; set; }

        [IgnoreOnPatching]
        public string Ignored { get; set; }

        public EnTest EnumTest { get; set; }
    }

    public enum EnTest
    {
        One, Two
    }
}
