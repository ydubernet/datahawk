using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace amazon_scraper
{
    public class Program
    {
        private const string DB_DATA_SOURCE_CONNECTION_STRING = "DataSource=AmazonScraper.db";

        public static void Main(string[] args)
        {
            CheckDb();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void CheckDb()
        {
            string stm = "SELECT SQLITE_VERSION()";

            using var con = new SQLiteConnection(DB_DATA_SOURCE_CONNECTION_STRING);
            con.Open();

            using var cmd = new SQLiteCommand(stm, con);
            string version = cmd.ExecuteScalar().ToString();
            //Review (asin, date, title, rating, content)
            string createTableIfNotExists = @"CREATE TABLE IF NOT EXISTS Review
                                            (
                                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                Asin VARCHAR(15) NOT NULL,
                                                Date DATE,
                                                Title VARCHAR(100) NOT NULL,
                                                Rating DOUBLE DEFAULT 0,
                                                Content VARCHAR(20000)
                                            );";

            using var cmdCreate = new SQLiteCommand(createTableIfNotExists, con);
            cmdCreate.ExecuteScalar();

            Console.WriteLine($"SQLite version: {version}");
        }
    }
}
