using Dsl.Core.Shared;
using Common;
using Dsl.Core.MyShop.UseCases.Base;

namespace Dsl.Core.Scenario
{
    public class ExecutionResult<TSuccessResponse, TSuccessVerification>
        where TSuccessVerification : ResponseVerification<TSuccessResponse>
    {
        internal ExecutionResult(MyShopUseCaseResult<TSuccessResponse, TSuccessVerification> result,
            string? orderNumber, string? couponCode)
        {
            if (result == null)
            {
                throw new ArgumentException("Result cannot be null");
            }

            Result = result;
            OrderNumber = orderNumber;
            CouponCode = couponCode;
            Context = new ExecutionResultContext(orderNumber, couponCode);
        }

        public MyShopUseCaseResult<TSuccessResponse, TSuccessVerification> Result { get; }

        public string? OrderNumber { get; }

        public string? CouponCode { get; }

        /// <summary>
        /// Context with order number and coupon code from the executed operation.
        /// </summary>
        public ExecutionResultContext Context { get; }
    }
}


