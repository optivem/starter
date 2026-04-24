import { expect, type TestType } from '@playwright/test';

// Playwright's TestType is invariant in its fixture shape, so we accept any test
// type and rely on the runtime destructuring of `myShopDriver` from fixtures.
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function runMyShopBaseSmokeTest(test: TestType<any, any>): void {
  test('shouldBeAbleToGoToMyShop', async ({ myShopDriver }) => {
    const result = await myShopDriver.goToMyShop();
    expect(result.success).toBe(true);
  });
}
