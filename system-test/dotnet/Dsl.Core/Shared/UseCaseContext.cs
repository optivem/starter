using Dsl.Port;
using System.Collections.Generic;


namespace Dsl.Core.Shared;

public class UseCaseContext
{
    private readonly Dictionary<string, string> _paramMap;
    private readonly Dictionary<string, string> _resultMap;

    public ExternalSystemMode ExternalSystemMode { get; }

    public UseCaseContext(ExternalSystemMode externalSystemMode)
    {
        ExternalSystemMode = externalSystemMode;
        _paramMap = new();
        _resultMap = new();
    }

    public string? GetParamValue(string? alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return alias;
        }

        if (_paramMap.TryGetValue(alias, out var value))
        {
            return value;
        }

        var generatedValue = GenerateParamValue(alias);
        _paramMap[alias] = generatedValue;

        return generatedValue;
    }

    public string? GetParamValueOrLiteral(string? alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return alias;
        }

        return ExternalSystemMode switch
        {
            ExternalSystemMode.Stub => GetParamValue(alias),
            ExternalSystemMode.Real => alias,
            _ => throw new InvalidOperationException($"Unsupported external system mode: {ExternalSystemMode}")
        };
    }

    public void SetResultEntry(string alias, string value)
    {
        EnsureAliasNotNullOrWhiteSpace(alias);

        if (_resultMap.ContainsKey(alias))
        {
            throw new InvalidOperationException($"Alias already exists: {alias}");
        }

        _resultMap[alias] = value;
    }

    public void SetResultEntryFailed(string alias, string errorMessage)
    {
        EnsureAliasNotNullOrWhiteSpace(alias);
        SetResultEntry(alias, $"FAILED: {errorMessage}");
    }

    public string? GetResultValue(string? alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return alias;
        }

        if (_resultMap.TryGetValue(alias, out var value))
        {
            if (value.Contains("FAILED"))
            {
                throw new InvalidOperationException($"Cannot get result value for alias '{alias}' because the operation failed: {value}");
            }
            return value;
        }

        return alias; // Return literal value if not found as alias
    }

    public string ExpandAliases(string message)
    {
        var expandedMessage = ExpandAlias(message, _paramMap);
        expandedMessage = ExpandAlias(expandedMessage, _resultMap);
        return expandedMessage;
    }

    private static string ExpandAlias(string message, Dictionary<string, string> map)
    {
        var expandedMessage = message;
        foreach (var entry in map)
        {
            var alias = entry.Key;
            var actualValue = entry.Value;
            expandedMessage = expandedMessage.Replace(alias, actualValue);
        }
        return expandedMessage;
    }

    private static string GenerateParamValue(string alias)
    {
        EnsureAliasNotNullOrWhiteSpace(alias);
        var suffix = Guid.NewGuid().ToString()[..8];
        return $"{alias}-{suffix}";
    }

    private static void EnsureAliasNotNullOrWhiteSpace(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException("Alias cannot be null or blank");
        }
    }
}
