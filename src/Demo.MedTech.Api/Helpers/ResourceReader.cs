using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Demo.MedTech.Api.Helpers
{
    /// <summary>
    /// Static class to read string resources
    /// </summary>
    public static class ResourceReader
    {
        static ResourceReader()
        {
            Resources = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("Resources/Resources.json"));
        }

        private static Dictionary<string, string> Resources { get; }

        public static string ReadValue(string key, params string[] placeHolders)
        {
            var textResource = Resources[key];

            if (placeHolders != null && placeHolders.Any())
            {
                textResource = string.Format(textResource, placeHolders);
            }

            return textResource;
        }
    }
}