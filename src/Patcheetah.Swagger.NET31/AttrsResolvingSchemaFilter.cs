using Microsoft.OpenApi.Models;
using Patcheetah.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Patcheetah.Swagger.NET31
{
    public class AttrsResolvingSchemaFilter : ISchemaFilter
	{
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			if (schema?.Properties == null || context.Type == null)
				return;

			var excludedProperties = context.Type.GetProperties()
										 .Where(t =>
												t.GetCustomAttribute<IgnoreOnPatchingAttribute>()
												!= null);

			var aliasProperties = context.Type.GetProperties()
										 .Where(t =>
												t.GetCustomAttribute<JsonAliasAttribute>()
												!= null);

			var requiredProperties = context.Type.GetProperties()
										 .Where(t =>
												t.GetCustomAttribute<RequiredOnPatchingAttribute>()
												!= null);

			foreach (var excludedProperty in excludedProperties)
			{
				var toDel = schema.Properties.FirstOrDefault(x => x.Key.ToLower() == excludedProperty.Name.ToLower());
				schema.Properties.Remove(toDel);
			}

			foreach (var aliasProperty in aliasProperties)
			{
				var toDel = schema.Properties.FirstOrDefault(x => x.Key.ToLower() == aliasProperty.Name.ToLower());
				var val = toDel.Value;
				var key = aliasProperty.GetCustomAttribute<JsonAliasAttribute>().Alias;

				schema.Properties.Remove(toDel);
				schema.Properties.Add(key, val);
			}

			foreach (var requiredProperty in requiredProperties)
			{
				var propName = requiredProperty.Name;

				if (aliasProperties.Contains(requiredProperty))
                {
					var alias = aliasProperties
						.First(x => x.Name == requiredProperty.Name)
						.GetCustomAttribute<JsonAliasAttribute>()
						.Alias;
				}

				schema.Required.Add(propName);
			}
		}
	}
}