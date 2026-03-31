import { useParams, useNavigate } from 'react-router-dom';
import { Layout, DataState } from '../components';
import { OrderDetailView } from '../features/orders';
import { useOrderDetails } from '../hooks';

/**
 * Order Details page component for viewing individual order information
 * Allows users to view detailed order information
 */
export function OrderDetails() {
  const { orderNumber } = useParams<{ orderNumber: string }>();
  const navigate = useNavigate();
  const { order, isLoading, error } = useOrderDetails(orderNumber);

  return (
    <Layout
      title="Order Details"
      breadcrumbs={[
        { label: 'Home', path: '/' },
        { label: 'Order History', path: '/order-history' },
        { label: 'Order Details' }
      ]}
    >
      <div className="card shadow">
        <div className="card-header bg-primary text-white">
          <h4 className="mb-0">Order Details</h4>
        </div>
        <div className="card-body">
          <DataState
            isLoading={isLoading}
            error={error}
            isEmpty={!order}
            loadingMessage="Loading order details..."
            emptyMessage="Order not found"
          >
            {order && (
              <>
                <OrderDetailView order={order} />
                <div className="mt-4">
                  <button
                    className="btn btn-secondary"
                    onClick={() => navigate('/order-history')}
                  >
                    Back to Order History
                  </button>
                </div>
              </>
            )}
          </DataState>
        </div>
      </div>
    </Layout>
  );
}
