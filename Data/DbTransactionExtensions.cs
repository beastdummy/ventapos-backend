
using System.Data;

namespace Ventapos.Api.Data;

public static class DbTransactionExtensions
{
    /// <summary>
    /// Permite usar 'await tx.CommitAsync()' sobre IDbTransaction (envuelve Commit síncrono).
    /// </summary>
    public static Task CommitAsync(this IDbTransaction tx, CancellationToken ct = default)
    {
        tx.Commit();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Permite usar 'await tx.RollbackAsync()' sobre IDbTransaction (envuelve Rollback síncrono).
    /// </summary>
    public static Task RollbackAsync(this IDbTransaction tx, CancellationToken ct = default)
    {
        tx.Rollback();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Opcional: compatibilidad con 'await using' si alguien intenta usar DisposeAsync.
    /// </summary>
    public static ValueTask DisposeAsync(this IDbTransaction tx)
    {
        tx.Dispose();
        return ValueTask.CompletedTask;
    }
}
