using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceInstaller.Abstractions
{
    public interface IUrnOptions
    {
        string LocationZone { get; }
        string ClusterName { get; }
        string ControllerMachineName { get; }
        string LocationPath { get; }
    }
}
