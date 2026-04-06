import { createScenario, ExternalSystemMode } from '../../../src/test-setup';
import { OrderStatus } from '../../../src/common/dtos';

const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

const nonExistentOrderCases = [
  { orderNumber: 'NON-EXISTENT-ORDER-99999', message: 'Order NON-EXISTENT-ORDER-99999 does not exist.' },
  { orderNumber: 'NON-EXISTENT-ORDER-88888', message: 'Order NON-EXISTENT-ORDER-88888 does not exist.' },
  { orderNumber: 'NON-EXISTENT-ORDER-77777', message: 'Order NON-EXISTENT-ORDER-77777 does not exist.' },
];

describe('CancelOrder Negative Test', () => {
  it.each(nonExistentOrderCases)(
    'shouldNotCancelNonExistentOrder_API_$orderNumber',
    async ({ orderNumber, message }) => {
      const scenario = createScenario({ channel: 'api', externalSystemMode });
      try {
        await scenario
          .when()
          .cancelOrder()
          .withOrderNumber(orderNumber)
          .then()
          .shouldFail()
          .errorMessage(message);
      } finally {
        await scenario.close();
      }
    },
  );

  it('shouldNotCancelAlreadyCancelledOrder_API', async () => {
    const scenario = createScenario({ channel: 'api', externalSystemMode });
    try {
      await scenario
        .given()
        .order()
        .withStatus(OrderStatus.CANCELLED)
        .when()
        .cancelOrder()
        .then()
        .shouldFail()
        .errorMessage('Order has already been cancelled');
    } finally {
      await scenario.close();
    }
  });

  it('cannotCancelNonExistentOrder_API', async () => {
    const scenario = createScenario({ channel: 'api', externalSystemMode });
    try {
      await scenario
        .when()
        .cancelOrder()
        .withOrderNumber('non-existent-order-12345')
        .then()
        .shouldFail()
        .errorMessage('Order non-existent-order-12345 does not exist.');
    } finally {
      await scenario.close();
    }
  });
});
