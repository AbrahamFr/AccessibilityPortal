using ComplianceSheriff.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Responses
{
    public class GetUserByUserNameResponse
    {
        public GetUserByUserNameResponse(UserResponse userResponse)
        {
            this.User = userResponse;
        }
        public UserResponse User { get; set; }
    }
}
