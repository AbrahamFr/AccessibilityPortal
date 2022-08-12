using ComplianceSheriff.RestApi.WebResponse;

namespace ComplianceSheriff.WebResponse
{
    public class GenericApiResponse<T> : ApiResponse
    {
        public new T Data;
    }
}
