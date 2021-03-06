﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using UsedCarsPrice.WebScraper.Services.Scraping;
using Serilog;
using System.Threading;
using HtmlAgilityPack;
using UsedCarsPrice.Common.Models;
using UsedCarsPrice.WebScraper.Services.Utils;

namespace UsedCarsPrice.WebScraper.Services.Scraping
{
    public class AutovitScraper : Scraper
    {
        public AutovitScraper()
        {
            BaseUrls = new List<string>() { "www.autovit.ro", "http://autovit.ro" };
        }

        public override UsedCarModel ScrapeAdvert(UsedCarModel usedCar)
        {
            HttpStatusCode status = HttpStatusCode.NotFound;
            string html = HttpUtils.DownloadPage(usedCar.Url, ref status);

            if (status == HttpStatusCode.OK)
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.OptionFixNestedTags = true;
                htmlDoc.LoadHtml(html);

                if (htmlDoc.DocumentNode != null)
                {
                    try
                    {
                        usedCar.Title = htmlDoc.DocumentNode.SelectSingleNode("//h1")?.FirstChild.InnerText;
                        usedCar.Title = HtmlUtils.SanitizeString(usedCar.Title);
                        var descriere = htmlDoc.DocumentNode.Descendants().Where(n => n.Name == "div" && n.HasClass("offer-description__description")).FirstOrDefault()?.InnerText;
                        usedCar.Description = HtmlUtils.SanitizeString(descriere);

                        //Features
                        usedCar.OferitDe = ScrapeAdvertFeature("Oferit de", htmlDoc);
                        usedCar.Model = ScrapeAdvertFeature("Model", htmlDoc);
                        usedCar.Brand = ScrapeAdvertFeature("Brand", htmlDoc);
                        usedCar.Year = ScrapeAdvertFeature("Anul fabricatiei", htmlDoc);
                        usedCar.Fuel = ScrapeAdvertFeature("Fuel", htmlDoc);
                        usedCar.Gearbox = ScrapeAdvertFeature("Cutie de viteze", htmlDoc);
                        usedCar.Body = ScrapeAdvertFeature("Body", htmlDoc);
                        usedCar.Color = ScrapeAdvertFeature("Color", htmlDoc);
                        usedCar.Stare = FixCarState(ScrapeAdvertFeature("Stare", htmlDoc));

                        float capacitateMotorTemp;
                        bool capacitateWasParsed = float.TryParse(ScrapeAdvertFeature("Capacitate cilindrica", htmlDoc, removeNonNumericCharacters: true, capacitateMotor: true), out capacitateMotorTemp);
                        if (capacitateWasParsed)
                        {
                            usedCar.EngineCapacity = capacitateMotorTemp;
                        }

                        float rulajTemp;
                        bool rulajWasParsed = float.TryParse(ScrapeAdvertFeature("Km", htmlDoc, removeNonNumericCharacters: true), out rulajTemp);
                        if (rulajWasParsed)
                        {
                            usedCar.Mileage = rulajTemp;
                        }

                        //Price
                        string pret = htmlDoc.DocumentNode.Descendants()
                            .Where(n => n.Name == "span" && n.HasClass("offer-price__number"))
                            .FirstOrDefault()?.InnerText;
                        pret = HtmlUtils.RemoveNonNumericCharacters(pret);
                        float pretTemp;
                        bool pretWasParsed = float.TryParse(pret, out pretTemp);
                        if (pretWasParsed)
                        {
                            usedCar.Price = pretTemp;
                        }

                        usedCar.Scraped = (int)ProcessingStatus.Processed;
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e, "Error trying to scrape " + usedCar.Url);
                        return usedCar;
                    }
                }
            }
            else
            {
                usedCar.Scraped = (int)ProcessingStatus.Invalid;
            }
            usedCar.LastModified = DateTime.Now;

            Interlocked.Increment(ref Program.UrlsScraped);

            return usedCar;
        }

        public string ScrapeAdvertFeature(string label, HtmlDocument htmlDoc, bool sanitizeString = true, bool removeNonNumericCharacters = false, bool capacitateMotor = false)
        {
            string value = null;
            var oferitDe = htmlDoc.DocumentNode.SelectNodes("//li").Where(n => n.HasChildNodes && n.ChildNodes.Where(c => c.Name == "span" && c.InnerText.Contains(label)).FirstOrDefault() != null).FirstOrDefault();
            if (oferitDe != null)
            {
                var valueNode = oferitDe.ChildNodes.Where(n => n.Name == "div" && n.HasClass("offer-params__value")).FirstOrDefault();
                var link = valueNode.ChildNodes.Where(n => n.Name == "a").FirstOrDefault();
                if (link != null)
                {
                    value = link.InnerText;
                }
                else
                {
                    value = valueNode.InnerText;
                }

                if (capacitateMotor)
                {
                    value = value.Replace("cm3", "");
                }

                if (sanitizeString)
                {
                    value = HtmlUtils.SanitizeString(value);
                }

                if (removeNonNumericCharacters)
                {
                    value = HtmlUtils.RemoveNonNumericCharacters(value);
                }
            }

            return value;
        }
    }
}
