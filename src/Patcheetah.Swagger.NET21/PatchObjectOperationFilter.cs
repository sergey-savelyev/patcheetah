using Patcheetah.Patching;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace Patcheetah.Swagger.NET21
{
    public class PatchObjectOperationFilter : IOperationFilter
	{
		private static bool IsPatchObjectType(Type t) =>
			(t != null) &&
			t.IsGenericType &&
			(t.GetGenericTypeDefinition() == typeof(PatchObject<>));


        public void Apply(Operation operation, OperationFilterContext context)
        {
			var parameters = context.ApiDescription.ParameterDescriptions;

			for (var i = 0; i < parameters.Count; i++)
			{
				var parameter = parameters[i];

				if (IsPatchObjectType(parameter.Type))
				{
					(operation.Parameters[i] as BodyParameter).Schema = context.SchemaRegistry.GetOrRegister(parameter.Type.GenericTypeArguments[0]);
				}

				if ((parameter.Type != null) && parameter.Type.IsGenericType && (parameter.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
				{
					var patchObjectType = parameter.Type.GenericTypeArguments[0];

					if (IsPatchObjectType(patchObjectType))
					{
						var enumerableType = typeof(IEnumerable<>);
						var genericEnumerableType = enumerableType.MakeGenericType(patchObjectType.GenericTypeArguments[0]);

						(operation.Parameters[i] as BodyParameter).Schema = context.SchemaRegistry.GetOrRegister(genericEnumerableType);
					}
				}
			}
		}
    }
}