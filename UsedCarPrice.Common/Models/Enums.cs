using System;
using System.Collections.Generic;
using System.Text;

namespace UsedCarsPrice.Common.Models
{
    public enum ProcessingStatus
    {
        Unprocessed = 0,
        Processing = 1,
        Processed = 2,
        Invalid = 3
    }
    public static class CarState
    {
        public const string Used = "Utilizat";
        public const string New = "Nou";
    }

    public static class Constants
    {
        public static string OlxBaseSearchUrl = "https://www.olx.ro/auto-masini-moto-ambarcatiuni/autoturisme/?page={0}";
        public static string OlxBaseSearchUrlByCounty = "https://www.olx.ro/auto-masini-moto-ambarcatiuni/autoturisme/{0}-judet/?page={1}";

        public static List<string> Counties = new List<string>()
        {
            "alba", "arad", "arges", "bacau", "bihor","bistrita-nasaud","botosani","braila","brasov","bucuresti-ilfov","buzau","calarasi","caras-severin","cluj","constanta","covasna","dambovita","dolj","galati","giurgiu","gorj","harghita","hunedoara","ialomita","iasi","maramures","mehedinti","mures","neamt","olt","prahova","salaj","satu-mare","sibiu","suceava","teleorman","timis","tulcea","valcea","vaslui","vrancea"
        };
    }
}
