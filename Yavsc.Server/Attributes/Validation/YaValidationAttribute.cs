using System;

namespace Yavsc.Attributes.Validation
{
    public class YaValidationAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        public YaValidationAttribute() : base(()=> ResourcesHelpers.DefaultResourceManager.GetString("validationError"))
        {

        }
        
        public YaValidationAttribute(Func<string> acr): base(acr)
        {

        }

        public override string FormatErrorMessage(string name)
        {
            return ResourcesHelpers.DefaultResourceManager.GetString(name);
        }
    }
}