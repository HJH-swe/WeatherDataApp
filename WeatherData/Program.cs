using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Runtime.CompilerServices;
using WeatherData;

public class Program
{
    private const string filePath = "../../../TempFuktData.csv";

    private static void Main(string[] args)
    {
        bool menuBreaker = true;
        try
        {
            WDDataAccess.InitializeData(filePath);
            do
            {
                MainMenu(out bool breaker);
            }
            while (menuBreaker);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Oväntat fel!");
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private static void MainMenu(out bool breaker)
    {
        WritePanel("WEATHER DATA", "#ffffff", "#0087ff");

        List<string> menuOptions = new List<string>
            {
                "[#ffffff]Outside: Average temperature (search by date)[/]\n",
                "[#ffffff]Outside: Sort from warmest to coldest day (average temp)[/]\n",
                "[#ffffff]Outside: Sort from driest to most humid day (average humdity)[/]\n",
                "[#ffffff]Outside: Sort from lowest to highest mold risk[/]\n\n",
                "[#ffffff]Date of meteorological autumn[/]\n",
                "[#ffffff]Date of meteorological winter [/]\n\n",
                "[#ffffff]Inside: Average temperature (search by date)[/]\n",
                "[#ffffff]Inside: Sort from warmest to coldest day (average temp)[/]\n",
                "[#ffffff]Inside: Sort from driest to most humid day (average humdity)[/]\n",
                "[#ffffff]Inside: Sort from lowest to highest mold risk[/]\n\n",
                "[#ffffff]Finish and close[/]\n"
            };

        string menuSelect = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("[#0087ff]\n\nSelect in the menu using the arrow keys\n[/]")
            .AddChoices(menuOptions)
            );

        string cleanSelect = Markup.Remove(menuSelect).Trim();
        AnsiConsole.Clear();

        switch (cleanSelect)
        {
            case "Outside: Average temperature (search by date)":
                {
                    AverageTempDay("Ute");
                    break;
                }

            case "Outside: Sort from warmest to coldest day (average temp)":
                {
                    break;
                }
            case "Outside: Sort from driest to most humid day (average humdity)":
                {
                    break;
                }
            case "Outside: Sort from lowest to highest mold risk":
                {
                    break;
                }
            case "Date of meteorological autumn":
                {
                    break;
                }
            case "Date of meteorological winter":
                {
                    break;
                }
            case "Inside: Average temperature (search by date)":
                {
                    break;
                }
            case "Inside: Sort from warmest to coldest day (average temp)":
                {
                    break;
                }
            case "Inside: Sort from driest to most humid day (average humdity)":
                {
                    break;
                }
            case "Inside: Sort from lowest to highest mold risk":
                {
                    break;
                }
            case "Finish and close":
                {
                    AnsiConsole.Clear();
                    WritePanel("THANK YOU FOR USING WEATHER DATA", "#00ff00", "#0087ff");
                    breaker = false;
                    break;
                }
        }
    }

    private static void AverageTempDay(string location)
    {
        DateTime? date = SelectDate();
        if (date == null)
        {
            Console.WriteLine("\nAn error has occured. Returning to main menu.");
            return;
        }

        using (var db = new WeatherDataContext)
        {

        }

    }

    private static DateTime? SelectDate()
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
    // Metod från Prague Parking
    private static void WritePanel(string panelText, string textColor, string borderColor)
    {
        Panel menuPanel = new Panel(new Markup($"[{textColor} bold]{panelText}[/]").Centered());
        menuPanel.Border = BoxBorder.Heavy;
        menuPanel.BorderColor(Color.FromHex(borderColor));
        menuPanel.Padding = new(2, 1);
        AnsiConsole.Write(menuPanel);
    }
}