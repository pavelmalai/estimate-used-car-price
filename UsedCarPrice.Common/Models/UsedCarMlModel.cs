using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Data;


namespace UsedCarsPrice.Common.Models
{
    public class UsedCarMlModel
    {
        [LoadColumn(0)]
        public string Brand { get; set; }

        [LoadColumn(1)]
        public string Model { get; set; }

        [LoadColumn(2)]
        public string Body { get; set; }

        [LoadColumn(3)]
        public string Gearbox { get; set; }

        [LoadColumn(4)]
        public float YearCreated { get; set; }

        [LoadColumn(5)]
        public string Fuel { get; set; }

        [LoadColumn(6)]
        public float Turnover { get; set; }

        [LoadColumn(7)]
        public float EngineCapacity { get; set; }

        [LoadColumn(8)]
        public float Price { get; set; }

    }

    public class UsedCarPricePrediction
    {
        [ColumnName("Score")]
        public float Price { get; set; }
    }
}
