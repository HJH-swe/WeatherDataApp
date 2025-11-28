using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;

namespace Models
{
    // Hittade kod för mapping properties i CsvHelper-dokumentationen
    public sealed class WeatherDataMap : ClassMap<WeatherData>
    {
        public WeatherDataMap()
        {
            Map(m => m.Date).Name("Datum").TypeConverterOption.Format("yyyy-MM-dd H:mm");
            Map(m => m.Location).Name("Plats");
            Map(m => m.Temperature).Name("Temp").TypeConverter<NullableDoubleConverter>();
            Map(m => m.Humidity).Name("Luftfuktighet").TypeConverter<NullableDoubleConverter>();
        }
    }

    // Använder en hjälpklass för att se till att felaktiga double-värden blir null
    public class NullableDoubleConverter : DefaultTypeConverter
    {

        public override object? ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            // Om det är en siffra, godkänd data --> returnera siffran
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result)) 
            {
                return result; 
            }
            // Om det inte är en siffra, felaktig data --> returnera null
            return null;
        }

    }
}