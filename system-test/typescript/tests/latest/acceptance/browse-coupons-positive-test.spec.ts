import { test, forChannels } from './base/fixtures.js';

forChannels('ui', 'api')(() => {
    test('shouldBeAbleToBrowseCoupons', async ({ scenario }) => {
        await scenario
            .when()
            .browseCoupons()
            .then()
            .shouldSucceed();
    });
});
