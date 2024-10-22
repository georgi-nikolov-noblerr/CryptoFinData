// src/app/core/services/crypto.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CryptoPrice } from '../models/crypto.model';
import { ApiError } from '../models/api-error.model'

@Injectable({
  providedIn: 'root'
})
export class CryptoService {
  private http = inject(HttpClient);

  getCurrentPrice(): Observable<CryptoPrice> {
    return this.http.get<CryptoPrice>(`${environment.apiUrl}/crypto/price`)
      .pipe(
        catchError(this.handleError)
      );
  }

  getHistoricalPrices(from: Date, to: Date): Observable<CryptoPrice[]> {
    const params = {
      from: from.toISOString(),
      to: to.toISOString()
    };
    return this.http.get<CryptoPrice[]>(`${environment.apiUrl}/crypto/history`, { params })
      .pipe(
        catchError(this.handleError)
      );
  }

  private handleError(error: HttpErrorResponse) {
    let errorMessage: string;

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = 'An error occurred: ' + error.error.message;
    } else {
      // Server-side error
      const apiError = error.error as ApiError;
      errorMessage = apiError?.message || `Server error: ${error.status}`;
    }

    return throwError(() => new Error(errorMessage));
  }
}