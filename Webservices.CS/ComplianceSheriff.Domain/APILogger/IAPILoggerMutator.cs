using ComplianceSheriff.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.APILogger
{
    public interface IAPILoggerMutator
    {
       void AddAPILogger(APILoggerRequest requestObject);
    }
}
