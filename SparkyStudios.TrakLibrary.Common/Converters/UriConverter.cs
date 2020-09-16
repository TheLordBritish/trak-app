using System;
using Newtonsoft.Json;

namespace SparkyStudios.TrakLibrary.Common.Converters
{
    public class UriConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch (value)
            {
                case null:
                    writer.WriteNull();
                    break;
                case Uri uri:
                    writer.WriteValue(uri.OriginalString);
                    break;
                default:
                    throw new InvalidOperationException("Unhandled case for UriConverter. Check to see if this converter has been applied to the wrong serialization type.");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.TokenType switch
            {
                JsonToken.String => new Uri((string) reader.Value ?? ""),
                JsonToken.Null => null,
                _ => throw new InvalidOperationException(
                    "Unhandled case for UriConverter. Check to see if this converter has been applied to the wrong serialization type.")
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Uri);
        }
    }
}