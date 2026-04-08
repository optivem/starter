import { ErrorResponse, ViewOrderResponse } from '../../../common/dtos';
import { DEFAULTS } from '../../defaults';
import { AppContext } from '../app-context';
import { ScenarioContext } from '../scenario-context';

export class ThenCancelOrderResultStage implements PromiseLike<void> {
  private _expectSuccess = true;
  private _orderAssertions: ((order: ViewOrderResponse) => void)[] = [];
  private _errorAssertions: ((error: ErrorResponse) => void)[] = [];
  private _executionPromise: Promise<void> | null = null;

  constructor(
    private readonly app: AppContext,
    private readonly ctx: ScenarioContext,
    private readonly orderNumber: string,
  ) {}

  shouldSucceed(): ThenCancelOrderSuccess {
    this._expectSuccess = true;
    return new ThenCancelOrderSuccess(this);
  }

  shouldFail(): ThenCancelOrderFailure {
    this._expectSuccess = false;
    return new ThenCancelOrderFailure(this);
  }

  _addOrderAssertion(fn: (order: ViewOrderResponse) => void): void {
    this._orderAssertions.push(fn);
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

    // Place any given orders
    for (const oc of this.ctx.orderConfigs) {
      const placeResult = await this.app.shop().placeOrder({
        sku: oc.sku,
        quantity: oc.quantity,
        country: oc.country,
        couponCode: oc.couponCode,
      });
      if (placeResult.success) {
        oc.orderNumber = placeResult.value.orderNumber;
        if (oc.status === 'CANCELLED') {
          await this.app.shop().cancelOrder(placeResult.value.orderNumber);
        }
        if (oc.status === 'DELIVERED') {
          await this.app.shop().deliverOrder(placeResult.value.orderNumber);
        }
      }
    }

    const targetOrderNumber = this.ctx.orderConfigs.length > 0 && this.ctx.orderConfigs[0].orderNumber
      ? this.ctx.orderConfigs[0].orderNumber
      : this.orderNumber;

    const result = await this.app.shop('dynamic').cancelOrder(targetOrderNumber);

    if (this._expectSuccess) {
      expect(result.success).toBe(true);

      if (this._orderAssertions.length > 0) {
        const orderResult = await this.app.shop().viewOrder(targetOrderNumber);
        expect(orderResult.success).toBe(true);
        if (orderResult.success) {
          for (const fn of this._orderAssertions) fn(orderResult.value);
        }
      }
    } else {
      expect(result.success).toBe(false);
      if (!result.success) {
        for (const fn of this._errorAssertions) fn(result.error);
      }
    }
  }

  then<TResult1 = void, TResult2 = never>(
    onfulfilled?: ((value: void) => TResult1 | PromiseLike<TResult1>) | null,
    onrejected?: ((reason: unknown) => TResult2 | PromiseLike<TResult2>) | null,
  ): PromiseLike<TResult1 | TResult2> {
    return this.execute().then(onfulfilled, onrejected);
  }
}

export class ThenCancelOrderSuccess implements PromiseLike<void> {
  constructor(private readonly stage: ThenCancelOrderResultStage) {}

  and(): ThenCancelOrderSuccessAnd {
    return new ThenCancelOrderSuccessAnd(this.stage);
  }

  then<TResult1 = void, TResult2 = never>(
    onfulfilled?: ((value: void) => TResult1 | PromiseLike<TResult1>) | null,
    onrejected?: ((reason: unknown) => TResult2 | PromiseLike<TResult2>) | null,
  ): PromiseLike<TResult1 | TResult2> {
    return this.stage.then(onfulfilled, onrejected);
  }
}

export class ThenCancelOrderSuccessAnd implements PromiseLike<void> {
  constructor(private readonly stage: ThenCancelOrderResultStage) {}

  order(): ThenCancelOrderOrder {
    return new ThenCancelOrderOrder(this.stage);
  }

  then<TResult1 = void, TResult2 = never>(
    onfulfilled?: ((value: void) => TResult1 | PromiseLike<TResult1>) | null,
    onrejected?: ((reason: unknown) => TResult2 | PromiseLike<TResult2>) | null,
  ): PromiseLike<TResult1 | TResult2> {
    return this.stage.then(onfulfilled, onrejected);
  }
}

export class ThenCancelOrderOrder implements PromiseLike<void> {
  constructor(private readonly stage: ThenCancelOrderResultStage) {}

  hasStatus(status: string): ThenCancelOrderOrder {
    this.stage._addOrderAssertion((order) => {
      expect(order.status).toBe(status);
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

export class ThenCancelOrderFailure implements PromiseLike<void> {
  constructor(private readonly stage: ThenCancelOrderResultStage) {}

  errorMessage(expected: string): ThenCancelOrderFailure {
    this.stage._addErrorAssertion((error) => {
      expect(error.message).toBe(expected);
    });
    return this;
  }

  fieldErrorMessage(field: string, message: string): ThenCancelOrderFailure {
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
