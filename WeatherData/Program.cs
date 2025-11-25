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
        bool breaker = true;
        try
        {
            WDDataAccess.InitializeData(filePath);
            do
            {
                MainMenu(out breaker);
            }
            while (breaker);
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
        breaker = true;
        WritePanel("WEATHER DATA", "#ffffff", "#0087ff");

        List<string> menuOptions = new List<string>
            {
                "[#ffffff]Outside: Average temperature (search by date)[/]\n",
                "[#ffffff]Outside: Sort from hottest to coldest day (average temp)[/]\n",
                "[#ffffff]Outside: Sort from driest to most humid day (average humdity)[/]\n",
                "[#ffffff]Outside: Sort from lowest to highest mold risk[/]\n\n",
                "[#ffffff]Date of meteorological autumn[/]\n",
                "[#ffffff]Date of meteorological winter [/]\n\n",
                "[#ffffff]Inside: Average temperature (search by date)[/]\n",
                "[#ffffff]Inside: Sort from hottest to coldest day (average temp)[/]\n",
                "[#ffffff]Inside: Sort from driest to most humid day (average humdity)[/]\n",
                "[#ffffff]Inside: Sort from lowest to highest mold risk[/]\n\n",
                "[#ffffff]Extra: Sort by how long the balcony door is open[/]\n",
                "[#ffffff]Extra: Sort by temperature difference[/]\n\n",
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
                    WritePanel("AVERAGE TEMP OUTSIDE", "#ffffff", "#0087ff");
                    WDCalculate.AverageTempDay("Ute");
                    break;
                }

            case "Outside: Sort from hottest to coldest day (average temp)":
                {
                    WritePanel("OUTSIDE - HOTTEST TO COLDEST", "#ffffff", "#0087ff");
                    WDCalculate.SortHotToCold("Ute");
                    break;
                }
            case "Outside: Sort from driest to most humid day (average humdity)":
                {
                    WritePanel("OUTISDE - DRIEST TO MOST HUMID", "#ffffff", "#0087ff");
                    WDCalculate.SortDryToHumid("Ute");
                    break;
                }
            case "Outside: Sort from lowest to highest mold risk":
                {
                    WritePanel("OUTSIDE - LOWEST TO HIGHEST MOLD RISK", "#ffffff", "#0087ff");
                    WDCalculate.SortMoldRiskLowToHigh("Ute");
                    break;
                }
            case "Date of meteorological autumn":
                {
                    WritePanel("METEOROLOGICAL AUTUMN", "#ffffff", "#0087ff");
                    WDCalculate.MeteorologicalAutumn();
                    break;
                }
            case "Date of meteorological winter":
                {
                    WritePanel("METEOROLOGICAL WINTER", "#ffffff", "#0087ff");
                    WDCalculate.MeteorologicalWinter();
                    break;
                }
            case "Inside: Average temperature (search by date)":
                {
                    WritePanel("AVERAGE TEMP INSIDE", "#ffffff", "#0087ff");
                    WDCalculate.AverageTempDay("Inne");
                    break;
                }
            case "Inside: Sort from hottest to coldest day (average temp)":
                {
                    WritePanel("INSIDE - HOTTEST TO COLDEST", "#ffffff", "#0087ff");
                    WDCalculate.SortHotToCold("Inne");
                    break;
                }
            case "Inside: Sort from driest to most humid day (average humdity)":
                {
                    WritePanel("INSIDE - DRIEST TO MOST HUMID", "#ffffff", "#0087ff");
                    WDCalculate.SortDryToHumid("Inne");
                    break;
                }
            case "Inside: Sort from lowest to highest mold risk":
                {
                    WritePanel("INSIDE - LOWEST TO HIGHEST MOLD RISK", "#ffffff", "#0087ff");
                    WDCalculate.SortMoldRiskLowToHigh("Inne");
                    break;
                }
            case "Extra: Sort by how long the balcony door is open":
                {
                    WritePanel("BALCONY DOOR OPEN\nLONGEST TO SHORTEST TIME PER DAY", "#ffffff", "#0087ff");
                    WDCalculate.SortBalconyDoorOpen();
                    break;
                }
            case "Extra: Sort by temperature difference":
                {
                    WritePanel("TEMPERATURE DIFFERENCE INSIDE AND OUTSIDE\n\nGREATEST TO SMALLEST DIFFERENCE", "#ffffff", "#0087ff");
                    WDCalculate.SortTemperatureDiff();
                    break;
                }
            case "Finish and close":
                {
                    AnsiConsole.Clear();
                    WritePanel("THANK YOU FOR USING THE WEATHER DATA APP\n\nGOOD BYE!", "#ffffff", "#0087ff");
                    breaker = false;
                    break;
                }
        }
        AnsiConsole.Console.Input.ReadKey(false);
        AnsiConsole.Clear();
    }


    // Metod som skapar paneler (från Prague Parking)
    private static void WritePanel(string panelText, string textColor, string borderColor)
    {
        Panel menuPanel = new Panel(new Markup($"[{textColor} bold]{panelText}[/]").Centered());
        menuPanel.Border = BoxBorder.Heavy;
        menuPanel.BorderColor(Color.FromHex(borderColor));
        menuPanel.Padding = new(2, 1);
        AnsiConsole.Write(menuPanel);
    }
}