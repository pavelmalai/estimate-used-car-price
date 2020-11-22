using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UsedCarsPrice.Core.Models;
using UsedCarsPrice.WebScraper.Services.Utils;

namespace UsedCarsPrice.WebScraper.Services.Scraping
{
    public class ScrapingServices
    {
        public static IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json").Build();

        private int _numberOfThreads;
        private int _chunkSize;
        MySqlConnection _conn;

        public ScrapingServices()
        {
            _numberOfThreads = Convert.ToInt32(config["NumberOfThreads"]);
            _chunkSize = Convert.ToInt32(config["ChunkSize"]);
            _conn = DBUtils.GetDBConnection("ConnectionString");
        }


        public static void ScrapeOlxUrls()
        {
            for (int page = 2; page <= 500; page++)
            {
                HashSet<string> urls = new HashSet<string>();

                string olxPage = string.Format(Constants.OlxBaseSearchUrl, page);
                HttpStatusCode status = HttpStatusCode.NotFound;
                string html = HttpUtils.DownloadPage(olxPage, ref status);

                if (status == HttpStatusCode.OK)
                {
                    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    htmlDoc.OptionFixNestedTags = true;
                    htmlDoc.LoadHtml(html);

                    // ParseErrors is an ArrayList containing any errors from the Load statement
                    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                    {
                        Log.Debug("Error trying to parse content from ScrapeOlxUrls(), Page : " + olxPage + ", errors : " + htmlDoc.ParseErrors);
                        Console.WriteLine("Error trying to parse content from ScrapeOlxUrls(), Page : " + olxPage + ", errors : " + htmlDoc.ParseErrors);
                    }
                    else
                    {
                        if (htmlDoc.DocumentNode != null)
                        {
                            var nodes = htmlDoc.DocumentNode.SelectNodes("//a");
                            var links = nodes.Where(n => n.HasClass("detailsLink") || n.HasClass("detailsLinkPromoted")).Distinct();

                            foreach (var link in links)
                            {
                                string cleanedURL = HtmlUtils.RemoveAnchorFromURL(link.GetAttributeValue("href", null));
                                urls.Add(cleanedURL);
                            }
                        }
                    }
                }
                else
                {
                    continue;
                }

                PersistenceServices.InsertUrls(urls);
            }
        }

        public static int ScrapeOlxURLByCounty()
        {
            int NewURLsCount = 0;
            foreach (string judet in Constants.Counties)
            {
                Log.Information("Scraping URLs for " + judet);
                for (int page = 2; page <= 500; page++)
                {
                    string url = string.Format(Constants.OlxBaseSearchUrlByCounty, judet, page);

                    HashSet<string> urls = new HashSet<string>();

                    HttpStatusCode status = HttpStatusCode.NotFound;
                    string html = HttpUtils.DownloadPage(url, ref status);

                    if (status != HttpStatusCode.OK)
                    {
                        break;
                    }
                    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    htmlDoc.OptionFixNestedTags = true;
                    htmlDoc.LoadHtml(html);

                    // ParseErrors is an ArrayList containing any errors from the Load statement
                    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                    {
                        Log.Debug("Error trying to parse content " + htmlDoc.ParseErrors);
                        Console.WriteLine("Error trying to parse content " + htmlDoc.ParseErrors);
                    }
                    else
                    {
                        if (htmlDoc.DocumentNode != null)
                        {
                            var nodes = htmlDoc.DocumentNode.SelectNodes("//a");
                            var links = nodes.Where(n => n.HasClass("detailsLink") || n.HasClass("detailsLinkPromoted")).Distinct();

                            foreach (var link in links)
                            {
                                string cleanedURL = HtmlUtils.RemoveAnchorFromURL(link.GetAttributeValue("href", null));
                                urls.Add(cleanedURL);
                            }
                        }
                    }

                    PersistenceServices.InsertUrls(urls);
                }
            }

            return NewURLsCount;
        }

        public static void ScrapeAdverts(Scraper scraper)
        {
            using (UsedCarsDbContext db = new UsedCarsDbContext())
            {
                while (true)
                {
                    List<UsedCarModel> cars = db.Usedcars.Where(c => c.Scraped == (int)ProcessingStatus.Unprocessed && scraper.BaseUrls.Any(u => c.Url.Contains(u)))
                        .Take(1000).ToList();

                    if (cars == null || cars.Count == 0)
                    {
                        break;
                    }

                    for (int i = 0; i < cars.Count; i++)
                    {
                        UsedCarModel tempCar = cars.ElementAt(i);

                        scraper.ScrapeAdvert(tempCar);
                    }
                    db.SaveChanges();
                }
            }
        }

