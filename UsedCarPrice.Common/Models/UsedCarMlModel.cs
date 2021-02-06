using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ML.Data;


namespace UsedCarsPrice.Common.Models
{
    public class UsedCarMlModel
    {
        [ColumnName("Marca")]
        [LoadColumn(0)]
        public string Brand { get; set; }

        [ColumnName("Model")]

        [LoadColumn(1)]
        public string Model { get; set; }

        [ColumnName("Caroserie")]
        [LoadColumn(2)]
        public string Body { get; set; }

        [ColumnName("CutieDeViteze")]
        [LoadColumn(3)]
        public string Gearbox { get; set; }

        [ColumnName("AnFabricatie")]
        [LoadColumn(4)]
        public float YearCreated { get; set; }

        [ColumnName("Combustibil")]
        [LoadColumn(5)]
        public string Fuel { get; set; }

        [ColumnName("Rulaj")]
        [LoadColumn(6)]
        public float Mileage { get; set; }

        [LoadColumn(7)]
        [ColumnName("CapacitateMotor")]
        public float EngineCapacity { get; set; }

        [ColumnName("Pret")]
        [LoadColumn(8)]
        public float Price { get; set; }

    }

    public class UsedCarPricePrediction
    {
        [ColumnName("Score")]
        public float Price { get; set; }
    }
}
