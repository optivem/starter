using System.Runtime.CompilerServices;
using Driver.Port.Shop.Dtos;

namespace Dsl.Port.Then.Steps;

public interface IThenOrder
{
    IThenOrder HasSku(string expectedSku);

    IThenOrder HasQuantity(int expectedQuantity);

    IThenOrder HasUnitPrice(decimal expectedUnitPrice);

    IThenOrder HasTotalPrice(decimal expectedTotalPrice);

    IThenOrder HasTotalPrice(string expectedTotalPrice);

    IThenOrder HasStatus(OrderStatus expectedStatus);

    IThenOrder HasOrderNumberPrefix(string expectedPrefix);

    IThenOrder HasTotalPriceGreaterThanZero();

    TaskAwaiter GetAwaiter();
}
