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
        // Metod för att beräkna medeltemperaturen för vald dag och plats
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
                                    && w.Date < date.Value.Date.AddDays(1)      // .Date används för att strunta i den exakta tiden
                                    && w.Location == location
                                    && w.Temperature.HasValue);

                // Om ingen data hittas för vald datum och plats
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
            DateTime? date = null;      // Om null skickas tillbaka - då har nåt gått fel
            do
            {
                string dateInput = AnsiConsole.Ask<string>("\nEnter a date between 2016-10-01 and 2016-11-30 (yyyymmdd): ");
                date = DateTime.ParseExact(dateInput, "yyyyMMdd", CultureInfo.InvariantCulture);

                // Validera datumet som användaren skriver in
                if (date < new DateTime(2016, 10, 1) || date > new DateTime(2016, 11, 30))
                {
                    Console.WriteLine("\n\nInvalid date. Please enter a date between 2016-10-01 and 2016-11-30.\n");
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
        // Använder medelvärdet för varje dag - som de flesta andra sorteringsmetoderna -
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
                            autumnStartDate = day.GroupDate.AddDays(-4);        // När vi har hittat fem dagar i rad --> startdatum var fyra dagar tidigare
                            break;
                        }
                    }
                    else
                    {
                        // Nollställer räknaren så fort medeltemperaturen är för varm en dag
                        consecutiveDays = 0;
                    }
                }
                // Skriv ut resultatet
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

                // Går igenom listan och kollar när temperaturen är under 0 grader
                foreach (var day in winterData)
                {
                    if (day.AverageTemp < 0.0)
                    {
                        // Plussar på räknaren om temperaturen är under 0 grader
                        consecutiveDays++;
                        if (consecutiveDays == 5)
                        {
                            autumnStartDate = day.GroupDate.AddDays(-4);        // När vi har hittat fem dagar i rad --> då vet vi startdatum
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
        // Metod för att sortera på hur länge balkongdörren är öppen
        // Använder genomsnittliga temperaturskillnaden per dag (inne och ute) 
        // och kollar när skillnaden är tydligt mindre --> då är dörren öppen.
        // Räknar hur många gånger per dag man kan anta att dörren är öppen
        // --> en gång motsvarar en minut --> ger en uppskattning av hur många minuter dörren är öppen per dag
        public static void SortBalconyDoorOpen()
        {
            using (var db = new WeatherDataContext())
            {
                // Först ska vi kolla genomsnittliga temperaturskillnaden per dag
                // Samma kod som i SortTemperatureDiff-metoden
                var tempDiffAvg = db.WeatherDataTbl
                                    .Where(w => w.Temperature.HasValue && !string.IsNullOrEmpty(w.Location))
                                    .GroupBy(w => w.Date.Date)                                                                   // Gruppera per dag
                                    .Select(g => new
                                    {
                                        GroupDate = g.First().Date.Date,
                                        AvgInsideTemp = g.Where(w => w.Location == "Inne").Average(w => w.Temperature.Value),    // Räkna ut medeltemperaturer per dag
                                        AvgOutsideTemp = g.Where(w => w.Location == "Ute").Average(w => w.Temperature.Value)
                                    })
                                    .Select(d => new
                                    {
                                        GroupDate = d.GroupDate,
                                        TempDifference = Math.Abs(d.AvgInsideTemp - d.AvgOutsideTemp)                            // Räkna ut temperaturskillnaden, genomsnitt per dag
                                    })
                                    .ToList();

                // Sen ska vi kolla temp-skillnaden varje minut där vi kan
                var tempDiffPerMinute = db.WeatherDataTbl
                                      .Where(w => w.Temperature.HasValue && (w.Location == "Inne" || w.Location == "Ute"))    // Måste se till att temp. och plats finns
                                      .GroupBy(w => w.Date.Date)                                                              // Gruppera per dag igen
                                      .Select(g => new                                                                        // Dagarna blir nu nya objekt
                                      {
                                          Date = g.Key,
                                          DiffPerMinute = g
                                              .GroupBy(x => x.Date)                                                           // Gruppera per exakt tidpunkt (per minut)
                                              .Where(t => t.Count() == 2)                                                     // där vi har mätningar för både inne och ute
                                              .Select(t => Math.Abs(
                                                  t.First(x => x.Location == "Inne").Temperature.GetValueOrDefault() -        // Fick nullrelaterade fel här - copilot föreslog GetValueOrDefault
                                                  t.First(x => x.Location == "Ute").Temperature.GetValueOrDefault()           // Det kan bli felberäkningar, men beräkningarna 
                                                  ))                                                                          // är ju inte det viktigaste med den här uppgiften
                                      })
                                      .ToList();

                // När vi har båda listorna kan vi jämföra dem
                // Antar att skilladen är minst 1 grad mindre än genomsnittet när dörren är öppen
                var comparison = tempDiffAvg
                                 .Join(tempDiffPerMinute,
                                       avg => avg.GroupDate,
                                       perMin => perMin.Date,
                                       (avg, perMin) => new
                                       {
                                           Date = avg.GroupDate,
                                           AvgDiff = avg.TempDifference,
                                           OpenMinutes = perMin.DiffPerMinute.Count(diff => diff < avg.TempDifference - 1.0)   // Fick hjälp av copilot med den här raden
                                                                                                                               // --> räknar hur många gånger temp-skillnaden 
                                       })                                                                                      // är mindre än genomsnittet per dag
                                 .OrderByDescending(x => x.OpenMinutes)                                                        // Sortera från dörren öppen längst till kortast tid
                                 .ToList();                                                                                    // och spara i lista

                // Slutligen - skriv ut resultatet
                foreach (var day in comparison)
                {
                    Console.WriteLine($"\nDate: {day.Date:yyyy-MM-dd}, Door estimated open: {day.OpenMinutes} minutes");
                }

            }
        }

        // Metod för att sortera på temperaturskillnad inne och ute
        // Sorterar alla dagar enligt temperaturskillnad (medeltemperatur per dag)
        // Skriver ut alla dagar, och när det skiljt sig mest och minst
        public static void SortTemperatureDiff()
        {
            using (var db = new WeatherDataContext())
            {
                var data = db.WeatherDataTbl
                                    .Where(w => w.Temperature.HasValue)
                                    .GroupBy(w => w.Date.Date)              // Gruppera per dag
                                    .Select(g => new
                                    {
                                        GroupDate = g.First().Date.Date,                                                            // Vi vill fortfarande bara ha datum - inte exakt tid
                                        AvgInsideTemp = g.Where(w => w.Location == "Inne").Average(w => w.Temperature.Value),       // Räkna ut medeltemperaturer per dag
                                        AvgOutsideTemp = g.Where(w => w.Location == "Ute").Average(w => w.Temperature.Value)
                                    })
                                    .Select(d => new
                                    {
                                        GroupDate = d.GroupDate,
                                        TempDifference = Math.Abs(d.AvgInsideTemp - d.AvgOutsideTemp)       // Räkna ut temperaturskillnaden
                                    })
                                    .OrderByDescending(t => t.TempDifference)                               // Sortera från störst till minst temperaturskillnad
                                    .ToList();                                                              // Spara i lista

                // Felmeddelande om ingen data hittas - listan är tom
                if (!data.Any())
                {
                    Console.WriteLine($"\nError! Could not determine temperature difference for any days.");
                }

                // Skriv ut när tempen skiljt sig mest och minst - första och sista elementen i listan
                Console.WriteLine($"\nPlease note: The method uses average temperature per day (inside and outside)\n");
                Console.WriteLine($"-----------------------------------------------");
                Console.WriteLine($"\nGreatest difference: {data.First().TempDifference:F2} °C, Date: {data.First().GroupDate:yyyy-MM-dd}");
                Console.WriteLine($"\nSmallest difference: {data.Last().TempDifference:F2} °C, Date: {data.Last().GroupDate:yyyy-MM-dd}\n");
                Console.WriteLine($"-----------------------------------------------\n\n");
                Console.WriteLine("All days sorted from greatest to smallest difference:\n");

                // Skriv ut alla dagar i listan
                foreach (var day in data)
                {
                    Console.WriteLine($"\nDate: {day.GroupDate:yyyy-MM-dd}, Difference: {day.TempDifference:F2} °C");
                }
            }
        }
    }
}
