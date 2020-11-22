import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { UsedCar } from 'src/app/models/UsedCars'
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PricePredictionService {

  readonly httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json',
      'Access-Control-Allow-Origin': '*'
    })
  };

  constructor(private http: HttpClient) {
  }

  GetPrice(usedcar: UsedCar) {
    return this.http.post(environment.apiEstimatePriceURL, usedcar, this.httpOptions);
  }

  ScrapeAdvert(url: string) {
    return this.http.post(environment.apiScrapeAdvertURL, { "url": url }, this.httpOptions);
  }
}
