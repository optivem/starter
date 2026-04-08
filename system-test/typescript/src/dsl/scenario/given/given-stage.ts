import { DEFAULTS } from '../../defaults';
import { UseCaseContext } from '../../use-case-context';
import { AppContext } from '../app-context';
import { ScenarioContext, ProductConfig, CouponConfig, CountryConfig, OrderConfig } from '../scenario-context';
import { WhenStage } from '../when/when-stage';
import { ThenContractStage } from '../then/then-contract';
import { GivenClock } from './given-clock';
import { GivenProduct } from './given-product';
import { GivenPromotion } from './given-promotion';
import { GivenCoupon } from './given-coupon';
import { GivenCountry } from './given-country';
import { GivenOrder } from './given-order';

export class GivenStage {
  constructor(
    private readonly app: AppContext,
    private readonly ctx: ScenarioContext,
    private readonly useCaseContext: UseCaseContext,
  ) {}

  clock(): GivenClock {
    this.ctx.clockConfig = { time: DEFAULTS.CLOCK_TIME };
    return new GivenClock(this, this.ctx.clockConfig);
  }

  product(): GivenProduct {
    const config: ProductConfig = { sku: DEFAULTS.SKU, price: DEFAULTS.UNIT_PRICE };
    this.ctx.productConfigs.push(config);
    this.ctx.hasExplicitProduct = true;
    return new GivenProduct(this, config);
  }

  promotion(): GivenPromotion {
    this.ctx.hasExplicitPromotion = true;
    return new GivenPromotion(this, this.ctx.promotionConfig);
  }

  coupon(): GivenCoupon {
    const config: CouponConfig = { code: '', discountRate: 0.10 };
    this.ctx.couponConfigs.push(config);
    return new GivenCoupon(this, config);
  }

  country(): GivenCountry {
    const config: CountryConfig = { country: DEFAULTS.COUNTRY, taxRate: DEFAULTS.TAX_RATE };
    this.ctx.countryConfigs.push(config);
    return new GivenCountry(this, config);
  }

  order(): GivenOrder {
    const config: OrderConfig = {
      sku: DEFAULTS.SKU,
      quantity: DEFAULTS.QUANTITY,
      country: DEFAULTS.COUNTRY,
      couponCode: null,
      status: DEFAULTS.STATUS,
    };
    this.ctx.orderConfigs.push(config);
    return new GivenOrder(this, config);
  }

  and(): GivenStage {
    return this;
  }

  when(): WhenStage {
    return new WhenStage(this.app, this.ctx, this.useCaseContext);
  }

  then(): ThenContractStage {
    return new ThenContractStage(this.app, this.ctx, this.useCaseContext);
  }
}
