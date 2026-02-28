
using System;
using System.Reflection;

namespace Yavsc.Attributes.Validation
{
    public class YaRegularExpression : System.ComponentModel.DataAnnotations.RegularExpressionAttribute { 
        public YaRegularExpression(string pattern): base (pattern)
        {
            this.ErrorMessage = "RegularExpression: "+ pattern;
            
        }

        public override string FormatErrorMessage(string name)
        {
            if (ErrorMessageResourceType==null || string.IsNullOrEmpty(ErrorMessageResourceName))
             return ErrorMessage;
            var prop = this.ErrorMessageResourceType.GetProperty(ErrorMessageResourceName);
            return (string) prop.GetValue(null, BindingFlags.Static, null, null, System.Globalization.CultureInfo.CurrentUICulture);

        }
    }
}