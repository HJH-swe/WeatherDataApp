using Microsoft.EntityFrameworkCore;

namespace WeatherData
{
    public class WeatherData
    {
        // Properties som matchar tabellens kolumner
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public double? Temperature { get; set; }
        public double? Humidity { get; set; }
        public string MoldRisk
        {
            get
            {
                // Delat upp mögelrisken i tre nivåer baserat på temperatur och fuktighet
                // Hög, måttlig och låg risk - hittade enkla riktlinjer på nätet

                // Först lite felhantering
                if (Temperature == null || Humidity == null)
                {
                    return "Okänd - otillräcklig data";        // Todo: Ska det vara på engelska? Hur är det i tabellen?
                }
                // High risk: T between 5–30°C and RH above 75%
                if (Temperature >= 5 && Temperature <= 30 && Humidity > 75)
                {
                    return "Hög";
                }
                //Medium risk: T between 5–30°C and RH between 65–75%
                else if (Temperature >= 5 && Temperature <= 30 && Humidity >= 65 && Humidity <= 75)
                {
                    return "Måttlig";
                }
                // Low risk: Otherwise
                else
                {
                    return "Låg";
                }

            }
        }
    }
}
