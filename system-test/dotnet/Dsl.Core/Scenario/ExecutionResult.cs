using Dsl.Core.Shared;
using Common;
using Dsl.Core.Shop.UseCases.Base;

namespace Dsl.Core.Scenario
{
    public class ExecutionResult<TSuccessResponse, TSuccessVerification>
        where TSuccessVerification : ResponseVerification<TSuccessResponse>
    {
        internal ExecutionResult(ShopUseCaseResult<TSuccessResponse, TSuccessVerification> result,
            string? orderNumber)
        {
            if (result == null)
            {
                throw new ArgumentException("Result cannot be null");
            }

            Result = result;
            OrderNumber = orderNumber;
            Context = new ExecutionResultContext(orderNumber);
        }

        public ShopUseCaseResult<TSuccessResponse, TSuccessVerification> Result { get; }

        public string? OrderNumber { get; }

        /// <summary>
        /// Context with order number from the executed operation.
        /// </summary>
        public ExecutionResultContext Context { get; }
    }
}


