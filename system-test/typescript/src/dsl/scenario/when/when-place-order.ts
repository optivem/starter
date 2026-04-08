import { DEFAULTS } from '../../defaults';
import { AppContext } from '../app-context';
import { ScenarioContext } from '../scenario-context';
import { ThenResultStage } from '../then/then-place-order';

export class WhenPlaceOrder {
  private sku: string = DEFAULTS.SKU;
  private quantity: string | null = DEFAULTS.QUANTITY;
  private country: string = DEFAULTS.COUNTRY;
  private couponCode: string | null = null;

  constructor(
    private readonly app: AppContext,
    private readonly ctx: ScenarioContext,
  ) {}

  withSku(sku: string): WhenPlaceOrder {
    this.sku = sku;
    return this;
  }

  withQuantity(quantity: string | number | null): WhenPlaceOrder {
    this.quantity = quantity === null ? null : String(quantity);
    return this;
  }

  withCountry(country: string): WhenPlaceOrder {
    this.country = country;
    return this;
  }

  withCouponCode(couponCode: string): WhenPlaceOrder {
    this.couponCode = couponCode;
    return this;
  }

  then(): ThenResultStage {
    return new ThenResultStage(this.app, this.ctx, this.sku, this.quantity, this.country, this.couponCode);
  }
}
