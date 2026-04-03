import { NextRequest, NextResponse } from 'next/server';
import { findByOrderNumber } from '@/lib/db';
import { notFoundResponse, internalErrorResponse } from '@/lib/errors';

export async function GET(
  _request: NextRequest,
  { params }: { params: Promise<{ orderNumber: string }> }
) {
  try {
    const { orderNumber } = await params;
    const order = await findByOrderNumber(orderNumber);

    if (!order) {
      return notFoundResponse(`Order ${orderNumber} does not exist.`);
    }

    return NextResponse.json({
      orderNumber: order.order_number,
      orderTimestamp: order.order_timestamp.toISOString(),
      sku: order.sku,
      quantity: order.quantity,
      unitPrice: Number.parseFloat(order.unit_price),
      totalPrice: Number.parseFloat(order.total_price),
      status: order.status,
    });
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    return internalErrorResponse(message);
  }
}
