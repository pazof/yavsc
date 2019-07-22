using System;

namespace Yavsc.Attributes.Validation
{
    public partial class YaStringLength: YaValidationAttribute
    {
        public long MinimumLength { get; set; } = 0;
        private long maxLen;
        public YaStringLength(long maxLen) : base( ()=> "BadStringLength")
        {
            this.maxLen = maxLen;
            UseDefaultErrorMessage();
        }
        public YaStringLength(long minLen, long maxLen) : base( ()=> "BadStringLength")
        {
            this.maxLen = maxLen;
            this.MinimumLength=minLen;
            UseDefaultErrorMessage();
        }
        void UseDefaultErrorMessage()
        {
            if (ErrorMessageResourceType==null)  {
                ErrorMessageResourceType = typeof(YaStringLength);
                ErrorMessageResourceName = "InvalidStringLength"; 
            }
        }

        public override bool IsValid(object value) {
            
            if (value == null) {
                return false;
            }
            
            string stringValue = value as string;
            if (stringValue==null) return false;
            if (MinimumLength>=0) 
                {
                    if (stringValue.Length< MinimumLength) {
                        return false;

                    }
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