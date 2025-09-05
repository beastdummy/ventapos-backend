
using System.Data;
using MySqlConnector;

namespace Ventapos.Api.Data;

public sealed class Db
{
    private readonly string _connectionString;
    public Db(string connectionString) => _connectionString = connectionString;

    public async Task<IDbConnection> OpenAsync()
    {
        var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
}
