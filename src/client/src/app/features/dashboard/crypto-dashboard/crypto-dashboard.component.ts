import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CryptoService } from '../../../core/services/crypto.service';
import { CryptoPrice } from '../../../core/models/crypto.model';
import { interval } from 'rxjs';
import { startWith, switchMap, catchError } from 'rxjs/operators';
import { firstValueFrom } from 'rxjs';
import { PriceWidgetComponent } from '../price-widget/price-widget.component';
import { PriceChartComponent } from '../price-chart/price-chart.component';


@Component({
  selector: 'app-crypto-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    PriceWidgetComponent,
    PriceChartComponent
  ],
  templateUrl: './crypto-dashboard.component.html',
  styleUrls: ['./crypto-dashboard.component.scss']
})
export class CryptoDashboardComponent implements OnInit {
  private cryptoService = inject(CryptoService);
  
  // State signals
  currentPrice = signal<CryptoPrice | null>(null);
  historicalPrices = signal<CryptoPrice[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  hasError = computed(() => !!this.error());
  hasData = computed(() => !!this.currentPrice() && this.historicalPrices().length > 0);

  constructor() {
    this.setupPriceUpdates();
  }

  ngOnInit(): void {
    this.loadInitialData();
  }

  private async loadInitialData(): Promise<void> {
    try {
      this.loading.set(true);
      await Promise.all([
        this.loadCurrentPrice(),
        this.loadHistoricalData()
      ]);
      this.error.set(null);
    } catch (err) {
      this.error.set('Failed to load initial data. Please try again later.');
      console.error('Error loading data:', err);
    } finally {
      this.loading.set(false);
    }
  }

  private async loadCurrentPrice(): Promise<void> {
    try {
      const price = await firstValueFrom(
        this.cryptoService.getCurrentPrice().pipe(
          catchError(err => {
            console.error('Error fetching current price:', err);
            throw new Error('Failed to load current price');
          })
        )
      );
      this.currentPrice.set(price);
    } catch (error) {
      throw error;
    }
  }

  private async loadHistoricalData(): Promise<void> {
    try {
      const endDate = new Date();
      const startDate = new Date();
      startDate.setDate(startDate.getDate() - 30);

      const prices = await firstValueFrom(
        this.cryptoService.getHistoricalPrices(startDate, endDate).pipe(
          catchError(err => {
            console.error('Error fetching historical prices:', err);
            throw new Error('Failed to load historical data');
          })
        )
      );
      this.historicalPrices.set(prices);
    } catch (error) {
      throw error;
    }
  }

  private setupPriceUpdates(): void {
    interval(5000).pipe(
      startWith(0),
      switchMap(() => this.cryptoService.getCurrentPrice()),
      catchError((error) => {
        console.error('Error in price update:', error);
        this.error.set('Failed to update price data. Will retry automatically.');
        throw error;
      }),
      takeUntilDestroyed()
    ).subscribe({
      next: (price) => {
        this.currentPrice.set(price);
        this.error.set(null);
      }
    });
  }

  async refreshData(): Promise<void> {
    await this.loadInitialData();
  }
}