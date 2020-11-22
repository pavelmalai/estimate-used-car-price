using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace UsedCarsPricePrediction.Model
{

    public class UsedCar
    {
        [LoadColumn(0)]
        public string Marca { get; set; }

        [LoadColumn(1)]
        public string Model { get; set; }

        [LoadColumn(2)]
        public string Caroserie { get; set; }

        [LoadColumn(3)]
        public string CutieDeViteze { get; set; }

        [LoadColumn(4)]
        public float AnFabricatie { get; set; }

        [LoadColumn(5)]
        public string Combustibil { get; set; }

        [LoadColumn(6)]
        public float Rulaj { get; set; }

        [LoadColumn(7)]
        public float CapacitateMotor { get; set; }

        [LoadColumn(8)]
        public float Pret { get; set; }

    }

    public class UsedCarPricePrediction
    {
        [ColumnName("Score")]
        public float Pret { get; set; }
    }

}
