using AvaloniaChat.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace AvaloniaChat.Services
{
    public class ConfigService(/*ILogger<ConfigService> logger*/)
    {

        public async Task InitializeDatabase()
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Config (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Key TEXT,
                Value TEXT
            );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                //logger.LogError(e, "Error creating Category table");
                throw;
            }

        }

        public static async Task<ImmutableArray<Config>> ListAsync()
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT ID,Key,Value FROM Config";
            List<Config> configs = [];
            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Config config = new()
                {
                    ID = reader.GetInt32(0),
                    Key = reader.GetString(1),
                    Value = reader.GetString(2)
                };
                configs.Add(config);
            }

            return [.. configs];
        }

        public async Task<int> SaveItemAsync(Config item)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (item.ID == 0)
            {
                saveCmd.CommandText = @"
                INSERT INTO Config (Key, Value)
                VALUES (@Key, @Value);
                SELECT last_insert_rowid();";
            }
            else
            {
                saveCmd.CommandText = @"
                UPDATE Config SET Key = @Key, Value = @Value
                WHERE ID = @ID";
                saveCmd.Parameters.AddWithValue("@ID", item.ID);
            }

            saveCmd.Parameters.AddWithValue("@Key", item.Key);
            saveCmd.Parameters.AddWithValue("@Value", item.Value);

            var result = await saveCmd.ExecuteScalarAsync();
            if (item.ID == 0)
            {
                item.ID = Convert.ToInt32(result);
            }

            return item.ID;
        }
    }
}
