using Elsa.Attributes;
using Elsa.Models;

namespace Elsa.Activities;

/// <summary>
/// Represents an inline code activity that can be used to execute arbitrary .NET code from a workflow.
/// </summary>
[Activity("Elsa", "Primitives", "Evaluate a Boolean condition to determine which path to execute next.")]
public class Inline : Activity
{
    private readonly Func<ActivityExecutionContext, ValueTask> _activity;

    public Inline(Func<ActivityExecutionContext, ValueTask> activity) => _activity = activity;

    public Inline(Func<ValueTask> activity) : this(_ => activity())
    {
    }

    public Inline(Action<ActivityExecutionContext> activity) : this(c =>
    {
        activity(c);
        return new ValueTask();
    })
    {
    }

    public Inline(Action activity) : this(c =>
    {
        activity();
        return new ValueTask();
    })
    {
    }

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context) => await _activity(context);

    public static Inline From(Func<ActivityExecutionContext, ValueTask> activity) => new(activity);
    public static Inline From(Func<ValueTask> activity) => new(activity);
    public static Inline From(Action<ActivityExecutionContext> activity) => new(activity);
    public static Inline From(Action activity) => new(activity);

    public static Inline<T> From<T>(Func<ActivityExecutionContext, ValueTask<T>> activity) => new(activity);
    public static Inline<T> From<T>(Func<ValueTask<T>> activity) => new(activity);
    public static Inline<T> From<T>(Func<ActivityExecutionContext, T> activity) => new(activity);
    public static Inline<T> From<T>(Func<T> activity) => new(activity);
}

/// <summary>
/// Represents an inline code activity that can be used to execute arbitrary .NET code from a workflow and return a value.
/// </summary>
public class Inline<T> : Activity<T>
{
    private readonly Func<ActivityExecutionContext, ValueTask<T>> _activity;

    public Inline(Func<ActivityExecutionContext, ValueTask<T>> activity, RegisterLocationReference? output = default)
    {
        _activity = activity;
        if (output != null) Result = new Output<T?>(output);
    }

    public Inline(Func<ValueTask<T>> activity, RegisterLocationReference? output = default) : this(_ => activity(), output)
    {
    }

    public Inline(Func<ActivityExecutionContext, T> activity, RegisterLocationReference? output = default) : this(c =>
    {
        var result = activity(c);
        return new ValueTask<T>(result);
    }, output)
    {
    }

    public Inline(Func<T> activity, RegisterLocationReference? output = default) : this(c =>
    {
        var result = activity();
        return new ValueTask<T>(result);
    }, output)
    {
    }

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var result = await _activity(context);
        context.Set(Result, result);
    }
}