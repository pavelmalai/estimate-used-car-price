using UsedCarsPrice.Common.Models;

namespace UsedCarsPrice.API.Models
{
    public class UsedCarViewModel
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Body { get; set; }
        public string Gearbox { get; set; }
        public int Year { get; set; }
        public string Fuel { get; set; }
        public float? Mileage { get; set; }
        public float? EngineCapacity { get; set; }
        public float? Price { get; set; }

        public UsedCarViewModel CreateFrom(UsedCarModel model)
        {
            int yearTemp;
            return new UsedCarViewModel()
            {
                Year = int.TryParse(model.Year, out yearTemp) ? yearTemp : default(int),
                Brand = model.Brand,
                Model = model.Model,
                Body = model.Body,
                Gearbox = model.Gearbox,
                Fuel = model.Fuel,
                Mileage = model.Mileage,
                EngineCapacity = model.EngineCapacity,
                Price = model.Price
            };
        }
        public UsedCarMlModel ToMlModel()
        {
            return new UsedCarMlModel()
            {
                YearCreated = Year,
                Brand = Brand,
                Model = Model,
                Body = Body,
                Gearbox = Gearbox,
                Fuel = Fuel,
                Mileage = Mileage.HasValue ? Mileage.Value : default(float),
                EngineCapacity = EngineCapacity.HasValue ? EngineCapacity.Value : default(float),
                Price = Price.HasValue ? Price.Value : default(float)
            };
        }
    }
}
