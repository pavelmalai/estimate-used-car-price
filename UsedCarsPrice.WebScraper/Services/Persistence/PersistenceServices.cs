using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UsedCarsPrice.Core.Models;

namespace UsedCarsPrice.WebScraper.Services.Utils
{
    public class PersistenceServices
    {
        public static void InsertUrls(HashSet<string> urls)
        {
            using (UsedCarsDbContext db = new UsedCarsDbContext())
            {
                foreach (var url in urls)
                {
                    if (!db.Usedcars.Any(l => l.Url == url))
                    {
                        db.Usedcars.Add(new UsedCarModel()
                        {
                            Url = url,
                            Scraped = 0
                        });

                        Interlocked.Increment(ref Program.NewUrlsCountThreadSafe);
                    }
                }

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Log.Debug(e, "Error trying to insert URLs.");
                }
            }
        }
    }
}
