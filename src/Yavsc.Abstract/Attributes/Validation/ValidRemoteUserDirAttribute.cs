
using System;
using System.ComponentModel.DataAnnotations;
using Yavsc.Helpers;

namespace Yavsc.Attributes.Validation
{ 
    /// <summary>
    /// Valid Remote User Dir Attribute
    /// </summary>
    public class ValidRemoteUserFilePathAttribute : ValidationAttribute
    {
        public ValidRemoteUserFilePathAttribute()
        {
            UseDefaultErrorMessage();
        }
        void UseDefaultErrorMessage()
        {
            if (ErrorMessageResourceType==null)  {
                ErrorMessageResourceType = typeof(Yavsc.Attributes.Validation.Resources);
                ErrorMessageResourceName = "InvalidPath"; 
            }
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;
            var str = (string) value;
            return str.IsValidYavscPath();
        }
    }
}
