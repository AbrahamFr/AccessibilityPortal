using ComplianceSheriff.IssueTrackerReport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ComplianceSheriff.AdoNet.Helpers
{
    public static class Helper
    {
        public static string BuildImpactRange(List<ImpactRange> impactList)
        {
            var stringBuilder = new StringBuilder();
            var impactListCounter = impactList.Count();

            stringBuilder.Append("AND (");

            foreach (var range in impactList)
            {
                var minValue = Convert.ToInt32(range.MinImpact);
                var maxValue = Convert.ToInt32(range.MaxImpact);

                stringBuilder.Append($" Impact BETWEEN {minValue} AND {maxValue} ");

                impactListCounter -= 1;

                if (impactListCounter > 0)
                {
                    stringBuilder.Append(" OR ");
                }
            }

            stringBuilder.Append(")");

            string result = stringBuilder.ToString();

            return result;
        }

        public static string ReadXmlConfigForDBName(string sharedDir = "", string clusterName = "", string fileName = "default.xml")
        {
            string serverName = "";

            if (String.IsNullOrWhiteSpace(sharedDir))
            {
                throw new ApplicationException("No Server Name supplied in application settings");
            }

            if (String.IsNullOrWhiteSpace(clusterName))
            {
                clusterName = "ComplianceSheriff";
            }

            string path = System.IO.Path.Combine("\\\\", sharedDir, "Cryptzone", clusterName, "config", "sqlserver", fileName);

            using (XmlTextReader reader = new XmlTextReader(path))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == "Host")
                        {
                            serverName = reader.ReadString();
                        }
                    }
                }
            }

            return serverName;
        }

        public static string ConvertObjectToXmlString(object classObject)
        {
            string xmlString = null;
            XmlSerializer xmlSerializer = new XmlSerializer(classObject.GetType());
            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memoryStream, classObject);
                memoryStream.Position = 0;
                xmlString = new StreamReader(memoryStream).ReadToEnd();
            }
            return xmlString;
        }

        public static T ConvertXmlStringtoObject<T>(string xmlString)
        {
            T classObject;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StringReader stringReader = new StringReader(xmlString))
            {
                classObject = (T)xmlSerializer.Deserialize(stringReader);
            }
            return classObject;
        }
    }
}
