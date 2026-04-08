import { ErrorResponse } from '../../../common/dtos';
import { AppContext } from '../app-context';
import { ScenarioContext } from '../scenario-context';

export class ThenPublishCouponResultStage implements PromiseLike<void> {
  private _expectSuccess = true;
  private _errorAssertions: ((error: ErrorResponse) => void)[] = [];
  private _executionPromise: Promise<void> | null = null;

  constructor(
    private readonly app: AppContext,
    private readonly ctx: ScenarioContext,
    private readonly code: string,
    private readonly discountRate: number,
    private readonly validFrom?: string,
    private readonly validTo?: string,
    private readonly usageLimit?: number | string,
  ) {}

  shouldSucceed(): ThenPublishCouponSuccess {
    this._expectSuccess = true;
    return new ThenPublishCouponSuccess(this);
  }

  shouldFail(): ThenPublishCouponFailure {
    this._expectSuccess = false;
    return new ThenPublishCouponFailure(this);
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
    // Set up given coupons first (for duplicate tests)
    for (const cc of this.ctx.couponConfigs) {
      await this.app.shop().publishCoupon({
        code: cc.code,
        discountRate: cc.discountRate,
        validFrom: cc.validFrom,
        validTo: cc.validTo,
        usageLimit: cc.usageLimit,
      });
    }

    const result = await this.app.shop('static').publishCoupon({
      code: this.code,
      discountRate: this.discountRate,
      validFrom: this.validFrom,
      validTo: this.validTo,
      usageLimit: this.usageLimit,
    });

    if (this._expectSuccess) {
      expect(result.success).toBe(true);
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

export class ThenPublishCouponSuccess implements PromiseLike<void> {
  constructor(private readonly stage: ThenPublishCouponResultStage) {}

  then<TResult1 = void, TResult2 = never>(
    onfulfilled?: ((value: void) => TResult1 | PromiseLike<TResult1>) | null,
    onrejected?: ((reason: unknown) => TResult2 | PromiseLike<TResult2>) | null,
  ): PromiseLike<TResult1 | TResult2> {
    return this.stage.then(onfulfilled, onrejected);
  }
}

export class ThenPublishCouponFailure implements PromiseLike<void> {
  constructor(private readonly stage: ThenPublishCouponResultStage) {}

  errorMessage(expected: string): ThenPublishCouponFailure {
    this.stage._addErrorAssertion((error) => {
      expect(error.message).toBe(expected);
    });
    return this;
  }

  fieldErrorMessage(field: string, message: string): ThenPublishCouponFailure {
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
