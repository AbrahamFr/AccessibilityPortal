using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ComplianceSheriff.AdoNet
{
    class ConnectionManager : IDisposable, IConnectionManager
    {
        private readonly DbConnection dbConnection;
        private DbTransaction transaction;
        

        public DbTransaction Transaction => transaction;

        public ConnectionManager(DbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        public async Task<DbConnection> GetOpenDbConnection(CancellationToken cancellationToken)
        {
            try
            {
                if (dbConnection.State != System.Data.ConnectionState.Open)
                {
                    await dbConnection.OpenAsync(cancellationToken);
                }
                return dbConnection;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<DbTransaction> BeginTransaction(CancellationToken cancellationToken)
        {
            var dbConnection = await GetOpenDbConnection(cancellationToken);
            this.transaction = dbConnection.BeginTransaction();
            return this.transaction;
        }
        
        public void Dispose()
        {
            transaction?.Dispose();
            dbConnection.Dispose();
        }
    }
}
