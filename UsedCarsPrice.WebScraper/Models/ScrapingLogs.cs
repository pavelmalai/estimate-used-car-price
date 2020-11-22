using System;
using System.Collections.Generic;

namespace UsedCarsPrice.Common.Models
{
    public class ScrapingLogs
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int NewUrlsAdded { get; set; }
        public string UrlScrapingDuration { get; set; }
        public int NewArticlesScraped { get; set; }
        public string ArticlesScrapingDuration { get; set; }
        public string TotalDuration { get; set; }
        public int TotalValidRecords { get; set; }
    }
}
