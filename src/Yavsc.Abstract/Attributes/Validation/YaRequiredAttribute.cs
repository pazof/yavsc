using System;

namespace Yavsc.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class YaRequiredAttribute : YaValidationAttribute
    {

        /// <summary>
        /// Gets or sets a flag indicating whether the attribute should allow empty strings.
        /// </summary>
        public bool AllowEmptyStrings { get; set; }
         public YaRequiredAttribute (string msg) : base(msg)
         {
             ErrorMessage = msg;
         } 
        public YaRequiredAttribute () : base("Required Field")
        {
            ErrorMessageResourceType = typeof(Yavsc.Attributes.Validation.Resources);
            ErrorMessageResourceName = "FieldRequired";
        }
        
        public override bool IsValid(object value) {
            if (value == null) {
                return false;
            }

            // only check string length if empty strings are not allowed
            var stringValue = value as string;
            if (stringValue != null && !AllowEmptyStrings) {
                return stringValue.Trim().Length != 0;
            }

            return true;
        }
    }
    
}