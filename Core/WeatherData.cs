namespace Models
{

    public class WeatherData
    {
        // Properties som matchar tabellens kolumner
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public double? Temperature { get; set; }
        public double? Humidity { get; set; }

        // En beräknad property som inte finns i csv-filen
        public double MoldRisk
        {
            get
            {
                // Först lite felhantering
                if (Temperature == null || Humidity == null)
                {
                    return 0;
                }
                // Vid låg temp och luftfuktighet är risken liten/obetydlig
                else if (Temperature < 10 || Humidity < 60)
                {
                    return 0;
                }
                // I annat fall - beräkna mögelrisken, avrundat
                else
                {
                    return Math.Round((double)((Temperature - 10.0) * (Humidity / 100.0)), 2);
                }
            }
        }
    }
}
