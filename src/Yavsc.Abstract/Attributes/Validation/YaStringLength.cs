using System;

namespace Yavsc.Attributes.Validation
{
    public partial class YaStringLength: YaValidationAttribute
    {
        public long MinimumLength { get; set; } = 0;
        private readonly long maxLen;
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
                ErrorMessageResourceType = typeof(Yavsc.Attributes.Validation.Resources);
                ErrorMessageResourceName = "InvalidStringLength"; 
            }
        }

        public override bool IsValid(object value) {
            
            string stringValue = value as string;
            if (stringValue==null) return MinimumLength <= 0;
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
