using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComplianceSheriff.ExportFormatter
{
    public interface IFileExporter<T>
    {
        MemoryStream Export(string workSheetName = "");
    }
}
