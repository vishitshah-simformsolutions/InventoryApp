using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Demo.MedTech.Utility.Converter
{
    /// <summary>
    /// Serializes the datetime in yyyy-MM-ddTHH:mm:ss.fffZ
    /// eg: 2022-03-17T09:44:18.123Z
    /// </summary>
    public class SbsDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString(), DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }
}