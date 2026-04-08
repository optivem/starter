import {
  ErrorResponse,
  ViewOrderResponse,
  GetTimeResponse,
} from '../../../common/dtos';
import { DEFAULTS } from '../../defaults';
import { AppContext } from '../app-context';
import { ScenarioContext } from '../scenario-context';

export class ThenResultStage implements PromiseLike<void> {
  private _expectSuccess = true;
  private _orderAssertions: ((order: ViewOrderResponse) => void)[] = [];
  private _clockAssertions: ((time: GetTimeResponse) => void)[] = [];
  private _errorAssertions: ((error: ErrorResponse) => void)[] = [];
  private _executionPromise: Promise<void> | null = null;

  constructor(
    private readonly app: AppContext,
    private readonly ctx: ScenarioContext,
    private readonly sku: string,
    private readonly quantity: string | null,
    private readonly country: string = DEFAULTS.COUNTRY,
    private readonly couponCode: string | null = null,
  ) {}

  shouldSucceed(): ThenSuccess {
    this._expectSuccess = true;
    return new ThenSuccess(this);
  }

  shouldFail(): ThenFailure {
    this._expectSuccess = false;
    return new ThenFailure(this);
  }

  _addOrderAssertion(fn: (order: ViewOrderResponse) => void): void {
    this._orderAssertions.push(fn);
  }

  _addClockAssertion(fn: (time: GetTimeResponse) => void): void {
    this._clockAssertions.push(fn);
  }

  _addErrorAssertion(fn: (error: ErrorResponse) => void): void {
    this._errorAssertions.push(fn);
  }

  private async execute(): Promise<void> {
    if (this._executionPromise) return this._executionPromise;
    this._executionPromise = this._doExecute();
    return this._executionPromise;
  }

  private async _doExecute(): Promise<void> {
    if (this.ctx.clockConfig) {
      await this.app.clockDriver.returnsTime({ time: this.ctx.clockConfig.time });
    }

    for (const countryConfig of this.ctx.countryConfigs) {
      await this.app.taxDriver.returnsTaxRate({ country: countryConfig.country, taxRate: countryConfig.taxRate });
    }

    await this.app.erpDriver.returnsPromotion({
      promotionActive: this.ctx.promotionConfig.promotionActive,
      discount: this.ctx.promotionConfig.discount,
    });

    if (this.ctx.hasExplicitProduct) {
      for (const pc of this.ctx.productConfigs) {
        await this.app.erpDriver.returnsProduct({ sku: pc.sku, price: pc.price });
      }
    } else {
      await this.app.erpDriver.returnsProduct({ sku: DEFAULTS.SKU, price: DEFAULTS.UNIT_PRICE });
    }

    for (const cc of this.ctx.couponConfigs) {
      await this.app.shop().publishCoupon({
        code: cc.code,
        discountRate: cc.discountRate,
        validFrom: cc.validFrom,
        validTo: cc.validTo,
        usageLimit: cc.usageLimit,
      });
    }

    const result = await this.app.shop('dynamic').placeOrder({
      sku: this.sku,
      quantity: this.quantity,
      country: this.country,
      couponCode: this.couponCode,
    });

    if (this._expectSuccess) {
      expect(result.success).toBe(true);
      if (!result.success) return;

      if (this._orderAssertions.length > 0) {
        const orderResult = await this.app.shop().viewOrder(result.value.orderNumber);
        expect(orderResult.success).toBe(true);
        if (orderResult.success) {
          for (const fn of this._orderAssertions) fn(orderResult.value);
        }
      }

      if (this._clockAssertions.length > 0) {
        const timeResult = await this.app.clockDriver.getTime();
        expect(timeResult.success).toBe(true);
        if (timeResult.success) {
          for (const fn of this._clockAssertions) fn(timeResult.value);
        }
      }
    } else {
      expect(result.success).toBe(false);
      if (result.success) return;
      for (const fn of this._errorAssertions) fn(result.error);
    }
  }

  then<TResult1 = void, TResult2 = never>(
    onfulfilled?: ((value: void) => TResult1 | PromiseLike<TResult1>) | null,
    onrejected?: ((reason: unknown) => TResult2 | PromiseLike<TResult2>) | null,
  ): PromiseLike<TResult1 | TResult2> {
    return this.execute().then(onfulfilled, onrejected);
  }
}

export class ThenSuccess implements PromiseLike<void> {
  constructor(private readonly stage: ThenResultStage) {}

  and(): ThenSuccessAnd {
    return new ThenSuccessAnd(this.stage);
  }

  then<TResult1 = void, TResult2 = never>(
    onfulfilled?: ((value: void) => TResult1 | PromiseLike<TResult1>) | null,
    onrejected?: ((reason: unknown) => TResult2 | PromiseLike<TResult2>) | null,
  ): PromiseLike<TResult1 | TResult2> {
    return this.stage.then(onfulfilled, onrejected);
  }
}

