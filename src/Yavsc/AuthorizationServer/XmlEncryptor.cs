


using System;
using System.Xml.Linq;
using Microsoft.AspNet.DataProtection.XmlEncryption;

namespace Yavsc.Auth {

    public class MonoXmlEncryptor : IXmlEncryptor
    {
        public MonoXmlEncryptor ()
        {
        }
        public EncryptedXmlInfo Encrypt(XElement plaintextElement)
        {
            var result = new EncryptedXmlInfo(plaintextElement,
            typeof(MonoDataProtector));
            return result;
        }
    }

}
