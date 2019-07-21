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

        /// <summary>
        /// Get given string from resources
        /// specified by ErrorMessageResourceType
        /// </summary>
        /// <param name="stringName"></param>
        /// <returns></returns>
        public virtual string GetResourceString(string stringName)
        {
            var prop = this.ErrorMessageResourceType.GetProperty(stringName);
            if (prop==null)
            {
                return " !e! noprop "+stringName+" in "+ErrorMessageResourceType.Name;
            }
            else {
                return (string) prop.GetValue(null, null);
            }
        }

        public override string FormatErrorMessage(string name)
        {
            if (ErrorMessageResourceType == null) // failed :/
            {
                return base.FormatErrorMessage(name);
            }
            if (ErrorMessageResourceName == null) // re failed :/
                return base.FormatErrorMessage(name);

            return GetResourceString(ErrorMessageResourceName);
        }
    }
}