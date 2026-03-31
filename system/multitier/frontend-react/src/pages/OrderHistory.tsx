import { Layout } from '../components';
import { OrderHistoryTable } from '../features/orders';
import { useOrders } from '../hooks';

/**
 * Order History page component for browsing past orders
 * Provides filtering by order number and displays order details in a table
 */
export function OrderHistory() {
  const { orders, filter, setFilter, isLoading, error, refresh } = useOrders();

  return (
    <Layout
      title="Order History"
      breadcrumbs={[{ label: 'Home', path: '/' }, { label: 'Order History' }]}
    >
      <OrderHistoryTable
        orders={orders}
        filter={filter}
        onFilterChange={setFilter}
        isLoading={isLoading}
        error={error}
        onRefresh={refresh}
      />
    </Layout>
  );
}
