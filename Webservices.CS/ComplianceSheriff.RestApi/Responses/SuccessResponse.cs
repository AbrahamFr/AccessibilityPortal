using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Responses
{
    public class SuccessResponse
    {
        public SuccessResponse(string successValue)
        {
            Success = successValue;
        }
        public string Success { get; set; }
    }
}
