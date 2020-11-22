using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace UsedCarPriceCore.Utils
{
    public class CommonUtils
    {

        public static IConfigurationRoot config = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json").Build();

        public static void SendEmail(string email, string subject, string body)
        {
            using (var message = new MailMessage())
            {
                message.To.Add(new MailAddress(email));
                message.From = new MailAddress(config["EmailFrom"]);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = false;

                using (var client = new SmtpClient(config["SMTPHost"]))
                {
                    client.Port = 587;
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = true;
                    client.Credentials = new NetworkCredential(config["EmailFrom"], config["EmailFromPassword"]);
                    client.Send(message);
                }
            }
        }
    }
}
