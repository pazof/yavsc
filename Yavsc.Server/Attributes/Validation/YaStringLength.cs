namespace Yavsc.Attributes.Validation
{
    public class YaStringLength: YaValidationAttribute
    {
        public long MinLen { get; set; } = -1;
        private long maxLen;
        public YaStringLength(long maxLen) : base(
            ()=>string.Format(
                ResourcesHelpers.DefaultResourceManager.GetString("BadStringLength"),
                maxLen))
        {
            this.maxLen = maxLen;
        }

        private long excedent;
        private long manquant;

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
        public override string FormatErrorMessage(string name)
        {
            if (MinLen<0) {
                // DetailledMaxStringLength
                return string.Format(
                ResourcesHelpers.DefaultResourceManager.GetString("DetailledMaxStringLength"),
                maxLen,
                excedent);
            } else 
                return string.Format(
                ResourcesHelpers.DefaultResourceManager.GetString("DetailledMinMaxStringLength"),
                MinLen,
                maxLen,
                manquant,
                excedent);
        }
    }
}