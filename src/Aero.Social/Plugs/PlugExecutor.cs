using Aero.Social.Abstractions;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Plugs;

/// <summary>
/// Default implementation of the plug executor
/// </summary>
public class PlugExecutor(ILogger<PlugExecutor>? logger = null) : IPlugExecutor
{
    /// <inheritdoc />
    public async Task<PlugExecutionResult> ExecuteAsync(
        Func<PlugExecutionContext, CancellationToken, Task<PlugExecutionResult>>? plugExecute,
        ISocialProvider provider,
        PlugExecutionContext context,
        Dictionary<string, object>? fieldValues = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation(
                "Executing plug on provider {ProviderIdentifier}",
                provider.Identifier);

            if (plugExecute is null)
            {
                return PlugExecutionResult.FailedResult("Plug has no execute delegate assigned.");
            }

            return await plugExecute(context, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to execute plug on provider {ProviderIdentifier}", provider.Identifier);
            return PlugExecutionResult.FailedResult(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public PlugValidationResult ValidateFields(PostPlugAttribute attribute, Dictionary<string, object>? fieldValues)
    {
        var result = new PlugValidationResult();

        foreach (var field in attribute.Fields)
        {
            var value = fieldValues?.GetValueOrDefault(field.Name);

            foreach (var validation in field.Validations)
            {
                if (!validation.Validate(value))
                {
                    result.AddError(field.Name, validation.ErrorMessage ?? $"Validation failed for {field.Name}");
                }
            }
        }

        return result;
    }

    /// <inheritdoc />
    public PlugValidationResult ValidateFields(PlugAttribute attribute, Dictionary<string, object>? fieldValues)
    {
        var result = new PlugValidationResult();

        foreach (var field in attribute.Fields)
        {
            var value = fieldValues?.GetValueOrDefault(field.Name);

            foreach (var validation in field.Validations)
            {
                if (!validation.Validate(value))
                {
                    result.AddError(field.Name, validation.ErrorMessage ?? $"Validation failed for {field.Name}");
                }
            }
        }

        return result;
    }

    /// <inheritdoc />
    public bool ShouldExecute(PostPlugAttribute attribute, DateTime? lastRunTime, int executionCount)
    {
        // Check if we've exceeded the total runs
        if (attribute.TotalRuns > 0 && executionCount >= attribute.TotalRuns)
        {
            logger?.LogDebug(
                "Plug {PlugIdentifier} has reached max runs ({ExecutionCount}/{TotalRuns})",
                attribute.Identifier,
                executionCount,
                attribute.TotalRuns);
            return false;
        }

        // Check if enough time has passed since last run
        if (lastRunTime.HasValue)
        {
            var nextRunTime = lastRunTime.Value.AddMilliseconds(attribute.RunEveryMilliseconds);
            if (DateTime.UtcNow < nextRunTime)
            {
                logger?.LogDebug(
                    "Plug {PlugIdentifier} not ready to run. Next run at {NextRunTime}",
                    attribute.Identifier,
                    nextRunTime);
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public bool ShouldExecute(PlugAttribute attribute, DateTime? lastRunTime, int executionCount)
    {
        // Check if we've exceeded the total runs
        if (attribute.TotalRuns > 0 && executionCount >= attribute.TotalRuns)
        {
            logger?.LogDebug(
                "Plug {PlugIdentifier} has reached max runs ({ExecutionCount}/{TotalRuns})",
                attribute.Identifier,
                executionCount,
                attribute.TotalRuns);
            return false;
        }

        // Check if enough time has passed since last run
        if (lastRunTime.HasValue)
        {
            var nextRunTime = lastRunTime.Value.AddMilliseconds(attribute.RunEveryMilliseconds);
            if (DateTime.UtcNow < nextRunTime)
            {
                logger?.LogDebug(
                    "Plug {PlugIdentifier} not ready to run. Next run at {NextRunTime}",
                    attribute.Identifier,
                    nextRunTime);
                return false;
            }
        }

        return true;
    }}
