import { Component, OnInit } from '@angular/core';
import { UsedCar } from './models/UsedCars';
import { PricePredictionService } from './services/price-prediction.service';
import { combustibil, caroserii, cutieDeViteze } from 'src/assets/lists.json'
import { carBrands, brandsModels } from 'src/assets/dropdownsValues.json'
import { Options } from 'ng5-slider';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { DropdownValue, Locale } from './models/enums';
import { CookieOptions, CookieService } from 'ngx-cookie';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  providers: [TranslatePipe]
})
export class AppComponent {
  scrapedUsedCar: UsedCar;
  response: string;
  advertURL: string;

  /* Default values */
  brand: string = 'Volkswagen';
  model: string = 'Golf';
  fuel: string;
  gearbox: string;
  carBody: string;
  manufacturingYear: number = 2007;
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

  LOCALE = Locale;
  currentLocale: Locale;
  loading = true;
  loadingResult = false;

  constructor(private pricePredictionService: PricePredictionService,
    private translate: TranslateService,
    private cookieService: CookieService,
    private translatePipe: TranslatePipe) {
    this.setCurrentLanguage();
    this.scrapedUsedCar = new UsedCar();
    this.advertURL = ""
  }

  ngOnInit() {
    this.setCurrentLanguage();
    this.initSettings();
    this.loading = false;

  }

  initSettings() {
    this.brands = carBrands;
    this.models = brandsModels[this.brand];
    this.carBodies = caroserii;
    this.manufacturingYears = this.getYearMade();

    this.fuels = combustibil.map(x => {
      return {
        id: x,
        name: "fuels." + x.toLowerCase()
      } as DropdownValue
    });
    this.fuel = this.fuels[0].id;

    this.gearboxes = cutieDeViteze.map(x => {
      return {
        id: x,
        name: "gearbox." + x.toLowerCase()
      } as DropdownValue
    });
    this.gearbox = this.gearboxes[0].id;


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

  setCurrentLanguage() {
    let localeCookie = this.cookieService.get('locale')
    let userLocale = this.getLang();
    if (localeCookie) {
      this.currentLocale = localeCookie as Locale;
      this.setLanguage(localeCookie);
      return;
    } else if (userLocale == Locale.EN || userLocale == Locale.RO) {
      this.setLanguage(userLocale);
      this.currentLocale = userLocale as Locale;
      return;
    }

    //default locale English
    this.currentLocale = Locale.EN;
    this.setLanguage(this.currentLocale);
  }

  setLanguage(language: string) {
    // this language will be used as a fallback when a translation isn't found in the current language
    this.translate.setDefaultLang(language);
    // the lang to use, if the lang isn't available, it will use the current loader to get them
    this.translate.use(language);
  }

  PredictPrice() {
    this.loadingResult = true;
    let usedCar = new UsedCar();
    usedCar.brand = this.brand;
    usedCar.model = this.model;
    usedCar.body = this.carBody;
    usedCar.gearbox = this.gearbox;
    usedCar.year = this.manufacturingYear;
    usedCar.fuel = this.fuel;
    usedCar.engineCapacity = this.engineCapacity;
    usedCar.mileage = this.mileage;

    console.log(JSON.parse(JSON.stringify(usedCar)));
    this.pricePredictionService.GetPrice(usedCar).subscribe(data => {
      this.response = Number(data).toFixed();
      this.loadingResult = false;
    },
      Error => {
        this.loadingResult = false;
        alert("Error trying to access the backend service!")
      });
  }

  EstimatePriceFromAdvert() {
    this.pricePredictionService.ScrapeAdvert(this.advertURL).subscribe((data: UsedCar) => {
      this.scrapedUsedCar = data;
    },
      Error => { alert("Error trying to access the backend service!") });
  }

  getYearMade(): number[] {
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

  getLang() {
    if (navigator.languages != undefined)
      return navigator.languages[0];
    else
      return navigator.language;
  }

  setLocale(locale: Locale) {
    this.translate.use(locale);
    this.currentLocale = locale;
    this.setLocaleCookie(locale);
  }

  setLocaleCookie(locale: string) {
    var today = new Date();
    this.cookieService.put('locale', locale, { expires: new Date(today.setMonth(today.getMonth() + 3)) } as CookieOptions);
  }
}
