using ComplianceSheriff.Authentication;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace ComplianceSheriff.AdoNet
{
    public interface IConnectionFactory
    {
        SqlConnection GetContextDBConnection();
    }
}
