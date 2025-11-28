using Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DataAccess
{
    public class WDDataAccess
    {
        public static void InitializeData(string filePath)
        {
            // Kolla först om filen finns - felmeddelande om den inte hittas
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Filen kunde inte hittas.", filePath);
            }

            // Läs av CSV-filen och spara datan i en lista
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            }))
            { 
                // CSV-kolumnerna kopplas till WeatherData properties via mapping
                csv.Context.RegisterClassMap<WeatherDataMap>();

                // Datan sparas som en lista av WeatherData-objekt
                var dataList = csv.GetRecords<WeatherData>().ToList();

                // Se till att databasen skapas om den inte finns
                CreateDatabase(dataList);
            }
        }

        private static void CreateDatabase(List<WeatherData> dataList)
        {
            // Lite felhantering med try-catch
            try
            {
                using (var db = new WeatherDataContext())
                {
                    db.Database.EnsureCreated();

                    //Om det inte finns data i tabellen - fylla på från listan och spara
                    if (!db.WeatherDataTbl.Any())
                    {
                        db.WeatherDataTbl.AddRange(dataList);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initializing database: {ex.Message}");
            }

        }
    }
}
