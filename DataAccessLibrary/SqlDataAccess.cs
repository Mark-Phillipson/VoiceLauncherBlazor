using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
// using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace DataAccessLibrary
{
	public class SqlDataAccess : ISqlDataAccess
	{

		public required IConfiguration _config;
		public string ConnectionStringName { get; set; } = "VoiceLauncher";

		public SqlDataAccess(IConfiguration config)
		{
			_config = config;
		}

		private bool IsSqlServerConnection(string? connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString)) return false;
			// Heuristic: SQL Server connection strings usually contain 'Initial Catalog' or 'Server=' or 'Data Source=Localhost;Initial Catalog'
			return connectionString.IndexOf("Initial Catalog", StringComparison.OrdinalIgnoreCase) >= 0 ||
			       connectionString.IndexOf("Server=", StringComparison.OrdinalIgnoreCase) >= 0 ||
			       connectionString.IndexOf("Data Source=localhost", StringComparison.OrdinalIgnoreCase) >= 0 ||
			       connectionString.IndexOf("Data Source=Localhost", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public async Task<List<T>> LoadData<T, U>(string sql, U parameters)
		{
			string? connectionString = _config.GetConnectionString(ConnectionStringName);
			if (IsSqlServerConnection(connectionString))
			{
				using (IDbConnection connection = new SqlConnection(connectionString))
				{
					var data = await connection.QueryAsync<T>(sql, parameters);
					return data.ToList();
				}
			}
			else
			{
				using (IDbConnection connection = new SqliteConnection(connectionString))
				{
					var data = await connection.QueryAsync<T>(sql, parameters);
					return data.ToList();
				}
			}
		}
		public async Task<T> LoadSingleData<T, U>(string sql, U parameters)
		{
			if (ConnectionStringName == null || ConnectionStringName.Length == 0 || _config == null)
			{
				throw new ArgumentNullException("ConnectionStringName", "ConnectionStringName is null or empty");
			}
			string? connectionString = _config.GetConnectionString(ConnectionStringName);
			if (IsSqlServerConnection(connectionString))
			{
				using (IDbConnection connection = new SqlConnection(connectionString))
				{
					var data = await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
					if (data == null)
					{
						throw new ArgumentNullException("data", "data is null");
					}
					return data;
				}
			}
			else
			{
				using (IDbConnection connection = new SqliteConnection(connectionString))
				{
					var data = await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
					if (data == null)
					{
						throw new ArgumentNullException("data", "data is null");
					}
					return data;
				}
			}
		}
		public async Task SaveData<T>(string sql, T parameters)
		{
			string? connectionString = _config.GetConnectionString(ConnectionStringName);
			if (IsSqlServerConnection(connectionString))
			{
				using (IDbConnection connection = new SqlConnection(connectionString))
				{
					await connection.ExecuteAsync(sql, parameters);
				}
			}
			else
			{
				using (IDbConnection connection = new SqliteConnection(connectionString))
				{
					await connection.ExecuteAsync(sql, parameters);
				}
			}

		}
	}
}
