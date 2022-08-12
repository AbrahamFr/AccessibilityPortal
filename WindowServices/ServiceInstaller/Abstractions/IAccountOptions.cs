using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceInstaller.Abstractions
{
    public interface IAccountOptions
    {
        string ServiceAccountName { get; }
        string ServiceAccountPassword { get; }
    }
}
