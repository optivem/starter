import { createScenario, ExternalSystemMode } from '../../../src/test-setup';
import { OrderStatus } from '../../../src/common/dtos';

const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

describe('CancelOrder Positive Test', () => {
  it('shouldHaveCancelledStatusWhenCancelled_API', async () => {
    const scenario = createScenario({ channel: 'api', externalSystemMode });
    try {
      await scenario
        .given()
        .order()
        .when()
        .cancelOrder()
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasStatus(OrderStatus.CANCELLED);
    } finally {
      await scenario.close();
    }
  });
});
