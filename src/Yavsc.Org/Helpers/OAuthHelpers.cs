using System;
using System.Security.Cryptography;

namespace Yavsc.Helpers {
public class Helper
    {
        public static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm =  SHA256CryptoServiceProvider.Create();
       
            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }
    }
}
