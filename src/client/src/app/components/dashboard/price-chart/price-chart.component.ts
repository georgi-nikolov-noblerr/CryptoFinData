import { Component, Input, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { Chart, ChartConfiguration } from 'chart.js/auto';
import { CryptoService } from '../../../core/services/crypto.service';
import { CryptoPrice } from '../../../core/models/crypto.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-price-chart',
  standalone: true,
  templateUrl: './price-chart.component.html',
  styleUrls: ['./price-chart.component.scss'],
  imports: [FormsModule]
})
export class PriceChartComponent implements OnInit, OnDestroy {
  @ViewChild('chartCanvas', { static: false }) chartCanvas!: ElementRef<HTMLCanvasElement>;
  @Input() historicalData: CryptoPrice[] = [];

  private chart: Chart | undefined;
  selectedPeriod: number = 30;  // Default to 30 days
  private today: Date = new Date();

  constructor(private cryptoService: CryptoService) {}

  ngOnInit(): void {
    this.fetchDataForSelectedPeriod();
  }

  ngOnDestroy(): void {
    if (this.chart) {
      this.chart.destroy();
    }
  }

  onPeriodChange(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;  // Type assertion
    this.selectedPeriod = +selectElement.value;
    this.fetchDataForSelectedPeriod();
  }

  private fetchDataForSelectedPeriod(): void {
    const fromDate = this.calculateFromDate(this.selectedPeriod);
    this.cryptoService.getHistoricalPrices(fromDate, this.today).subscribe(
      data => {
        this.historicalData = data;
        this.initializeChart();
      },
      error => {
        console.error('Failed to fetch historical data', error);
      }
    );
  }

  private calculateFromDate(period: number): Date {
    const fromDate = new Date();
    fromDate.setDate(this.today.getDate() - period);
    return fromDate;
  }

  private initializeChart(): void {
    if (this.chart) {
      this.chart.destroy();  // Destroy previous chart instance
    }

    const ctx = this.chartCanvas?.nativeElement.getContext('2d');
    if (!ctx) {
      console.error('Canvas context not found');
      return;
    }

    const data = this.prepareChartData();

    const config: ChartConfiguration = {
      type: 'line',
      data: {
        labels: data.labels,
        datasets: [
          {
            label: 'Bitcoin Price',
            data: data.prices,
            borderColor: 'rgb(2, 132, 199)',
            backgroundColor: 'rgba(2, 132, 199, 0.1)',
            fill: true,
            tension: 0.4,
            borderWidth: 2,
            pointRadius: 3,
            pointBackgroundColor: 'rgb(2, 132, 199)',
            pointBorderColor: '#fff',
            pointBorderWidth: 2,
            pointHoverRadius: 5,
            pointHoverBackgroundColor: 'rgb(2, 132, 199)',
            pointHoverBorderColor: '#fff',
            pointHoverBorderWidth: 2,
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            mode: 'index',
            intersect: false,
            callbacks: {
              label: (context) => {
                return `Price: $${context.parsed.y.toLocaleString('en-US', {
                  minimumFractionDigits: 2,
                  maximumFractionDigits: 2
                })}`;
              }
            },
            backgroundColor: 'rgba(255, 255, 255, 0.9)',
            titleColor: '#1e293b',
            bodyColor: '#1e293b',
            borderColor: '#e2e8f0',
            borderWidth: 1,
            padding: 12
          }
        },
        scales: {
          x: {
            grid: {
              display: false
            },
            ticks: {
              maxRotation: 45,
              minRotation: 45
            }
          },
          y: {
            grid: {
              color: 'rgba(0, 0, 0, 0.05)'
            },
            ticks: {
              callback: (value) => {
                return `$${value.toLocaleString('en-US')}`;
              }
            }
          }
        }
      }
    };

    this.chart = new Chart(ctx, config);
  }

  private prepareChartData() {
    const sortedData = [...this.historicalData].sort(
      (a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
    );

    return {
      labels: sortedData.map(data => 
        new Date(data.timestamp).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric'
        })
      ),
      prices: sortedData.map(data => data.price)
    };
  }
}
