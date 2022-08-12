using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ComplianceSheriff.ExportFormatter
{
    public class XmlExporter<T> : BaseFileExporter<T>, IFileExporter<T>
    {
        public XmlExporter(IEnumerable<T> data, MemoryStream memoryStream)
        {
            base._output = memoryStream;
            base.Data = data;
        }

        public override MemoryStream Export(string workSheetName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            serializer.Serialize(_output, this.Data.ToList());
            _output.Position = 0;

            return _output;

        }
    }
}
