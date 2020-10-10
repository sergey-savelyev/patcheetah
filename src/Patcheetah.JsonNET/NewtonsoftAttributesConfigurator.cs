using Newtonsoft.Json;
using Patcheetah.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Patcheetah.JsonNET
{
    public class NewtonsoftAttributesConfigurator : ICustomAttributesConfigurator
    {
        public void Configure(PropertyInfo property, IEnumerable<Attribute> propertyAttributes, EntityConfigAccessor configAccessor)
        {
            var jsonIgnoreAttribute = propertyAttributes.FirstOrDefault(attr => attr is JsonIgnoreAttribute);
            var jsonPropertyAttribute = propertyAttributes.FirstOrDefault(attr => attr is JsonPropertyAttribute);

            if (jsonIgnoreAttribute != null)
            {
                configAccessor.EntityConfig.IgnoreOnPatching(property.Name);
            }

            if (jsonPropertyAttribute != null)
            {
                configAccessor.EntityConfig.UseJsonAlias(property.Name, (jsonPropertyAttribute as JsonPropertyAttribute).PropertyName);
            }
        }
    }
}
