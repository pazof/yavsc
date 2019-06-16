using System;

namespace Yavsc.Attributes.Validation
{
    public class YaStringLength: YaValidationAttribute
    {
        public long MinimumLength { get; set; } = -1;
        private long maxLen;
        public YaStringLength(long maxLen) : base( ()=> "BadStringLength")
        {
            this.maxLen = maxLen;
        }

        // hugly ... 
        static long excedent=0;
        static long manquant=0;

        public override bool IsValid(object value) {
            
            if (value == null) {
                return false;
            }
            
            string stringValue = value as string;
            if (stringValue==null) return false;
            if (MinimumLength>=0) 
                {
                    if (stringValue.Length< MinimumLength) 
                    return false;
                }
            if (maxLen>=0)
                {
                    if (stringValue.Length>maxLen) {
                        return false;
                    }
                }
            return true;
        }
        public override string FormatErrorMessage(string name)
        {
            var temp  = base.FormatErrorMessage(name);
            return string.Format(temp, MinimumLength, maxLen);
        }

    }
}