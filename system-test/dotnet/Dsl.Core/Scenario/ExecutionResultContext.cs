namespace Dsl.Core.Scenario;

/// <summary>
/// Context extracted from execution result - order number from the executed operation.
/// </summary>
public class ExecutionResultContext
{
    private static readonly ExecutionResultContext _empty = new(null);

    public ExecutionResultContext(string? orderNumber)
    {
        OrderNumber = orderNumber;
    }

    public static ExecutionResultContext Empty() => _empty;

    public string? OrderNumber { get; }
}

