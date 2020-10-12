using Patcheetah.Patching;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Patcheetah.SystemText
{
    public class SystemTextJsonTypesResolver : IJsonTypesResolver
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public SystemTextJsonTypesResolver()
        {
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new ObjectToInferredTypesConverter());
        }

        public bool IsArray(object value)
        {
            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind == JsonValueKind.Array;
            }

            return value.GetType().IsArray;
        }

        public bool IsObject(object value)
        {
            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind == JsonValueKind.Object;
            }

            return false;
        }

        public virtual object ResolveType(object value, Type type)
        {
            if (!(value is JsonElement jsonElement))
            {
                return value;
            }

            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                case JsonValueKind.Array:
                case JsonValueKind.Object:
                    return jsonElement.ToObject(type, _serializerOptions);
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.Number:
                    return jsonElement.ToObject(type);
                case JsonValueKind.String:
                    if (type.IsEnum)
                        return Enum.Parse(type, jsonElement.GetString(), true);
                    if (type == typeof(DateTime))
                        return DateTime.Parse(jsonElement.GetString());
                    return jsonElement.GetString();
                default:
                    return value;
            }

        }

        public T ResolveJsonType<T>(object value)
        {
            return (T)ResolveType(value, typeof(T));
        }
    }

    // https://docs.microsoft.com/ru-ru/dotnet/standard/serialization/system-text-json-converters-how-to#registration-sample---converters-collection
    public class ObjectToInferredTypesConverter
        : JsonConverter<object>
    {
        public override object Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }

            if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int l))
                {
                    return l;
                }

                return reader.GetDouble();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.TryGetDateTime(out DateTime datetime))
                {
                    return datetime;
                }

                return reader.GetString();
            }

            // Use JsonElement as fallback.
            // Newtonsoft uses JArray or JObject.
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            return document.RootElement.Clone();
        }

        public override void Write(
            Utf8JsonWriter writer,
            object objectToWrite,
            JsonSerializerOptions options) =>
                throw new InvalidOperationException("Should not get here.");
    }
}
