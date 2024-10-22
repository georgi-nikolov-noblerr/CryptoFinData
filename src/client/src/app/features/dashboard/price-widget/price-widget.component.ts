import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CryptoPrice } from '../../../core/models/crypto.model';

@Component({
  selector: 'app-price-widget',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './price-widget.component.html',
  styleUrls: ['./price-widget.component.scss']
})
export class PriceWidgetComponent {
  @Input() price: CryptoPrice | null = null;
  @Input() currencyType: 'USD' | 'EUR' | 'GBP' = 'USD';

  get currencyLabel(): string {
    switch(this.currencyType) {
      case 'EUR': return 'Bitcoin (EUR)';
      case 'GBP': return 'Bitcoin (GBP)';
      default: return 'Bitcoin (USD)';
    }
  }

  get currencyCode(): string {
    return this.currencyType;
  }

  getPrice(): number {
    if (!this.price) return 0;
    switch(this.currencyType) {
      case 'EUR': return this.price.eurPrice;
      case 'GBP': return this.price.gbpPrice;
      default: return this.price.price;
    }
  }
}