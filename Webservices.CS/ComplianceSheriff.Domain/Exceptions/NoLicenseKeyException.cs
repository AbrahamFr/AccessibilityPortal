using System;

namespace ComplianceSheriff.Exceptions
{
    public class NoLicenseKeyException : Exception
    {
        public NoLicenseKeyException(string message) : base(message){}
    }
}
