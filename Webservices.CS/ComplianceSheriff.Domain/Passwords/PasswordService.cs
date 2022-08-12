using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ComplianceSheriff.Passwords
{
    public class PasswordService : IPasswordService
    {
        public string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 1,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public HashResult GenerateHash(string valueToHash)
        {
            var rngCsp = new RNGCryptoServiceProvider();

            byte[] salt = new byte[32];
            rngCsp.GetBytes(salt);

            var hash = new Rfc2898DeriveBytes(valueToHash, salt, 10000);

            var hashbytes = new byte[36];
            var saltbytes = new byte[32];

            Array.Copy(salt, 0, saltbytes, 0, 32);
            Array.Copy(hash.GetBytes(36), 0, hashbytes, 0, 36);

            return new HashResult { 
                Hash = Convert.ToBase64String(hashbytes),
                Salt = Convert.ToBase64String(saltbytes)
            };
        }

        public string EncryptPassword(string password)
        {
            using (var sha1 = SHA1.Create())
            {
                return String.Join("", sha1.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("x2").ToUpperInvariant()).ToArray());
            }
        }

        //private byte[] GenerateRandomCryptographicKey(int keyLength)
        //{
        //    var rngCsp = new RNGCryptoServiceProvider();

        //    byte[] randomBytes = new byte[keyLength];
        //    rngCsp.GetBytes(randomBytes);

        //    return randomBytes;
        //}
    }
}
