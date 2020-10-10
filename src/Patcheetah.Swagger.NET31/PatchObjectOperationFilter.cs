using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Patcheetah.Patching;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Patcheetah.Swagger.NET31
{
    public class PatchObjectOperationFilter : IOperationFilter
	{
		private static readonly string[] CONTENT_TYPES = new[]
		{
			"application/json",
			"application/patch+json",
			"application/merge-patch+json"
		};

		private static bool IsPatchObjectType(Type t) =>
			(t != null) &&
			t.IsGenericType &&
			(t.GetGenericTypeDefinition() == typeof(PatchObject<>));

		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			OpenApiSchema GenerateSchema(Type type)
				=> context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);

			var bodyParameters = context.ApiDescription.ParameterDescriptions.Where(p => p.Source == BindingSource.Body).ToList();

			foreach (var parameter in bodyParameters)
			{
				if (IsPatchObjectType(parameter.Type))
				{
                    foreach (var contentType in CONTENT_TYPES)
                    {
						if (operation.RequestBody?.Content?.ContainsKey(contentType) ?? false)
                        {
							operation.RequestBody.Content[contentType].Schema = GenerateSchema(parameter.Type.GenericTypeArguments[0]);
						}
                    }
                }

				if ((parameter.Type != null) && parameter.Type.IsGenericType && (parameter.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
				{
					var patchObjectType = parameter.Type.GenericTypeArguments[0];

					if (IsPatchObjectType(patchObjectType))
					{
						var enumerableType = typeof(IEnumerable<>);
						var genericEnumerableType = enumerableType.MakeGenericType(patchObjectType.GenericTypeArguments[0]);

                        foreach (var contentType in CONTENT_TYPES)
                        {
							if (operation.RequestBody?.Content?.ContainsKey(contentType) ?? false)
							{
								operation.RequestBody.Content[contentType].Schema = GenerateSchema(genericEnumerableType);
							}
						}
                    }
				}
			}
		}
	}
}