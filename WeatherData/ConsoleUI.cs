using DataAccess;
using Spectre.Console;
using System.Globalization;

namespace WeatherData.UI
{
    // En klass med metoder som sköter utskrifter till konsolen och interaktion med användaren
    public class ConsoleUI
    {
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
        // Metoder som sköter utskrifter
        public static void UIAverageTempDay(string location)
        {
            DateTime? date = SelectDate();
            if (date == null)
            {
                Console.WriteLine("\nAn error has occured. Returning to main menu.");
                return;
            }
            string result = WDCalculate.AverageTempDay(location, date);
            Console.WriteLine(result);
        }
        public static void UISortHotToCold(string location) 
        {
            var list = WDCalculate.SortHotToCold(location);

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }
        public static void UISortDryToHumid(string location) 
        {
            var list = WDCalculate.SortDryToHumid(location);

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }
        public static void UISortMoldRisk(string location) 
        {
            var list = WDCalculate.SortMoldRiskLowToHigh(location);

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }
        public static void UIMeteorologicalSeason(string season) 
        {
            string result = string.Empty;
            if (season.ToLower() == "autumn")
            {
                result = WDCalculate.MeteorologicalAutumn();
            }
            else if (season.ToLower() == "winter")
            {
                result = WDCalculate.MeteorologicalWinter();
            }

            Console.WriteLine(result);
        }
        public static void UISortBalconyDoor()
        {
            var list = WDCalculate.SortBalconyDoorOpen();

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }
        public static void UISortTemperatureDiff()
        {
            var list = WDCalculate.SortTemperatureDiff();
            // Lite utrskrifter som förklarar för användaren
            // Stört och minst skillnad --> första och sista elementen i listan
            Console.WriteLine($"\nPlease note: The method uses average temperature per day (inside and outside)\n");
            Console.WriteLine($"-----------------------------------------------");
            Console.WriteLine($"\nGreatest difference: {list[0]}\n");
            Console.WriteLine($"Smallest difference: {list[list.Count - 1]}\n");
            Console.WriteLine($"-----------------------------------------------\n\n");
            Console.WriteLine("All days sorted from greatest to smallest difference:\n");

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }
    }
}
