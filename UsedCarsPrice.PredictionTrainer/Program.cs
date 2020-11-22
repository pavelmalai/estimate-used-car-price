using Microsoft.ML;
using System;
using System.IO;
using UsedCarsPricePrediction.Model;

namespace UsedCarsPricePrediction
{
    class Program
    {
        static readonly string _trainTaxiFareDataPath = "C:\\WORKSPACE\\UsedCarsScraper\\UsedCarsPricePrediction\\Data\\taxi-fare-train.csv";
        static readonly string _testTaxiFareDataPath = "C:\\WORKSPACE\\UsedCarsScraper\\UsedCarsPricePrediction\\Data\\taxi-fare-test.csv";

        static readonly string _trainUsedCarsDataPath = "C:\\WORKSPACE\\EstimateUsedCarPrice\\UsedCarsPricePrediction\\Data\\usedcars-train-1-7-2020.csv";
        static readonly string _testUsedCarsDataPath = "C:\\WORKSPACE\\EstimateUsedCarPrice\\UsedCarsPricePrediction\\Data\\usedcars-test-1-7-2020.csv";
        static readonly string _modelPath = Path.Combine("C:\\WORKSPACE\\EstimateUsedCarPrice\\UsedCarsPricePrediction\\Data\\UsedCarsPricePredictionModel-1-7-2020.zip");

        static void Main(string[] args)
        {
            RunUsedCarsPricePrediction();

            Console.ReadKey();
        }

        public static void RunUsedCarsPricePrediction()
        {
            MLContext mlContext = new MLContext(seed: 0);
            var model = TrainUsedCarPrice(mlContext, _trainUsedCarsDataPath);

            EvaluateUsedCarsPricePrediction(mlContext, model);
        }

        public static void RunTaxiFarePrediction()
        {
            MLContext mlContext = new MLContext(seed: 0);
            var model = TrainTaxiFare(mlContext, _trainTaxiFareDataPath);

            EvaluateTextFare(mlContext, model);
        }

        public static ITransformer TrainUsedCarPrice(MLContext mlContext, string dataPath)
        {
            IDataView dataView = mlContext.Data.LoadFromTextFile<UsedCar>(dataPath, hasHeader: false, separatorChar: ',');
            dataView = mlContext.Data.FilterRowsByColumn(dataView, "Pret", lowerBound: 1000, upperBound: 50000);
            dataView = mlContext.Data.FilterRowsByColumn(dataView, "Rulaj", lowerBound: 500, upperBound: 500000);

            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "Pret")
                .Append(mlContext.Transforms.NormalizeBinning("Rulaj", maximumBinCount: 50))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "MarcaEncoded", inputColumnName: "Marca"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "ModelEncoded", inputColumnName: "Model"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CaroserieEncoded", inputColumnName: "Caroserie"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CutieDeVitezeEncoded", inputColumnName: "CutieDeViteze"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CombustibilEncoded", inputColumnName: "Combustibil"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AnFabricatieEncoded", inputColumnName: "AnFabricatie"))
                .Append(mlContext.Transforms.Concatenate("Features", "MarcaEncoded", "ModelEncoded", "CaroserieEncoded", "CutieDeVitezeEncoded", "CombustibilEncoded", "AnFabricatieEncoded", "Rulaj", "CapacitateMotor"))
                .Append(mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);

            mlContext.Model.Save(model, dataView.Schema, _modelPath);

            return model;
        }

        private static void EvaluateUsedCarsPricePrediction(MLContext mlContext, ITransformer model)
        {
            IDataView dataView = mlContext.Data.LoadFromTextFile<UsedCar>(_testUsedCarsDataPath, hasHeader: false, separatorChar: ',');
            dataView = mlContext.Data.FilterRowsByColumn(dataView, "Pret", lowerBound: 1000, upperBound: 50000);
            dataView = mlContext.Data.FilterRowsByColumn(dataView, "Rulaj", lowerBound: 500, upperBound: 500000);
            var predictions = model.Transform(dataView);
            var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");

            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");

            Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:#.##}");
        }

        public static ITransformer TrainTaxiFare(MLContext mlContext, string dataPath)
        {
            IDataView dataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(dataPath, hasHeader: true, separatorChar: ',');

            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "FareAmount")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "VendorIdEncoded", inputColumnName: "VendorId"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "RateCodeEncoded", inputColumnName: "RateCode"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "PaymentTypeEncoded", inputColumnName: "PaymentType"))
                .Append(mlContext.Transforms.Concatenate("Features", "VendorIdEncoded", "RateCodeEncoded", "PassengerCount", "TripTime", "TripDistance", "PaymentTypeEncoded"))
                .Append(mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);
            return model;
        }

        private static void EvaluateTextFare(MLContext mlContext, ITransformer model)
        {
            IDataView dataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(_testTaxiFareDataPath, hasHeader: true, separatorChar: ',');
            var predictions = model.Transform(dataView);
            var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");

            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");

            Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:#.##}");

        }

        private static void TestSinglePrediction(MLContext mlContext, ITransformer model)
        {
            TestSinglePrediction(mlContext, model);
            var predictionFunction = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(model);
            var taxiTripSample = new TaxiTrip()
            {
                VendorId = "VTS",
                RateCode = "1",
                PassengerCount = 1,
                TripTime = 1140,
                TripDistance = 3.75f,
                PaymentType = "CRD",
                FareAmount = 0 // To predict. Actual/Observed = 15.5
            };
            var prediction = predictionFunction.Predict(taxiTripSample);
            Console.WriteLine($"**********************************************************************");
            Console.WriteLine($"Predicted fare: {prediction.FareAmount:0.####}, actual fare: 15.5");
            Console.WriteLine($"**********************************************************************");
        }
    }
}
