using Core.Pool;
using Microsoft.Data.Sqlite;
using System.Text;
namespace Database
{
    public static class TableManagementHelper
    {
        public static bool ColumnExists(LocalSQLite localSqlite, string tableName, string columnName) {
            return localSqlite.UsingConnection((connection) => { 
                return ColumnExists(connection, tableName, columnName);
            });
        }
        public static bool ColumnExists(SqliteConnection connection, string tableName, string columnName) {
            string commandString = $"select count(*) > 0 from pragma_table_info(@tableName) where name=@columnName";
            using (SqliteCommand command = new SqliteCommand(
                commandString,
                connection))
            {
                command.Parameters.Add(new SqliteParameter("@tableName", tableName));
                command.Parameters.Add(new SqliteParameter("@columnName", columnName));
                long result = (long)command.ExecuteScalar()!;
                return result > 0;
            }

        }
    }
}
