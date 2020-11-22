using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UsedCarsPrice.WebScraper.Services.Utils
{
    public class HtmlUtils
    {
        public static string RemoveNewLineCharacters(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return Regex.Replace(str, @"\t|\n|\r", "");
        }

        public static string SanitizeString(string str)
        {
            return RemoveNewLineCharacters(str).Trim();
        }

        public static string RemoveNonNumericCharacters(string str)
        {
            return Regex.Replace(str, "[^.0-9]", "");
        }

        public static bool BaseURLContains(string url, string str)
        {
            var uri = new Uri(url);
            var host = uri.Host;
            return host.Contains(str);
        }

        public static string RemoveAnchorFromURL(string url)
        {
            return url.Substring(0, url.LastIndexOf("#"));
        }
    }
}
