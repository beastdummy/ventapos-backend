
using System.Data;

namespace Ventapos.Api.Data;

public static class DbExtensions
{
    /// <summary>
    /// Permite usar 'await conn.BeginTransactionAsync()' aunque la variable sea IDbConnection.
    /// Internamente usa BeginTransaction síncrono.
    /// </summary>
    public static Task<IDbTransaction> BeginTransactionAsync(this IDbConnection conn, CancellationToken ct = default)
        => Task.FromResult(conn.BeginTransaction());
}
