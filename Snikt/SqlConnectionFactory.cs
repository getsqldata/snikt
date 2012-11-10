﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Snikt
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        public Dictionary<string, SqlConnection> Pool { get; private set; }

        protected SqlConnectionFactory()
        {
            Pool = new Dictionary<string, SqlConnection>();
        }

        public static ISqlConnectionFactory Get()
        {
            return new SqlConnectionFactory();
        }

        public SqlConnection CreateIfNotExists(string nameOrConnectionString)
        {
            Assert.ThrowIfNull(nameOrConnectionString, "string nameOrConnectionString", Messages.NameOrConnectionStringNullOrEmpty);

            string connectionString = ParseToConnectionString(nameOrConnectionString);
            SqlConnection connection = GetConnection(connectionString);
            return connection;
        }

        private string ParseToConnectionString(string nameOrConnectionString)
        {
            string connectionString = null;
            if (NeedConfiguration(nameOrConnectionString))
            {
                string connectionName = GetConnectionName(nameOrConnectionString);
                connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            }

            return connectionString ?? nameOrConnectionString;
        }

        private bool NeedConfiguration(string name)
        {
            return name.StartsWith("name=");
        }

        private string GetConnectionName(string stringName)
        {
            return stringName.Replace("name=", string.Empty);
        }

        private SqlConnection GetConnection(string connectionString)
        {
            if (NotPooled(connectionString))
            {
                CreatePooledConnection(connectionString);
            }
            return GetPooledConnection(connectionString);
        }

        private bool NotPooled(string connectionString)
        {
            return !Pooled(connectionString);
        }

        private bool Pooled(string connectionString)
        {
            return Pool.ContainsKey(connectionString);
        }

        private SqlConnection CreatePooledConnection(string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            PoolConnection(connection);
            return connection;
        }

        private void PoolConnection(SqlConnection connection)
        {
            Pool.Add(connection.ConnectionString, connection);
        }

        private SqlConnection GetPooledConnection(string connectionString)
        {
            SqlConnection connection = null;
            if (Pool.ContainsKey(connectionString))
            {
                connection = Pool[connectionString];
            }
            return connection;
        }
    }
}
