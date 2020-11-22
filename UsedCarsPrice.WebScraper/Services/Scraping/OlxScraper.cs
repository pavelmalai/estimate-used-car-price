using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using Serilog;
using System.Threading;
using HtmlAgilityPack;
using UsedCarsPrice.Core.Models;
using UsedCarsPrice.WebScraper.Services.Utils;

namespace UsedCarsPrice.WebScraper.Services.Scraping
{
    public class OlxScraper : Scraper
    {
        public OlxScraper()
        {
            BaseUrls = new List<string>() { "olx.ro" };
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

                // ParseErrors is an ArrayList containing any errors from the Load statement
                if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                {
                    Log.Debug("Error trying to scrape " + usedCar.Url);
                    foreach (var error in htmlDoc.ParseErrors)
                    {
                        Log.Debug(error.ToString());
                    }
                }

                usedCar = ParseDocument(usedCar, htmlDoc, html);

                if (htmlDoc.DocumentNode != null)
                {

                    usedCar.Scraped = (int)ProcessingStatus.Processed;
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

        private UsedCarModel ParseDocument(UsedCarModel usedCar, HtmlDocument htmlDoc, string html)
        {
            try
            {
                var pret = htmlDoc.DocumentNode.Descendants("strong").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("pricelabel__value")).FirstOrDefault()?.InnerText;
                if (pret != null)
                {
                    float pretParsed;
                    bool parsed = float.TryParse(HtmlUtils.RemoveNonNumericCharacters(pret), out pretParsed);
                    if (parsed)
                    {
                        usedCar.Pret = pretParsed;
                    }
                }

                //an fabricatie
                var anDeFabricatie = GetDetail(htmlDoc, "An de fabricatie");
                usedCar.AnFabricatie = anDeFabricatie;

                usedCar.Titlu = HtmlUtils.RemoveNewLineCharacters(htmlDoc.DocumentNode.SelectSingleNode("//h1")?.InnerText);
                usedCar.Descriere = HtmlUtils.RemoveNewLineCharacters(htmlDoc.GetElementbyId("textContent")?.InnerText);

                var oferitDe = GetDetail(htmlDoc, "Oferit de");
                usedCar.OferitDe = oferitDe;

                //model
                var model = GetDetail(htmlDoc, "Model");
                usedCar.Model = model;

                //combustibil
                var combustibil = GetDetail(htmlDoc, "Combustibil");
                usedCar.Combustibil = combustibil = GetDetail(htmlDoc, "Combustibil");

               


                //caroserie
                var caroserie = GetDetail(htmlDoc, "Caroserie");
                usedCar.Caroserie = caroserie != null ? caroserie : null;

                //stare
                var stare = GetDetail(htmlDoc, "Stare");
                usedCar.Stare = stare;

                //marca
                var marca = GetDetail(htmlDoc, "Marca");
                usedCar.Marca = marca;

                //culoare
                var culoare = GetDetail(htmlDoc, "Culoare");
                usedCar.Culoare = culoare;

                //Cutie de viteze	
                var cutie = GetDetail(htmlDoc, "Cutie de viteze");
                usedCar.CutieDeViteze = cutie;

                //Rulaj	
                var rulaj = GetDetail(htmlDoc, "Rulaj", true);
                if (rulaj != null)
                {
                    usedCar.Rulaj = float.Parse(rulaj);
                }

                //Capacitate motor	
                var capacitate = GetDetail(htmlDoc, "Capacitate motor", true);
                if (capacitate != null)
                {
                    usedCar.CapacitateMotor = float.Parse(capacitate);
                }

                return usedCar;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error trying to scrape " + usedCar.Url);
                Console.WriteLine(e.Message);
                Log.Debug(e, "Error trying to scrape " + usedCar.Url);
                Log.Information("HTML *************");
                Log.Information(html);
                Log.Information(Environment.NewLine);
                Log.Information(Environment.NewLine);

                return usedCar;
            }
        }

        private string GetDetail(HtmlDocument htmlDoc, string detailName, bool removeNonNumericChars = false, bool removeNewLine = true)
        {
            var detailNode = htmlDoc.DocumentNode.Descendants("span").Where(x => x.InnerText.Contains(detailName)).FirstOrDefault();

            if (!detailNode.ChildNodes.Where(x => x.Name == "strong").Any())
            {
                detailNode = detailNode.ParentNode;
            }
            string detail = detailNode.ChildNodes.Where(x => x.Name == "strong").FirstOrDefault().InnerText;
            if (removeNewLine)
            {
                detail = HtmlUtils.RemoveNewLineCharacters(detail);
            }

            if (removeNonNumericChars)
            {
                detail = HtmlUtils.RemoveNonNumericCharacters(detail);
            }
            return detail;
        }
    }
}
