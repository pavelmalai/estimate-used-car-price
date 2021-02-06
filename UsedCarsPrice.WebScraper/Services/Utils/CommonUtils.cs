using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using UsedCarsPrice.Common.Models;

namespace UsedCarsPrice.WebScraper.Services.Utils
{
    public class CommonUtils
    {
        public const int MARCI_MIN_COUNT = 3000;
        public const int MODEL_MIN_COUNT = 300;
        public static readonly List<string> BrandsIgnored = new List<string>() { "alte marci" };
        public static readonly List<string> ModelsIgnored = new List<string>() { "altul" };

        public static IConfigurationRoot config = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json").Build();

        public static void LogDuration(Stopwatch watch, string message)
        {
            Log.Information(string.Format(message + ": {0:D2}:{1:D2}:{2:D2}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds));
        }

        public static List<List<T>> Split<T>(List<T> collection, int size)
        {
            var chunks = new List<List<T>>();
            var chunkCount = collection.Count() / size;

            if (collection.Count % size > 0)
                chunkCount++;

            for (var i = 0; i < chunkCount; i++)
                chunks.Add(collection.Skip(i * size).Take(size).ToList());

            return chunks;
        }

        public static void SendEmail(string email, string subject, string body)
        {
            using (var message = new MailMessage())
            {
                message.To.Add(new MailAddress(email, "To my friend"));
                message.From = new MailAddress(config["EmailFrom"], "Your friend");
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = false;

                using (var client = new SmtpClient(config["SMTPHost"]))
                {
                    client.Port = 587;
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(config["EmailFrom"], config["EmailFromPassword"]);
                    client.Send(message);
                }
            }
        }

        public static string GetConfigItem(string name)
        {
            return config[name];
        }

        public static IEnumerable<List<T>> splitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        public static void GenerateDropdownValuesJson()
        {
            using (UsedCarsDbContext db = new UsedCarsDbContext())
            {
                var brands = db.Usedcars.GroupBy(c => c.Brand)
                    .Select(n => new
                    {
                        marca = n.Key,
                        count = n.Count()
                    })
                    .Where(n => n.count >= MARCI_MIN_COUNT && n.marca != null && !BrandsIgnored.Contains(n.marca))
                    .OrderBy(n => n.marca)
                    .Select(x => x.marca)
                    .ToList();

                Dictionary<string, List<string>> models = new Dictionary<string, List<string>>();
                foreach (var brand in brands)
                {
                    var modeleMarca = db.Usedcars.Where(c => c.Brand == brand)
                        .GroupBy(g => g.Model)
                        .Select(n => new
                        {
                            model = n.Key,
                            count = n.Count()
                        })
                        .Where(n => n.count >= MODEL_MIN_COUNT && n.model != null && !ModelsIgnored.Contains(n.model))
                        .OrderBy(n => n.model)
                        .Select(x => x.model)
                    .ToList();

                    models.Add(brand, modeleMarca);
                }

                var fuels = new List<DropdownItem>();
                fuels.Add(new DropdownItem("Benzina", "Benzină"));
                fuels.Add(new DropdownItem("Diesel", "Diesel"));

                var gears = new List<DropdownItem>();
                gears.Add(new DropdownItem("Manuala", "Manuală"));
                gears.Add(new DropdownItem("Automata", "Automată"));

                Dictionary<string, object> dropdownValues = new Dictionary<string, object>();
                dropdownValues.Add("carBrands", brands);
                dropdownValues.Add("brandsModels", models);

                dropdownValues.Add("fuels", fuels);
                dropdownValues.Add("gears", gears);

                //TODO caroserii

                var dropdownValuesJson = JsonSerializer.Serialize(dropdownValues);
                WriteTextToFile("dropdownValues.json", dropdownValuesJson);

            }


        }

        public static void WriteTextToFile(string fileName, string text)
        {
            File.WriteAllText(fileName, text);
        }
    }
}
