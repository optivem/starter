import { expect, type TestType } from '@playwright/test';

// Playwright's TestType is invariant in its fixture shape, so we accept any test
// type and rely on the runtime destructuring of `shopDriver` from fixtures.
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function runShopBaseSmokeTest(test: TestType<any, any>): void {
  test('shouldBeAbleToGoToShop', async ({ shopDriver }) => {
    const result = await shopDriver.goToShop();
    expect(result.success).toBe(true);
  });
}
