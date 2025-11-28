using Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    // Sätter upp Contextklassen som i inspelade föreläsningen
    public class WeatherDataContext : DbContext
    {
        private const string connectionString =
            "Server=(localdb)\\mssqllocaldb;Database=WeatherDataDB;Trusted_Connection=True;";

        public DbSet<WeatherData> WeatherDataTbl { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
