using ComplianceSheriff.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.UserAccounts
{
    public class UserAccountManagerResponse
    {
        public UserAccountCreateStatus Status { get; set; }
        public int ReturnId { get; set; }
    }
}
