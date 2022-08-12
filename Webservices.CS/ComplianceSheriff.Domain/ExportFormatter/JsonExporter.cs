using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComplianceSheriff.ExportFormatter
{
    public class JsonExporter<T> : BaseFileExporter<T>, IFileExporter<T>
    {
        public JsonExporter(IEnumerable<T> data, MemoryStream memoryStream)
        {
            base._output = memoryStream;
            base.Data = data;
        }

        public override MemoryStream Export(string workSheetName = "")
        {
            if (this.Data != null)
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this.Data));
                _output.Write(byteArray, 0, byteArray.Length);
                _output.Position = 0;
            }

            return _output;
        }
    }
}
