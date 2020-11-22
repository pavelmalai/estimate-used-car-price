using UsedCarsPrice.Common.Models;

namespace UsedCarsPrice.API.Models
{
    public class UsedCarViewModel
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Body { get; set; }
        public string Gearbox { get; set; }
        public int YearCreated { get; set; }
        public string Fuel { get; set; }
        public float? Rulaj { get; set; }
        public float? CapacitateMotor { get; set; }
        public float? Pret { get; set; }

        public UsedCarViewModel CreateFrom(UsedCarModel model)
        {
            int anFabricatieTemp;
            return new UsedCarViewModel()
            {
                YearCreated = int.TryParse(model.AnFabricatie, out anFabricatieTemp) ? anFabricatieTemp : default(int),
                Brand = model.Marca,
                Model = model.Model,
                Body = model.Caroserie,
                Gearbox = model.CutieDeViteze,
                Fuel = model.Combustibil,
                Rulaj = model.Rulaj,
                CapacitateMotor = model.CapacitateMotor,
                Pret = model.Pret
            };
        }
        public UsedCarMlModel ToMlModel()
        {
            return new UsedCarMlModel()
            {
                YearCreated = YearCreated,
                Brand = Brand,
                Model = Model,
                Body = Body,
                Gearbox = Gearbox,
                Fuel = Fuel,
                Turnover = Rulaj.HasValue ? Rulaj.Value : default(float),
                EngineCapacity = CapacitateMotor.HasValue ? CapacitateMotor.Value : default(float),
                Price = Pret.HasValue ? Pret.Value : default(float)
            };
        }
    }
}
