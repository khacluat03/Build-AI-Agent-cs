using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace AgentCS.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        public DatabaseService(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }
        public List<Dictionary<string, object>> ExecuteQuery(string sql)
        {
            var results = new List<Dictionary<string, object>>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                results.Add(row);
            }

            return results;
        }
    }
}
