using NUnit.Framework;
using Patcheetah.Patching;
using Patcheetah.SystemText;
using Patcheetah.Tests.Customization;
using System;
using System.Globalization;
using System.Text.Json;

namespace Patcheetah.Tests
{
    public class CustomizationTests
    {
        public void Setup()
        {
            PatchEngine.Init(config =>
            {
                config.EnableAttributes();
                config.SetCustomAttributesConfigurator(new CustomAttributesConfigurator());
                config
                    .SetPrePatchProcessingFunction(context =>
                    {
                        if (context.PropertyConfiguration.ExtraSettings.ContainsKey(RoundValueAttribute.PARAMETER_NAME))
                        {
                            if (!(context.NewValue is double))
                            {
                                return context.NewValue;
                            }

                            var precision = (int)(context.PropertyConfiguration.ExtraSettings[RoundValueAttribute.PARAMETER_NAME]);

                            return Math.Round((double)(context.NewValue), precision);
                        }

                        return context.NewValue;
                    });
            });
        }

        [Test]
        public void CustomAttributesTest()
        {
            Setup();

            var distanceValue = 12.6753;
            var distanceRounded = Math.Round(distanceValue, 2);
            var patchJson = @$"{{ ""distance"": {distanceValue.ToString(CultureInfo.InvariantCulture)} }}";
            var patchRequest = JsonSerializer.Deserialize<PatchObject<Route>>(patchJson);
            var model = new Route
            {
                Id = Guid.NewGuid().ToString(),
                StartPoint = "Start",
                DestinationPoint = "Finish",
                Title = "Road to heaven",
                Distance = 32.212
            };

            patchRequest.ApplyTo(model);

            Assert.AreEqual(distanceRounded.ToString(), model.Distance.ToString());
        }
    }
}
