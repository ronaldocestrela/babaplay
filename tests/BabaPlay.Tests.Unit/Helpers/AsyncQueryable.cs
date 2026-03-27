using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace BabaPlay.Tests.Unit.Helpers;

/// <summary>
/// In-memory IQueryable backed by a LINQ-to-Objects queryable that also supports
/// EF Core async operators (ToListAsync, AnyAsync, FirstOrDefaultAsync, etc.).
/// Implements both IOrderedQueryable and IAsyncEnumerable so that OrderBy/ThenBy
/// chains and async enumeration work correctly.
/// </summary>
internal sealed class AsyncQueryable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
{
    // The underlying LINQ-to-Objects query (preserves the full expression tree)
    internal readonly IQueryable<T> Inner;

    public AsyncQueryable(IEnumerable<T> data) => Inner = data.AsQueryable();
    internal AsyncQueryable(IQueryable<T> inner) => Inner = inner;

    public Type ElementType => Inner.ElementType;
    public Expression Expression => Inner.Expression;
    public IQueryProvider Provider => new AsyncQueryableProvider<T>(Inner);

    public IEnumerator<T> GetEnumerator() => Inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Inner.GetEnumerator();

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new AsyncEnumerator<T>(Inner.GetEnumerator());
}

internal sealed class AsyncQueryableProvider<T> : IQueryProvider, IAsyncQueryProvider
{
    private readonly IQueryable<T> _inner;

    public AsyncQueryableProvider(IQueryable<T> inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => CreateQuery<T>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        // EF Core Include/ThenInclude: not supported by LINQ-to-Objects.
        // Return the same source so the chain continues with our in-memory data.
        // Navigation properties must be pre-populated on the entities in the test setup.
        if (expression is MethodCallExpression call &&
            call.Method.Name is "Include" or "ThenInclude")
        {
            return new AsyncQueryable<TElement>(_inner.OfType<TElement>());
        }

        // Delegate to the LINQ-to-Objects provider so the full expression tree
        // (OrderBy, ThenBy, Where, Select, etc.) is preserved and chains correctly.
        var newInner = _inner.Provider.CreateQuery<TElement>(expression);
        return new AsyncQueryable<TElement>(newInner);
    }

    public object? Execute(Expression expression) => _inner.Provider.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Provider.Execute<TResult>(expression);

    /// <summary>
    /// Called by EF Core async operators (ToListAsync, AnyAsync, FirstOrDefaultAsync…).
    /// TResult is Task&lt;X&gt;; we unwrap to X, execute synchronously, then wrap in Task.
    /// </summary>
    TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        // TResult = Task<X> → unwrap to X
        var resultType = typeof(TResult).GetGenericArguments()[0];

        var executeGeneric = typeof(IQueryProvider)
            .GetMethods()
            .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
            .MakeGenericMethod(resultType);

        object? syncResult;
        try
        {
            syncResult = executeGeneric.Invoke(_inner.Provider, [expression]);
        }
        catch
        {
            syncResult = resultType.IsValueType ? Activator.CreateInstance(resultType) : null;
        }

        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, [syncResult])!;
    }
}

internal sealed class AsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(inner.MoveNext());
    public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
}

internal static class AsyncQueryableExtensions
{
    /// <summary>Returns an IQueryable that supports EF Core async LINQ operators.</summary>
    public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
        => new AsyncQueryable<T>(source);
}
