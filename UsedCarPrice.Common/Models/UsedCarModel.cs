using System;

namespace UsedCarsPrice.Common.Models
{
    public class UsedCarModel
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Body { get; set; }
        public string Gearbox { get; set; }
        public string Year { get; set; }
        public string OferitDe { get; set; }
        public string Fuel { get; set; }
        public string Color { get; set; }
        public float? Mileage { get; set; }
        public float? EngineCapacity { get; set; }
        public string Description { get; set; }
        public string Stare { get; set; }
        public sbyte? Scraped { get; set; }
        public float? Price { get; set; }
        public DateTime? LastModified { get; set; }
        public string GoupId { get; set; }

        public UsedCarMlModel ToMlModel()
        {
            return new UsedCarMlModel()
            {
                YearCreated = float.Parse(Year),
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
