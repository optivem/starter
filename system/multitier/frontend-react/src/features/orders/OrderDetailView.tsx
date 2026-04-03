import { DetailField } from '../../components/DetailField';
import type { ViewOrderDetailsResponse } from '../../types/api.types';

export interface OrderDetailViewProps {
  order: ViewOrderDetailsResponse;
}

/**
 * Order detail view component for displaying comprehensive order information
 * Renders all order fields in a consistent grid layout
 */
export function OrderDetailView({ order }: Readonly<OrderDetailViewProps>) {
  return (
    <div className="row">
      <DetailField label="Order Number" value={order.orderNumber} ariaLabel="Display Order Number" />
      <DetailField label="Order Timestamp" value={new Date(order.orderTimestamp).toLocaleString('en-US', { timeZone: 'UTC' })} ariaLabel="Display Order Timestamp" />
      <DetailField
        label="Status"
        value={order.status}
        valueClassName={`status-${order.status}`}
        ariaLabel="Display Status"
      />
      <DetailField label="SKU" value={order.sku} ariaLabel="Display SKU" />
      <DetailField label="Quantity" value={order.quantity} ariaLabel="Display Quantity" />
      <DetailField label="Unit Price" value={`$${order.unitPrice.toFixed(2)}`} ariaLabel="Display Unit Price" />
      <DetailField
        label="Total Price"
        value={`$${order.totalPrice.toFixed(2)}`}
        valueClassName="fs-5 fw-bold"
        ariaLabel="Display Total Price"
      />
    </div>
  );
}
