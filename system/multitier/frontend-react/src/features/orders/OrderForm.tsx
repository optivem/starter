import { FormEvent } from 'react';
import { FormInput, SubmitButton } from '../../components';
import type { OrderFormData } from '../../types/form.types';

export interface OrderFormProps {
  formData: OrderFormData;
  onFormChange: (updates: Partial<OrderFormData>) => void;
  onSubmit: (e: FormEvent) => void;
  isSubmitting: boolean;
}

/**
 * Order placement form component
 * Collects SKU and quantity
 */
export function OrderForm({ formData, onFormChange, onSubmit, isSubmitting }: Readonly<OrderFormProps>) {
  return (
    <div className="card shadow">
      <div className="card-header bg-primary text-white">
        <h4 className="mb-0">Place Your Order</h4>
      </div>
      <div className="card-body">
        <form onSubmit={onSubmit}>
          <FormInput
            label="SKU"
            value={formData.sku}
            onChange={(e) => onFormChange({ sku: e.target.value })}
            placeholder="Enter product SKU"
            ariaLabel="SKU"
          />
          <FormInput
            label="Quantity"
            value={formData.quantityValue}
            onChange={(e) => onFormChange({
              quantityValue: e.target.value,
              quantity: Number.parseInt(e.target.value) || 0
            })}
            inputMode="numeric"
            placeholder="Enter quantity"
            ariaLabel="Quantity"
          />
          <div className="d-grid">
            <SubmitButton
              isSubmitting={isSubmitting}
              text="Place Order"
              loadingText="Placing Order..."
              ariaLabel="Place Order"
              className="btn btn-primary btn-lg"
            />
          </div>
        </form>
      </div>
    </div>
  );
}