export class ThenSuccessAnd implements PromiseLike<void> {
  constructor(private readonly stage: ThenResultStage) {}

  order(): ThenOrder {
    return new ThenOrder(this.stage);
  }

  clock(): ThenClock {
    return new ThenClock(this.stage);
  }

  then<TResult1 = void, TResult2 = never>(
    onfulfilled?: ((value: void) => TResult1 | PromiseLike<TResult1>) | null,
    onrejected?: ((reason: unknown) => TResult2 | PromiseLike<TResult2>) | null,
  ): PromiseLike<TResult1 | TResult2> {
    return this.stage.then(onfulfilled, onrejected);
  }
}

export class ThenOrder implements PromiseLike<void> {
  constructor(private readonly stage: ThenResultStage) {}

  hasOrderNumberPrefix(prefix: string): ThenOrder {
    this.stage._addOrderAssertion((order) => {
      expect(order.orderNumber.startsWith(prefix)).toBe(true);
    });
    return this;
  }

  hasStatus(status: string): ThenOrder {
    this.stage._addOrderAssertion((order) => {
      expect(order.status).toBe(status);
    });
    return this;
  }

  hasTotalPrice(price: number): ThenOrder {
    this.stage._addOrderAssertion((order) => {
      expect(order.totalPrice).toBe(price);
    });
    return this;
  }

  hasSubtotalPrice(price: number): ThenOrder {
    this.stage._addOrderAssertion((order) => {
      expect(order.subtotalPrice).toBe(price);
    });
    return this;
  }

  hasTaxRate(rate: number): ThenOrder {
    this.stage._addOrderAssertion((order) => {
      expect(order.taxRate).toBe(rate);
    });
    return this;
  }

  hasDiscountRate(rate: number): ThenOrder {
    this.stage._addOrderAssertion((order) => {
      expect(order.discountRate).toBe(rate);
    });
    return this;
  }

  hasAppliedCouponCode(code: string | null): ThenOrder {
    this.stage._addOrderAssertion((order) => {
      expect(order.appliedCouponCode).toBe(code);
    });
    return this;
  }

  hasBasePrice(price: number): ThenOrder {
    this.stage._addOrderAssertion((order) => {
      expect(order.basePrice).toBe(price);
    });
    return this;
  }

  hasDiscountAmount(amount: number): ThenOrder {
    this.stage._addOrderAssertion((order) => {
      expect(order.discountAmount).toBe(amount);
    });
    return this;
  }

  hasTaxAmount(amount: number): ThenOrder {
    this.stage._addOrderAssertion((order) => {
      expect(order.taxAmount).toBe(amount);
    });
    return this;
  }

  then<TResult1 = void, TResult2 = never>(
    onfulfilled?: ((value: void) => TResult1 | PromiseLike<TResult1>) | null,
    onrejected?: ((reason: unknown) => TResult2 | PromiseLike<TResult2>) | null,
  ): PromiseLike<TResult1 | TResult2> {
    return this.stage.then(onfulfilled, onrejected);
  }
}

export class ThenClock implements PromiseLike<void> {
  constructor(private readonly stage: ThenResultStage) {}

  hasTime(time?: string): ThenClock {
    this.stage._addClockAssertion((t) => {
      if (time) {
        expect(t.time).toContain(time);
      } else {
        expect(t.time).toBeTruthy();
      }
    });
    return this;
  }

  then<TResult1 = void, TResult2 = never>(
    onfulfilled?: ((value: void) => TResult1 | PromiseLike<TResult1>) | null,
    onrejected?: ((reason: unknown) => TResult2 | PromiseLike<TResult2>) | null,
  ): PromiseLike<TResult1 | TResult2> {
    return this.stage.then(onfulfilled, onrejected);
  }
}

export class ThenFailure implements PromiseLike<void> {
  constructor(private readonly stage: ThenResultStage) {}

  errorMessage(expected: string): ThenFailure {
    this.stage._addErrorAssertion((error) => {
      expect(error.message).toBe(expected);
    });
    return this;
  }

  fieldErrorMessage(field: string, message: string): ThenFailure {
    this.stage._addErrorAssertion((error) => {
      const fieldError = error.fieldErrors.find((fe) => fe.field === field);
      expect(fieldError).toBeDefined();
      expect(fieldError!.message).toBe(message);
    });
    return this;
  }

  then<TResult1 = void, TResult2 = never>(
    onfulfilled?: ((value: void) => TResult1 | PromiseLike<TResult1>) | null,
    onrejected?: ((reason: unknown) => TResult2 | PromiseLike<TResult2>) | null,
  ): PromiseLike<TResult1 | TResult2> {
    return this.stage.then(onfulfilled, onrejected);
  }
}
