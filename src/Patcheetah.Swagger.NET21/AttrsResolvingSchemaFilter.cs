using Patcheetah.Attributes;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Patcheetah.Swagger.NET21
{
    public class AttrsResolvingSchemaFilter : ISchemaFilter
	{
        public void Apply(Schema schema, SchemaFilterContext context)
        {
			if (schema?.Properties == null || context.SystemType == null)
				return;

			var excludedProperties = context.SystemType.GetProperties()
										 .Where(t =>
												t.GetCustomAttribute<IgnoreOnPatchingAttribute>()
												!= null);

			var aliasProperties = context.SystemType.GetProperties()
										 .Where(t =>
												t.GetCustomAttribute<JsonAliasAttribute>()
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
		}
    }
}