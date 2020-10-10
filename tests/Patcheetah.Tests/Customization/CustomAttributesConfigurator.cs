using Patcheetah.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Patcheetah.Tests.Customization
{
    public class CustomAttributesConfigurator : ICustomAttributesConfigurator
    {
        public void Configure(PropertyInfo property, IEnumerable<Attribute> propertyAttributes, EntityConfigAccessor configAccessor)
        {
            var roundAttribute = propertyAttributes.FirstOrDefault(attr => attr is RoundValueAttribute);
            if (roundAttribute != null)
            {
                configAccessor
                    .GetPropertyConfiguration(property.Name)
                    .ExtraSettings
                    .Add(RoundValueAttribute.PARAMETER_NAME, (roundAttribute as RoundValueAttribute).Precision);
            }
        }
    }
}
