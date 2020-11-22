import { Component, OnInit } from '@angular/core';
import { UsedCar } from './models/UsedCars';
import { PricePredictionService } from './services/price-prediction.service';
import { marci, modele, combustibil, caroserii, cutieDeViteze } from 'src/assets/lists.json'
import { carBrands, brandsModels } from 'src/assets/dropdownsValues.json'
import { Options } from 'ng5-slider';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Estimeaza pret masina second-hand';
  scrapedUsedCar: UsedCar;
  pricePredictionService: PricePredictionService;
  response: string;
  advertURL: string;
  showScrapedCarDetails: boolean;

  /* Default values */
  brand:string = 'Volkswagen';
  model: string = 'Golf';
  fuel:string = 'Benzina';
  carBody: string;
  gearbox: string = 'Manuala';
  manufacturingYear:number = 2007;
  engineCapacity: number = 2000;
  mileage: number = 200000;
  engineCapacityOptions: Options;
  mileageOptions: Options;

  brands = [];
  models = {};
  fuels = [];
  carBodies = [];
  gearboxes = [];
  manufacturingYears = [];

  constructor(pricePredictionService: PricePredictionService) {
    this.pricePredictionService = pricePredictionService;
    this.scrapedUsedCar = new UsedCar();
    this.showScrapedCarDetails
    this.advertURL = "https://www.autovit.ro/anunt/volkswagen-golf-vii-ID7GyEws.html#xtor=SEC-81"
  }

  ngOnInit() {

    this.brands = carBrands;
    this.models = brandsModels[this.brand];
    this.fuels = combustibil;
    this.carBodies = caroserii;
    this.gearboxes = cutieDeViteze;
    this.manufacturingYears = this.getAniFabricatie();

    this.engineCapacityOptions = {
      floor: 0,
      ceil: 3000,
      step: 10,
      showTicks: false
    };
    this.mileageOptions = {
      floor: 0,
      ceil: 500000,
      step: 1000,
      showTicks: false
    };
  }

  PredictPrice() {
    let usedCar = new UsedCar();
    usedCar.marca = this.brand;
    usedCar.model = this.model;
    usedCar.caroserie = this.carBody;
    usedCar.cutieDeViteze = this.gearbox;
    usedCar.anFabricatie = this.manufacturingYear;
    usedCar.combustibil = this.fuel;
    usedCar.capacitateMotor = this.engineCapacity;
    usedCar.rulaj = this.mileage;

    this.pricePredictionService.GetPrice(usedCar).subscribe(data => {
      this.response = Number(data).toFixed();
    },
      Error => { alert("Error trying to access the backend service!") });
  }

  EstimatePriceFromAdvert() {
    this.pricePredictionService.ScrapeAdvert(this.advertURL).subscribe((data: UsedCar) => {
      this.showScrapedCarDetails = false;
      this.scrapedUsedCar = data;
      this.showScrapedCarDetails = true;
      console.log(this.scrapedUsedCar)
    },
      Error => { alert("Error trying to access the backend service!") });
  }

  getAniFabricatie(): number[] {
    let currentYear: Number = Number((new Date()).getFullYear());
    let startYear: number = 1950;
    let yearsList = [];
    while (startYear <= currentYear) {
      yearsList.push(startYear);
      startYear = startYear + 1;
    }

    return yearsList;
  }

  onBrandChange($event) {
    this.models = brandsModels[this.brand];
  }
}
