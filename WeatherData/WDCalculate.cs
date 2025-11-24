using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WeatherData
{
    // En klass med metoder för alla beräkningar och utskrifter
    // Hade egentligen varit snyggast att bara ha beräkningar här och utskrifter i Program.cs
    // men det fick bli så här den här gången
    public class WDCalculate
    {
        // Metod för att beräkna medeltemperaturen för en vald dag och plats
        public static void AverageTempDay(string location)
        {
            DateTime? date = SelectDate();
            if (date == null)
            {
                Console.WriteLine("\nAn error has occured. Returning to main menu.");
                return;
            }

            using (var db = new WeatherDataContext())
            {
                var data = db.WeatherDataTbl
                                    .Where(w => w.Date >= date.Value.Date       // .Value används eftersom date kan vara null
                                    && w.Date < date.Value.Date.AddDays(1)      // .Date används för att ta bort tidsdelen
                                    && w.Location == location
                                    && w.Temperature.HasValue);

                // Om ingen data hittas för valda datum och plats
                if (!data.Any())
                {
                    Console.WriteLine($"\nNo data found for location \"{location}\" on {date}.");
                }
                // Annars, beräkna och visa medeltemperaturen
                else
                {
                    double avgTemp = data.Average(w => w.Temperature.Value);
                    Console.WriteLine($"\nAverage temperature for \"{location}\" on {date:yyyy-MM-dd}: {avgTemp:F2} °C");
                }
            }
        }
        // Metod för att validera användarinput av datum
        public static DateTime? SelectDate()
        {
            bool isValid = false;
            DateTime? date = null;      // om null skickas tillbaka - nåt har gått fel
            do
            {
                string dateInput = AnsiConsole.Ask<string>("\nEnter a date between 2016-10-01 and 2016-11-30 (yyyymmdd): ");
                date = DateTime.ParseExact(dateInput, "yyyyMMdd", CultureInfo.InvariantCulture);

                // Validera datumet som användaren skriver in
                if (date < new DateTime(2016, 10, 1) || date > new DateTime(2016, 11, 30))
                {
                    Console.WriteLine("Invalid date. Please enter a date between 2016-10-01 and 2016-11-30.");
                }
                else
                {
                    isValid = true;
                    return date;
                }
            }
            while (!isValid);
            return date;
        }
        // Metod för att sortera från varmast till kallast (enligt dagens medeltemp) för en vald plats 
        public static void SortHotToCold(string location)
        {
            using (var db = new WeatherDataContext())
            {
                var data = db.WeatherDataTbl
                                    .Where(w => w.Location == location && w.Temperature.HasValue)
                                    .GroupBy(w => w.Date.Date)                                     // Group by Date --> vill bara få ut en rad per dag
                                    .Select(g => new
                                    {
                                        GroupDate = g.First().Date.Date,                           // .First() --> ger ett resultat
                                        AverageTemp = g.Average(w => w.Temperature.Value)
                                    })
                                    .OrderByDescending(t => t.AverageTemp)                         // Sortera enligt AverageTemp, varmt till kallt
                                    .ToList();

                // Felmeddelande om ingen data hittas
                if (!data.Any())
                {
                    Console.WriteLine($"\nError! No data found for \"{location}\".");
                }
                // Annars, visa medeltemperaturen för varje dag
                else
                {
                    foreach (var row in data)
                    {
                        Console.WriteLine($"\nDate: {row.GroupDate:yyyy-MM-dd}, Average temperature: {row.AverageTemp:F2} °C");
                    }
                }
            }
        }
        // Metod för att sortera från torrast till fuktigast (enligt dagens medel) för en vald plats
        // Väldigt lik SortHotToCold-metoden ovan
        public static void SortDryToHumid(string location)
        {
            using (var db = new WeatherDataContext())
            {
                var data = db.WeatherDataTbl
                                    .Where(w => w.Location == location && w.Humidity.HasValue)
                                    .GroupBy(w => w.Date.Date)                                     // Group by Date --> vill bara få ut en rad per dag
                                    .Select(g => new
                                    {
                                        GroupDate = g.First().Date.Date,                           // .First() --> ger ett resultat
                                        AverageHumidity = g.Average(w => w.Humidity.Value)
                                    })
                                    .OrderBy(h => h.AverageHumidity)                               // Sortera enligt AverageHumidity, torrt till fuktigt
                                    .ToList();

                // Felmeddelande om ingen data hittas
                if (!data.Any())
                {
                    Console.WriteLine($"\nError! No data found for \"{location}\".");
                }
                // Annars, visa medelluftfuktigheten för varje dag
                else
                {
                    foreach (var row in data)
                    {
                        Console.WriteLine($"\nDate: {row.GroupDate:yyyy-MM-dd}, Average humidity: {row.AverageHumidity:F2} %");
                    }
                }
            }
        }
        // Metod för att sortera från minst till störst risk för mögel (enligt dagens medel) för en vald plats
        // Använder medelvärdet för varje dag - som de andra sorteringsmetoderna -
        // men metoden är lite annorlunda eftersom MoldRisk inte är en kolumn i tabellen.
        public static void SortMoldRiskLowToHigh(string location)
        {
            using (var db = new WeatherDataContext())
            {
                // Delar upp det i två steg
                // Steg 1: Hämta data, gruppera per dag och spara i lista
                var dataList = db.WeatherDataTbl
                                    .Where(w => w.Location == location)                  // Behöver inte kolla om MoldRisk har värde (HasValue) eftersom det aldrig är null
                                    .ToList()                                            // Kör ToList redan här för att spara listan i minnet och sen använda MoldRisk-egenskapen
                                    .GroupBy(w => w.Date.Date);                          // Group by Date --> vill bara få ut en rad per dag

                // Steg 2: Beräkna medelvärdet för MoldRisk per dag direkt i C#
                // och spara i ny lista
                var data = dataList
                           .Select(g => new
                           {
                               GroupDate = g.First().Date.Date,
                               AverageMoldRisk = g.Average(w => w.MoldRisk)
                           })
                           .OrderBy(m => m.AverageMoldRisk)                               // Sortera enligt AverageMoldRisk först
                           .ThenBy(m => m.GroupDate)                                      // Sen sortera på datum
                           .ToList();

                // Felmeddelande om ingen data hittas
                if (!data.Any())
                {
                    Console.WriteLine($"\nError! No data found for \"{location}\".");
                }
                // Annars, visa mögelrisken för varje dag
                else
                {
                    foreach (var row in data)
                    {
                        Console.WriteLine($"\nDate: {row.GroupDate:yyyy-MM-dd}, Mold risk: {row.AverageMoldRisk:F2}");
                    }
                }
            }
        }
        // Metod som visar meteorologisk höst
        public static void MeteorologicalAutumn()
        {
            using (var db = new WeatherDataContext())
            {
                // Spara medeltemperaturen per dag i en lista
                var autumnData = db.WeatherDataTbl
                                    .Where(w => w.Location == "Ute" && w.Temperature.HasValue)      // Behöver bara kolla temperaturerna utomhus
                                    .GroupBy(w => w.Date.Date)
                                    .Select(g => new
                                    {
                                        GroupDate = g.First().Date.Date,
                                        AverageTemp = g.Average(w => w.Temperature.Value)
                                    })
                                    .OrderBy(d => d.GroupDate)
                                    .ToList();

                // Initiera variabler för startdatum och räknare (ska vara under 10 grader 5 dagar i rad)
                DateTime? autumnStartDate = null;
                int consecutiveDays = 0;

                // Går igenom listan och kollar när temperaturen är under 10 grader
                foreach (var day in autumnData)
                {
                    if (day.AverageTemp < 10.0)
                    {
                        // Lägger till en dag i räknaren om temperaturen är under 10 grader
                        consecutiveDays++;
                        if (consecutiveDays == 5)
                        {
                            autumnStartDate = day.GroupDate.AddDays(-4);
                            break;
                        }
                    }
                    else
                    {
                        // Nollställer räknaren så fort medeltemperaturen är för varm en dag
                        consecutiveDays = 0;
                    }
                }
                if (autumnStartDate.HasValue)
                {
                    Console.WriteLine($"\nMeteorological autumn started on: {autumnStartDate:yyyy-MM-dd}");
                }
                else
                {
                    Console.WriteLine("\nThe date for meteorological autumn could not be determined.");
                    Console.WriteLine("\nEither meteorological autumn did not start during the provided time frame");
                    Console.WriteLine("or the data was insufficient.");
                }
            }
        }
        // Metod som visar meteorologisk vinter
        // I princip likadan som metoden för att visa meteorologisk höst
        public static void MeteorologicalWinter()
        {
            using (var db = new WeatherDataContext())
            {
                // Spara medeltemperaturen per dag i en lista - samma som för höst
                var winterData = db.WeatherDataTbl
                                    .Where(w => w.Location == "Ute" && w.Temperature.HasValue)      // Behöver bara kolla temperaturerna utomhus
                                    .GroupBy(w => w.Date.Date)
                                    .Select(g => new
                                    {
                                        GroupDate = g.First().Date.Date,
                                        AverageTemp = g.Average(w => w.Temperature.Value)
                                    })
                                    .OrderBy(d => d.GroupDate)
                                    .ToList();

                // Initiera variabler för startdatum och räknare (ska vara under 0,0 grader 5 dagar i rad)
                DateTime? autumnStartDate = null;
                int consecutiveDays = 0;

                // Går igenom listan och kollar när temperaturen är under 10 grader
                foreach (var day in winterData)
                {
                    if (day.AverageTemp < 0.0)
                    {
                        // Lägger till en dag i räknaren om temperaturen är under 10 grader
                        consecutiveDays++;
                        if (consecutiveDays == 5)
                        {
                            autumnStartDate = day.GroupDate.AddDays(-4);
                            break;
                        }
                    }
                    else
                    {
                        // Nollställer räknaren så fort medeltemperaturen är för varm en dag
                        consecutiveDays = 0;
                    }
                }
                // Skriver ut resultatet - ingen meteorologisk vinter hittas för 2016
                if (autumnStartDate.HasValue)
                {
                    Console.WriteLine($"\nMeteorological winter started on: {autumnStartDate:yyyy-MM-dd}");
                }
                else
                {
                    Console.WriteLine("\nThe date for meteorological winter could not be determined.");
                    Console.WriteLine("\nEither meteorological winter did not start during the provided time frame");
                    Console.WriteLine("or the data was insufficient.");
                }
            }
        }
    }
}
