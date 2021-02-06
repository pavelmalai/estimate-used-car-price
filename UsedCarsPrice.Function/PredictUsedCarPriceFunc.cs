using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.ML;
using UsedCarsPrice.Common.Models;
using System;

namespace PredictPriceFunction
{
    public class PredictUsedCarPriceFunc
    {
        private readonly PredictionEnginePool<UsedCarMlModel, UsedCarPricePrediction> _predictionEnginePool;

        public PredictUsedCarPriceFunc(PredictionEnginePool<UsedCarMlModel, UsedCarPricePrediction> predictionEnginePool)
        {
            _predictionEnginePool = predictionEnginePool;
        }

        [FunctionName("PredictUsedCarPrice")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UsedCarPricePrediction prediction = null;
            try
            {
                UsedCarModel data = JsonConvert.DeserializeObject<UsedCarModel>(requestBody);
                UsedCarMlModel mlModel = data.ToMlModel();
                prediction = _predictionEnginePool.Predict(modelName: "UsedCarsPricePredictionModel", example: mlModel);
            }
            catch (Exception e)
            {
                log.LogError(e, $"Error trying to estimate the price for {requestBody}");
            }

            return new OkObjectResult(prediction?.Price);
        }
    }
}
