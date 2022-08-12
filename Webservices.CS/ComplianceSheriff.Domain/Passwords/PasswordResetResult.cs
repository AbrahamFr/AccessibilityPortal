using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Passwords
{
    public class PasswordResetResult
    {
        public string TempPassword { get; set; }
        public HashResult HashResult { get; set; }
    }
}
