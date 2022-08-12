using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComplianceSheriff.ExportFormatter
{
    public static class ExportFormatFactory<T>
    {
        public static IFileExporter<T> CreateFormatter(string formatType, IEnumerable<T> data)
        {
            IFileExporter<T> fileExporter = null;
            var memStream = new MemoryStream();

            switch(formatType.ToLower())
            {
                case "csv":
                    fileExporter = new CSVExporter<T>(data, memStream);
                    break;

                case "xlsx":
                    fileExporter = new ExcelExporter<T>(data, memStream);
                    break;

                case "json":
                    fileExporter = new JsonExporter<T>(data, memStream);
                    break;

                case "xml":
                    fileExporter = new XmlExporter<T>(data, memStream);
                    break;
            }

            return fileExporter;
        }
    }
}