        public static ConcurrentDictionary<string, UsedCarModel> ScrapeURLsByJudeteAsync(List<string> judete, int limit = 0)
        {
            Program.UsedCarsUrlsBag = new ConcurrentDictionary<string, UsedCarModel>();

            Log.Information("Scraping from thread " + Thread.CurrentThread.ManagedThreadId);
            foreach (string judet in judete)
            {
                Log.Information("Scraping URLs for async from thread " + Thread.CurrentThread.ManagedThreadId + " " + judet);
                for (int page = 2; page <= 500; page++)
                {

                    if (limit != 0 && Program.UsedCarsUrlsBag.Count > limit)
                    {
                        return Program.UsedCarsUrlsBag;
                    }

                    //Log.Information("Scraping URLs for async from thread " + Thread.CurrentThread.ManagedThreadId + " " + judet + " page " + page);
                    string url = string.Format(Constants.OlxBaseSearchUrlByCounty, judet, page);

                    HttpStatusCode status = HttpStatusCode.NotFound;
                    string html = HttpUtils.DownloadPage(url, ref status);

                    if (status != HttpStatusCode.OK)
                    {
                        break;
                    }
                    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    htmlDoc.OptionFixNestedTags = true;
                    htmlDoc.LoadHtml(html);

                    // ParseErrors is an ArrayList containing any errors from the Load statement
                    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                    {
                        Log.Debug("Error trying to parse content " + htmlDoc.ParseErrors);
                        Console.WriteLine("Error trying to parse content " + htmlDoc.ParseErrors);
                    }
                    else
                    {
                        if (htmlDoc.DocumentNode != null)
                        {
                            var nodes = htmlDoc.DocumentNode.SelectNodes("//a");
                            var links = nodes.Where(n => n.HasClass("detailsLink") || n.HasClass("detailsLinkPromoted")).Distinct();

                            foreach (var link in links)
                            {
                                string cleanedURL = HtmlUtils.RemoveAnchorFromURL(link.GetAttributeValue("href", null));
                                Program.UsedCarsUrlsBag.TryAdd(cleanedURL, new UsedCarModel()
                                {
                                    Url = cleanedURL,
                                    Scraped = (int)ProcessingStatus.Unprocessed
                                });
                            }
                        }
                    }
                }
            }

            Log.Information("Finished scraping from thread " + Thread.CurrentThread.ManagedThreadId);

            return Program.UsedCarsUrlsBag;
        }

        public void ScrapeUrlsInParallel(List<string> counties)
        {
            List<IEnumerable<string>> chunks = counties.Split(_numberOfThreads).ToList();
            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = _numberOfThreads };
            Parallel.ForEach(chunks, options, (chunk) =>
             {
                 ScrapeURLsByJudeteAsync(chunk.ToList());
             });

            InsertNewUrls(Program.UsedCarsUrlsBag);
        }

