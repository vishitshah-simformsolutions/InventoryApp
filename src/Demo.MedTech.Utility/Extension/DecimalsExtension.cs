using System.Globalization;

namespace Demo.MedTech.Utility.Extension
{
    public static class DecimalsExtension
    {
        public static int GetDecimalPlaces(this decimal n)
        {
            var parts = n.ToString(CultureInfo.InvariantCulture).Split('.');

            if (parts.Length < 2)
                return 0;

            return parts[1].Length;
        }
    }
}
