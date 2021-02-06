using System.Linq;
using UsedCarsPrice.Core.Models;
using UsedCarsPrice.WebScraper.Services.Scraping;
using Xunit;

namespace UsecCarsUnitTest
{
    public class ScraperTest
    {
        [Fact]
        public void ScrapeOlxAdvertTest()
        {
            string advertUrl = "https://www.olx.ro/oferta/ford-focus-2-IDdIHiF.html#dfdfbbd7c5;promoted";
            Scraper scraper = new OlxScraper();
            scraper.ScrapeAdvert(new UsedCarModel()
            {
                Url = advertUrl
            });
        }
    }
}
