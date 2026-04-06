import { createScenario, ExternalSystemMode } from '../../../src/test-setup';

const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

const nonExistentOrderCases = [
  { orderNumber: 'NON-EXISTENT-ORDER-99999', message: 'Order NON-EXISTENT-ORDER-99999 does not exist.' },
  { orderNumber: 'NON-EXISTENT-ORDER-88888', message: 'Order NON-EXISTENT-ORDER-88888 does not exist.' },
  { orderNumber: 'NON-EXISTENT-ORDER-77777', message: 'Order NON-EXISTENT-ORDER-77777 does not exist.' },
];

describe('ViewOrder Negative Test', () => {
  it.each(nonExistentOrderCases)(
    'shouldNotBeAbleToViewNonExistentOrder_API_$orderNumber',
    async ({ orderNumber, message }) => {
      const scenario = createScenario({ channel: 'api', externalSystemMode });
      try {
        await scenario
          .when()
          .viewOrder()
          .withOrderNumber(orderNumber)
          .then()
          .shouldFail()
          .errorMessage(message);
      } finally {
        await scenario.close();
      }
    },
  );
});
