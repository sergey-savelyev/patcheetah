using Newtonsoft.Json.Linq;
using Patcheetah.Patching;
using System;

namespace Patcheetah.JsonNET
{
    public class NewtonsoftJsonTypesResolver : IJsonTypesResolver
    {
        public bool IsArray(object value)
        {
            return value is JArray;
        }

        public bool IsObject(object value)
        {
            return value is JObject;
        }

        public virtual object ResolveType(object value, Type type)
        {
            if (!(value is JToken jtoken))
            {
                return value;
            }

            switch (jtoken.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;
                case JTokenType.Date:
                    return jtoken.ToObject<DateTime>();
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.String:
                    if (type.IsEnum)
                        return Enum.Parse(type, jtoken.ToString());

                    return jtoken.ToString();
                case JTokenType.Object:
                case JTokenType.Array:
                    return jtoken.ToObject(type);
                case JTokenType.Integer:
                    return jtoken.ToObject<int>();
                case JTokenType.Float:
                    return jtoken.ToObject<float>();
                case JTokenType.Boolean:
                    return jtoken.ToObject<bool>();
                case JTokenType.TimeSpan:
                    return jtoken.ToObject<TimeSpan>();
                default:
                    return value;
            }
        }

        public T ResolveType<T>(object value)
        {
            return (T)ResolveType(value, typeof(T));
        }
    }
}
