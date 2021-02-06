using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using UsedCarsPrice.Common.Models;

namespace UsedCarsPrice.WebScraper.Services.Scraping
{
    public abstract class Scraper
    {
        public List<string> BaseUrls { get; set; }

        public abstract UsedCarModel ScrapeAdvert(UsedCarModel car);

        public string FixCarState(string carState)
        {
            if (carState == null)
            {
                return null;
            }

            List<string> usedState = new List<string>() {
                "Second hand", "Used"
            };
            List<string> newState = new List<string>() {
                "New"
            };

            if (usedState.Contains(carState))
            {
                return CarState.Used;
            }
            else if (newState.Contains(carState))
            {
                return CarState.New;
            }

            Log.Error("Count not set stare masina from FixCarState");
            return null;
        }
    }
}
