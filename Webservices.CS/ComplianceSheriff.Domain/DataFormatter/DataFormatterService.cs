using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ComplianceSheriff.DataFormatter
{
    public class DataFormatterService : IDataFormatterService
    {
        public string BuildUrl(string baseUrl, string path)
        {
            var uriBuilder = new Uri(new Uri(baseUrl), path);
            return uriBuilder.ToString();
        }
    }
}
