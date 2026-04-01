using Dsl.Core.Shared;
using Common;
using Dsl.Core.Shop.UseCases.Base;
using Driver.Port.Shop.Dtos.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsl.Core.Scenario
{
    public class ExecutionResultBuilder<TSuccessResponse, TSuccessVerification>
        where TSuccessVerification : ResponseVerification<TSuccessResponse>
    {
        private readonly ShopUseCaseResult<TSuccessResponse, TSuccessVerification> _result;
        private string? _orderNumber;

        internal ExecutionResultBuilder(UseCaseResult<TSuccessResponse, SystemError, TSuccessVerification, SystemErrorFailureVerification> result)
        {
            // Cast to derived type - the result is always a ShopUseCaseResult at runtime
            _result = (ShopUseCaseResult<TSuccessResponse, TSuccessVerification>)result;
        }

        public ExecutionResultBuilder<TSuccessResponse, TSuccessVerification> OrderNumber(string? orderNumber)
        {
            _orderNumber = orderNumber;
            return this;
        }

        public ExecutionResult<TSuccessResponse, TSuccessVerification> Build()
        {
            return new ExecutionResult<TSuccessResponse, TSuccessVerification>(
                _result,
                _orderNumber);
        }
    }
}



