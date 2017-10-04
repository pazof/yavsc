using System;

namespace Yavsc.Attributes.Validation
{
    public class YaValidationAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        public YaValidationAttribute() : base(()=> Startup.GlobalLocalizer["validationError"])
        {

        }
        
        public YaValidationAttribute(Func<string> acr): base(acr)
        {

        }

        public override string FormatErrorMessage(string name)
        {
            return Startup.GlobalLocalizer[name];
        }
    }
}