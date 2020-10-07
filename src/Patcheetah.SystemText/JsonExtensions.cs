using System;
using System.Text.Json;
using System.Buffers;

namespace Patcheetah.SystemText
{
    public static partial class JsonExtensions
    {
        public static object ToObject(this JsonElement element, Type type, JsonSerializerOptions options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
                element.WriteTo(writer);
            return JsonSerializer.Deserialize(bufferWriter.WrittenSpan, type, options);
        }

        public static object ToObject(this JsonDocument document, Type type, JsonSerializerOptions options = null)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            return document.RootElement.ToObject(type);
        }
    }
}
