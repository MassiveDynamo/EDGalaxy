using MySqlConnector;
using Serilog.Core;

internal class DB
{
    internal static async Task<HashSet<int>> GetAllIds(Logger logger, string? dbConnection)
    {
        HashSet<int> ids = [];
        using var connection = new MySqlConnection(dbConnection);
        await connection.OpenAsync();

        using var command = new MySqlCommand("SELECT Id FROM system;", connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var value = reader.GetInt32(0);
            ids.Add(value);
            if( ids.Count % 1000000  == 0 )
            {
                logger.Information("Ids read: {rowsRead}", ids.Count);
            }
        }

        return ids;
    }
}