        public void InsertNewUrls(ConcurrentDictionary<string, UsedCarModel> bag)
        {
            try
            {
                var chunks = bag.Values.ToList().SplitListByChunkSize(_chunkSize).ToList();
                ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = _numberOfThreads };
                Parallel.ForEach(chunks, options, c => ProcessChunkOfUrls(c));
            }
            catch (Exception e)
            {
                Log.Information("Exception in InsertNewUrls(ConcurrentBag<UsedCarModel> bag)");
                Log.Debug(e, "{Timestamp} [{Level}] {Message}{NewLine}{Exception}");
            }
        }

        public void ProcessChunkOfUrls(List<UsedCarModel> chunk)
        {
            chunk = chunk.GroupBy(c => c.Url).Select(g => g.First()).ToList();
            using (var conn = DBUtils.GetDBConnection())
            {
                List<UsedCarModel> newUsedCars = new List<UsedCarModel>();
                foreach (var usedcar in chunk)
                {
                    if (!UrlExists(usedcar.Url, conn))
                    {
                        newUsedCars.Add(usedcar);
                    }
                }

                Interlocked.Exchange(ref Program.NewUrlsCountThreadSafe, Program.NewUrlsCountThreadSafe + newUsedCars.Count);
                BulkInsert(newUsedCars);
            }
        }

        public bool UrlExists(string URL, MySqlConnection conn)
        {
            string sql = @"SELECT id FROM usedcars WHERE url = @url";
            bool exists = conn.ExecuteScalar<bool>(sql, new { url = URL });
            return exists;
        }

        public void BulkInsert(List<UsedCarModel> cars)
        {
            try
            {
                using (UsedCarsDbContext db = new UsedCarsDbContext())
                {
                    db.Usedcars.AddRange(cars);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception in BulkInsert", e);
            }
        }

        public static void DeleteInvalidLinks()
        {
            try
            {
                using (UsedCarsDbContext db = new UsedCarsDbContext())
                {
                    db.Usedcars.RemoveRange(db.Usedcars.Where(c => c.Scraped == (int)ProcessingStatus.Invalid));
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception in DeleteInvalidLinks", e);
            }
        }

        public static void PersistScrapingLog(int newUrlsAdded, string urlsScrapingDuration, int newArticlesScraped, string articlesScrapingDuration, string totalDuration, bool sendEmail)
        {
            using (UsedCarsDbContext db = new UsedCarsDbContext())
            {
                int totalValidRecords = db.Usedcars.Count(c => c.Scraped == (int)ProcessingStatus.Processed && c.Marca != null && c.Caroserie != null && c.CutieDeViteze != null && c.Combustibil != null && c.CapacitateMotor != null && c.Rulaj != null && c.Pret != null);

                db.Scrapinglogs.Add(new ScrapingLogs()
                {
                    CreatedOn = DateTime.Now,
                    NewUrlsAdded = newUrlsAdded,
                    UrlScrapingDuration = urlsScrapingDuration,
                    NewArticlesScraped = newArticlesScraped,
                    ArticlesScrapingDuration = articlesScrapingDuration,
                    TotalDuration = totalDuration,
                    TotalValidRecords = totalValidRecords
                });
                db.SaveChanges();

                if (sendEmail)
                {
                    string body = "New URLs scraped: " + newUrlsAdded + ", duration: " + urlsScrapingDuration + Environment.NewLine +
                        "New articles scraped " + newArticlesScraped + ", duration: " + articlesScrapingDuration + Environment.NewLine +
                        "Total time: " + totalDuration + Environment.NewLine +
                        "Total valid records: " + totalValidRecords;
                    CommonUtils.SendEmail(config["EmailTo"], "Scraped cars report", body);
                }
            }
        }

        public IEnumerable<UsedCarModel> GetUsedOlxCars(int? ammountParam)
        {
            Guid groupId = Guid.NewGuid();
            string sql = @"SELECT * FROM usedcars WHERE scraped = 0 AND url LIKE '%olx.ro%'";
            if (ammountParam != null)
            {
                sql = sql + " LIMIT @ammount;";
            }
            var result = _conn.Query<UsedCarModel>(sql, new { ammount = ammountParam }).ToList();
            return result;
        }

        public IEnumerable<UsedCarModel> GetUsedAutovitCars(int? ammountParam)
        {
            Guid groupId = Guid.NewGuid();
            var sql = @"SELECT *  FROM usedcars WHERE scraped = 0 AND (url LIKE '%www.autovit.ro%' OR url LIKE '%http://autovit.ro%');";
            if (ammountParam != null)
            {
                sql = sql + " LIMIT @ammount;";
            }
            var result = _conn.Query<UsedCarModel>(sql, new { ammount = ammountParam }).ToList();
            return result;
        }

        public void ScrapeAdverts(int? size)
        {
            List<UsedCarModel> usedCarsOlx = GetUsedOlxCars(size).ToList();
            List<UsedCarModel> usedCarsAutovit = GetUsedAutovitCars(size).ToList();

            var chunksOlx = CommonUtils.Split(usedCarsOlx, _chunkSize);
            var chunksAutovit = CommonUtils.Split(usedCarsAutovit, _chunkSize);
            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = _numberOfThreads };

            try
            {
                Parallel.ForEach(chunksOlx, options, chunk => ScrapeAdvertsChunk(chunk, new OlxScraper()));
                Parallel.ForEach(chunksAutovit, options, chunk => ScrapeAdvertsChunk(chunk, new AutovitScraper()));

            }
            catch (Exception e)
            {
                Log.Information("Exception in InsertNewUrls(ConcurrentBag<UsedCarModel> bag)");
                Log.Debug(e, "{Timestamp} [{Level}] {Message}{NewLine}{Exception}");
            }

        }

        public void ScrapeAdvertsChunk(List<UsedCarModel> chunk, Scraper scraper)
        {
            for (int i = 0; i < chunk.Count; i++)
            {
                chunk[i] = scraper.ScrapeAdvert(chunk[i]);
            }

            BulkUpdateUsedCars(chunk);
        }

        public void BulkUpdateUsedCars(List<UsedCarModel> cars)
        {
            using (UsedCarsDbContext db = new UsedCarsDbContext())
            {
                //db.UsedCarModel.AttachRange(cars);
                db.Usedcars.UpdateRange(cars);
                db.SaveChanges();
            }
        }
    }
}
