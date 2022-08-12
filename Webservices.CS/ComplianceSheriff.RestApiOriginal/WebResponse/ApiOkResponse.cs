using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.RestApi.WebResponse
{
    public class ApiOkResponse : ApiResponse
    {
        public object Result { get; }

        public ApiOkResponse(object result)
            : base(200)
        {
            Result = result;
        }

    }
}
