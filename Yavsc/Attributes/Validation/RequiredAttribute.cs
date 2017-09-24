using System;

namespace Yavsc.Attributes.Validation
{
    public class YaValidationAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        public YaValidationAttribute() : base(()=> Startup.GlobalLocalizer["validationError"])
        {

        }
        public override string FormatErrorMessage(string name)
        {
            return Startup.GlobalLocalizer[name];
        }
    }
    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
       public class YaRequiredAttribute : YaValidationAttribute
    {

        /// <summary>
        /// Gets or sets a flag indicating whether the attribute should allow empty strings.
        /// </summary>
        public bool AllowEmptyStrings { get; set; }
         public YaRequiredAttribute (string msg) : base()
         {
             ErrorMessage = msg;
         } 
        public YaRequiredAttribute () 
        {
            this.ErrorMessage = Startup.GlobalLocalizer["RequiredField"];
            
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
    public class YaRegularExpression : System.ComponentModel.DataAnnotations.RegularExpressionAttribute { 
        public YaRegularExpression(string pattern): base (pattern)
        {
            this.ErrorMessage = pattern;
        }
        public override string FormatErrorMessage(string name)
        {
            return Startup.GlobalLocalizer[name];
        }
    }
}