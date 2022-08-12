using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComplianceSheriff.ExportFormatter
{
    public class CSVExporter<T> : BaseFileExporter<T>, IFileExporter<T>
    {
        public CSVExporter(IEnumerable<T> data, MemoryStream memoryStream)
        {
            base._output = memoryStream;
            base.Data = data;
        }

        public override MemoryStream Export(string workSheetName = "")
        {
            var textWriter = new StreamWriter(_output);

            var csv = new CsvWriter(textWriter, System.Globalization.CultureInfo.InvariantCulture);
            csv.WriteRecords(this.Data);
            csv.Flush();
            textWriter.Flush();
            _output.Position = 0;

            return _output;
        }
    }
}
