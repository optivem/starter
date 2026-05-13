import { Column, Entity, PrimaryGeneratedColumn } from 'typeorm';
import { OrderStatus } from './order-status.enum';

@Entity('orders')
export class Order {
  @PrimaryGeneratedColumn('identity')
  id: number;

  @Column({ name: 'order_number', type: 'varchar', length: 255, unique: true, nullable: false })
  orderNumber: string;

  @Column({ name: 'order_timestamp', type: 'timestamptz', nullable: false })
  orderTimestamp: Date;

  @Column({ name: 'country', type: 'varchar', length: 255, nullable: false, default: () => "'US'" })
  country: string;

  @Column({ name: 'sku', type: 'varchar', length: 255, nullable: false })
  sku: string;

  @Column({ name: 'quantity', nullable: false })
  quantity: number;

  @Column({
    name: 'unit_price',
    type: 'numeric',
    precision: 10,
    scale: 2,
    nullable: false,
  })
  unitPrice: number;

  @Column({
    name: 'base_price',
    type: 'numeric',
    precision: 10,
    scale: 2,
    nullable: false,
    default: 0,
  })
  basePrice: number;

  @Column({
    name: 'discount_rate',
    type: 'numeric',
    precision: 5,
    scale: 4,
    nullable: false,
    default: 0,
  })
  discountRate: number;

  @Column({
    name: 'discount_amount',
    type: 'numeric',
    precision: 10,
    scale: 2,
    nullable: false,
    default: 0,
  })
  discountAmount: number;

  @Column({
    name: 'subtotal_price',
    type: 'numeric',
    precision: 10,
    scale: 2,
    nullable: false,
    default: 0,
  })
  subtotalPrice: number;

  @Column({
    name: 'tax_rate',
    type: 'numeric',
    precision: 5,
    scale: 4,
    nullable: false,
    default: 0,
  })
  taxRate: number;

  @Column({
    name: 'tax_amount',
    type: 'numeric',
    precision: 10,
    scale: 2,
    nullable: false,
    default: 0,
  })
  taxAmount: number;

  @Column({
    name: 'total_price',
    type: 'numeric',
    precision: 10,
    scale: 2,
    nullable: false,
  })
  totalPrice: number;

  @Column({ name: 'status', type: 'varchar', length: 50, nullable: false })
  status: OrderStatus;

  @Column({
    name: 'applied_coupon_code',
    type: 'varchar',
    length: 255,
    nullable: true,
    default: null,
  })
  appliedCouponCode: string | null;
}
