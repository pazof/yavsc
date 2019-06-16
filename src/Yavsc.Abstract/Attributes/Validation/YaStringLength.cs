using System;

namespace Yavsc.Attributes.Validation
{
    public class YaStringLength: YaValidationAttribute
    {
        public long MinLen { get; set; } = -1;
        private long maxLen;
        public YaStringLength(long maxLen) : base(
            ()=>
                "BadStringLength")
        {
            this.maxLen = maxLen;
        }

        private long excedent=0;
        private long manquant=0;

        public override bool IsValid(object value) {
            
            if (value == null) {
                return false;
            }
            
            string stringValue = value as string;
            if (stringValue==null) return false;
            if (MinLen>=0) 
                {
                    if (stringValue.Length<MinLen)
                    {
                        manquant = MinLen-stringValue.Length;
                    }
                    return false;
                }
            if (maxLen>=0)
                {
                    if (stringValue.Length>maxLen) {
                        excedent = stringValue.Length-maxLen;
                        return false;
                    }
                }
            return true;
        }

    }
}