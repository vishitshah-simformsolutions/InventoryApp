using System.Text.Json;
using System.Text.Json.Serialization;
using Product.Utility.Converter;

namespace Product.Utility.Helper
{
    public class JsonSerializerOption
    {
        public static readonly JsonSerializerOptions CamelCasePolicy;
        public static readonly JsonSerializerOptions CaseInsensitive;
        public static readonly JsonSerializerOptions CamelCasePolicyWithEnumAndDateTimeConverter;
        static JsonSerializerOption()
        {
            CamelCasePolicy = new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            CaseInsensitive = new JsonSerializerOptions
            { PropertyNameCaseInsensitive = true };

            CamelCasePolicyWithEnumAndDateTimeConverter = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(), new DateTimeConverter() }
            };
        }
    }
}