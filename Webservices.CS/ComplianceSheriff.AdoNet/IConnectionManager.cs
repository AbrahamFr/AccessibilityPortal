using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet
{
    public interface IConnectionManager
    {
        DbTransaction Transaction { get; }

        Task<DbConnection> GetOpenDbConnection(CancellationToken cancellationToken);
        Task<DbTransaction> BeginTransaction(CancellationToken cancellationToken);
    }
}