using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComplianceSheriff.ExportFormatter
{
    public abstract class BaseFileExporter<T>
    {
        public MemoryStream _output { get; set; }

        public string ContentType { get; set; }
        public IEnumerable<T> Data { get; set; }
        public abstract MemoryStream Export(string workSheetName = "");
    }
}
