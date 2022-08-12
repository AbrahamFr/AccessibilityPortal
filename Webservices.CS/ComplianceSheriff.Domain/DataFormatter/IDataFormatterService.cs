using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.DataFormatter
{
    public interface IDataFormatterService
    {
        string BuildUrl(string baseUrl, string path);
    }
}
