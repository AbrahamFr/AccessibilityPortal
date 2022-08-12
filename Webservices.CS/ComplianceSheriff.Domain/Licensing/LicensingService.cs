using ComplianceSheriff.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security.Cryptography;
using System.Xml.Serialization;
using ComplianceSheriff.Exceptions;
using Microsoft.Extensions.Logging;

namespace ComplianceSheriff.Licensing
{
    public class LicensingService : ILicensingService
    {
        private readonly byte[] _init;
        private readonly ILogger<LicensingService> _logger;

        public LicensingService(ILogger<LicensingService> logger)
        {
            _init = Initialize();
            _logger = logger;
        }

        private byte[] Initialize()
        {
            // generate the salt for GetCryptoStream, viz. HISCHISCHISCHISC
            //object type = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            object type = "HISC.Utility";
            byte[] buffer = new byte[16];
            string value = new string('\0', 0) + type;
            for (int i = 0; i < 80; i += 5)
            {
                buffer[i & 0xf] = (byte)value[i & 0x3];
            }
            return buffer;
        }

        private string HashKey(string keyToHash)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            var hashedBytes = encoding.GetBytes(keyToHash);

            var hashedKeyBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(hashedBytes);

            var key = new StringBuilder();

            for (int i = 0; i < hashedKeyBytes.Length; i++)
            {
                key.Append(hashedKeyBytes[i].ToString("x2").ToUpperInvariant());
            }

            return key.ToString();
        }

        private SymmetricAlgorithm CreateSymmetricAlgorithm()
        {
            bool fips = false;
            using (
                var rkey =
                    Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                        @"System\CurrentControlSet\Control\Lsa\FIPSAlgorithmPolicy"))
            {
                fips = rkey != null && rkey.GetValue("Enabled") as int? == 1;
            }
            System.Security.Cryptography.SymmetricAlgorithm algorithm =
                System.Security.Cryptography.SymmetricAlgorithm.Create(fips ? "Aes" : "Rijndael");
            byte[] iv = _init;
            System.Array.Resize(ref iv, algorithm.IV.Length);
            algorithm.IV = iv;
            return algorithm;
        }

        private CryptoStream GetCryptoStream(string path, string key, bool forReading)
        {
            System.IO.Stream fileStream = forReading
                ? File.OpenRead(path)
                : new System.IO.FileStream(path, System.IO.FileMode.Create);

            System.Security.Cryptography.SymmetricAlgorithm algorithm = CreateSymmetricAlgorithm();
            key = key.PadRight(algorithm.Key.Length, ' ').Substring(0, algorithm.Key.Length);
            algorithm.Key = new System.Text.ASCIIEncoding().GetBytes(key);
            System.Security.Cryptography.ICryptoTransform transform = forReading
                ? algorithm.CreateDecryptor()
                : algorithm.CreateEncryptor();
            return new System.Security.Cryptography.CryptoStream(fileStream, transform,
                forReading
                    ? System.Security.Cryptography.CryptoStreamMode.Read
                    : System.Security.Cryptography.CryptoStreamMode.Write);
        }

        public Allocator GetLicenseInfo(string organizationId, ConfigurationOptions configOptions)
        {
            Allocator allocator = null;

            var directory = $@"\\{configOptions.SharedDir}\Cryptzone\{configOptions.ClusterName}\customers\{organizationId}\data";

            string path = Path.Combine(directory, "license.key");

            var keyToHash = configOptions.ClusterName + "." + organizationId;

            var key = HashKey(keyToHash);

            try
            {
                using (Stream stream = GetCryptoStream(path, key.ToString(), true))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var text = ((TextReader)reader).ReadToEnd();
                        var xmlReader = new XmlTextReader(new StringReader(text));
                        xmlReader.MoveToContent();

                        xmlReader = new XmlTextReader(new StringReader(text));
                        XmlSerializer xs = new XmlSerializer(typeof(Allocator));
                        allocator = (Allocator)xs.Deserialize(xmlReader);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new NoLicenseKeyException("noLicenseKeyFound");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return allocator;
        }

        public string GetLicensedModuleString(string organizationId, ConfigurationOptions configOptions)
        {
            var licenseInfo = GetLicenseInfo(organizationId, configOptions);
            return string.Join(",", licenseInfo.Modules.Cast<string>().ToArray());
        }

    }
}

