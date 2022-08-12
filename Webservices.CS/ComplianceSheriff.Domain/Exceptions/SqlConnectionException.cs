using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Exceptions
{
    public class SqlConnectionException : Exception
    {
        public SqlConnectionException(string message) : base(message) { }
    }
}
