using Serilog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using UsedCarsPrice.Common.Models;
using UsedCarsPrice.WebScraper.Services.Scraping;

namespace UsedCarsPrice.WebScraper
{
    class Program
    {
        public static int NewUrlsCount;
        public static int NewUrlsCountThreadSafe;
        public static int UrlsScraped;
        public static ConcurrentDictionary<string, UsedCarModel> UsedCarsUrlsBag;

        static void Main(string[] args)
        {
            NewUrlsCount = 0;
            NewUrlsCountThreadSafe = 0;
            UrlsScraped = 0;
            UsedCarsUrlsBag = new ConcurrentDictionary<string, UsedCarModel>();

            ScrapingServices scrapingService = new ScrapingServices();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("UsedCars-Scraper-logs.txt", outputTemplate: "{Timestamp} [{Level}] {Message}{NewLine}{Exception}").CreateLogger();
            Stopwatch scrapingStopWatch = new Stopwatch();
            Stopwatch globalStopWatch = new Stopwatch();

            try
            {
                Console.WriteLine("STARTING THE PROGRAM *");

                scrapingStopWatch.Start();
                globalStopWatch.Start();

                // 1) Scrape the adverts urls from the websites first
                scrapingService.ScrapeUrlsInParallel(Constants.Counties);
                scrapingStopWatch.Stop();
                string urlsScrapingDuration = string.Format("{0:D2}:{1:D2}:{2:D2}", scrapingStopWatch.Elapsed.Hours, scrapingStopWatch.Elapsed.Minutes, scrapingStopWatch.Elapsed.Seconds);

                // 2) Scrape the content for each advert url
                scrapingStopWatch.Restart();

                scrapingService.ScrapeAdverts(null);

                scrapingStopWatch.Stop();
                globalStopWatch.Stop();

                //Persist the report
                ScrapingServices.PersistScrapingLog(
                    NewUrlsCountThreadSafe,
                    urlsScrapingDuration,
                    UrlsScraped,
                   string.Format("{0:D2}:{1:D2}:{2:D2}", scrapingStopWatch.Elapsed.Hours, scrapingStopWatch.Elapsed.Minutes, scrapingStopWatch.Elapsed.Seconds),
                    string.Format("{0:D2}:{1:D2}:{2:D2}", globalStopWatch.Elapsed.Hours, globalStopWatch.Elapsed.Minutes, globalStopWatch.Elapsed.Seconds),
                    true);

                ScrapingServices.DeleteInvalidLinks();

                Console.WriteLine("Done!");
            }
            catch (Exception e)
            {
                Log.Information("Exception in the main.");
                Log.Debug(e, "{Timestamp} [{Level}] {Message}{NewLine}{Exception}");
            }
        }

    }
}
