using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Newtonsoft.Json;
using UsedCarsPrice.API.Models;
using UsedCarsPrice.Common.Models;

namespace UsedCarsPrice.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsedCarsController : ControllerBase
    {

        private readonly PredictionEnginePool<UsedCarMlModel, UsedCarPricePrediction> _predictionEnginePool;
        private readonly ILogger<UsedCarsController> _logger;

        public UsedCarsController(PredictionEnginePool<UsedCarMlModel, UsedCarPricePrediction> predictionEnginePool, ILogger<UsedCarsController> logger)
        {
            _predictionEnginePool = predictionEnginePool;
            _logger = logger;
        }

        /// <summary>
        /// Estimates the price for a used car
        /// </summary>
        [HttpPost]
        [Route("EstimateUsedCarPrice")]
        public ActionResult<string> EstimateUsedCarPrice([FromBody] UsedCarViewModel input)
        {
            _logger.LogInformation("Trying to estimate price for input: ", JsonConvert.SerializeObject(input) );
            double price;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                UsedCarPricePrediction prediction = _predictionEnginePool.Predict(input.ToMlModel());

                price = prediction.Pret;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in EstimateUsedCarPrice()", e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(price);
        }
    }
}