import { NextRequest, NextResponse } from 'next/server';
import { findCouponByCode } from '@/lib/db';
import { notFoundResponse, internalErrorResponse } from '@/lib/errors';

function formatTimestamp(date: Date): string {
  return date.toISOString().replace('.000Z', 'Z');
}

export async function GET(
  _request: NextRequest,
  { params }: { params: Promise<{ code: string }> }
) {
  try {
    const { code } = await params;
    const coupon = await findCouponByCode(code);

    if (!coupon) {
      return notFoundResponse(`Coupon ${code} does not exist.`);
    }

    return NextResponse.json({
      code: coupon.code,
      discountRate: Number.parseFloat(coupon.discount_rate),
      validFrom: coupon.valid_from ? formatTimestamp(coupon.valid_from) : null,
      validTo: coupon.valid_to ? formatTimestamp(coupon.valid_to) : null,
      usageLimit: coupon.usage_limit,
      usedCount: coupon.used_count,
    });
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    return internalErrorResponse(message);
  }
}
