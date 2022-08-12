using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceInstaller.Abstractions
{
    public interface IFolderOptions
    {
        string ServiceType { get; }
        string DestinationDirectory { get; }
        string SourceDirectory { get; }
    }
}
