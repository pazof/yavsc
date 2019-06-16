using System;
using System.Reflection;

namespace Yavsc.Attributes.Validation
{
    public class YaValidationAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        public YaValidationAttribute(string msg) : base(msg)
        {

        }
        
        public YaValidationAttribute(Func<string> acr): base(acr)
        {

        }

        public override string FormatErrorMessage(string name)
        {
            if (ErrorMessageResourceType == null) // failed :/
                return name;
            if (ErrorMessageResourceName == null) // re failed :/
                return name;

            var prop = this.ErrorMessageResourceType.GetProperty(ErrorMessageResourceName);
            if (prop==null) // re re failed :/
                return "noprop "+ErrorMessageResourceName+" in "+ErrorMessageResourceType.Name;
            return (string) prop.GetValue(null, null);
            
        }
    }
}