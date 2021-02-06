using System;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using UsedCarsPrice.Common.Models;
using UsedCarsPrice.Function;

[assembly: FunctionsStartup(typeof(Startup))]
namespace UsedCarsPrice.Function
{
    public class Startup : FunctionsStartup
    {
        private readonly string _environment;
        private readonly string _modelPath;
        public Startup()
        {
            _environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");

            if (_environment == "Development")
            {
                _modelPath = Path.Combine("Assets", "UsedCarsPricePredictionModel.zip");
            }
            else
            {
                string deploymentPath = @"D:\home\site\wwwroot\";
                _modelPath = Path.Combine(deploymentPath, "Assets", "UsedCarsPricePredictionModel.zip");
            }
        }
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddPredictionEnginePool<UsedCarMlModel, UsedCarPricePrediction>()
                .FromFile(modelName: "UsedCarsPricePredictionModel", filePath: _modelPath, watchForChanges: true);
        }
    }
}
