using ComplianceSheriff.AdoNet;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeKreyConsulting.AdoTestability
{
    static class CommandBuilderExtensions
    {
        public static async Task<DbCommand> BuildFrom(this CommandBuilder builder, IConnectionManager connection, Dictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            return builder.BuildFrom(await connection.GetOpenDbConnection(cancellationToken), parameters, connection.Transaction);
        }

        public static async Task<DbCommand> BuildFrom(this CommandBuilder builder, IConnectionManager connection, CancellationToken cancellationToken)
        {
            return builder.BuildFrom(await connection.GetOpenDbConnection(cancellationToken), transaction: connection.Transaction);
        }
    }
}
