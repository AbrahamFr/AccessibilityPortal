using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using ComplianceSheriff.Extensions;

namespace ComplianceSheriff.ExportFormatter
{
    public class ExcelExporter<T> : BaseFileExporter<T>, IFileExporter<T>
    {
        public ExcelExporter(IEnumerable<T> data, MemoryStream memoryStream)
        {
            base._output = memoryStream;
            base.Data = data;
        }

        public override MemoryStream Export(string workSheetName)
        {
            using (var package = new ExcelPackage(_output))
            {
                var workSheet = package.Workbook.Worksheets.Add(workSheetName);
                workSheet.Cells.LoadFromCollectionFiltered(this.Data);
                package.Save();

                _output.Position = 0;
            }

            return _output;
        }
    }
}
