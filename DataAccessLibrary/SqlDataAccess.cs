﻿using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
// using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

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
		public async Task<List<T>> LoadData<T, U>(string sql, U parameters)
		{
			string? connectionString = _config.GetConnectionString(ConnectionStringName);
			// using (IDbConnection connection = new SqlConnection(connectionString))
			using (Microsoft.Data.SqlClient.SqlConnection connection =  new Microsoft.Data.SqlClient.SqlConnection(connectionString))
			{
				var data = await connection.QueryAsync<T>(sql, parameters);
				return data.ToList();
			}
		}
		public async Task<T> LoadSingleData<T, U>(string sql, U parameters)
		{
			if (ConnectionStringName == null || ConnectionStringName.Length == 0 || _config == null)
			{
				throw new ArgumentNullException("ConnectionStringName", "ConnectionStringName is null or empty");
			}
			string? connectionString = _config.GetConnectionString(ConnectionStringName);
			using (Microsoft.Data.SqlClient.SqlConnection connection =  new Microsoft.Data.SqlClient.SqlConnection(connectionString))
			{
				var data = await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
				if (data == null)
				{
					throw new ArgumentNullException("data", "data is null");
				}
				return data;
			}
		}
		public async Task SaveData<T>(string sql, T parameters)
		{
			string? connectionString = _config.GetConnectionString(ConnectionStringName);
			using (Microsoft.Data.SqlClient.SqlConnection connection =  new Microsoft.Data.SqlClient.SqlConnection(connectionString))
			{
				await connection.ExecuteAsync(sql, parameters);
			}

		}
	}
}
