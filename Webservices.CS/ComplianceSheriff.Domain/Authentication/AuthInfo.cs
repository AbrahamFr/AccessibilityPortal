using ComplianceSheriff.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Authentication
{
    public class AuthInfo
    {
        public bool IsAuthenticated { get; set; }
        public User User { get; set; }
    }
}
