using System.Resources;
using Yavsc.Resources;

namespace Yavsc.Attributes.Validation
{
    public class YaRegularExpression : System.ComponentModel.DataAnnotations.RegularExpressionAttribute { 
        public YaRegularExpression(string pattern): base (pattern)
        {
            this.ErrorMessage = pattern;
        }

        public override string FormatErrorMessage(string name)
        {
            return ResourcesHelpers.DefaultResourceManager.GetString(this.ErrorMessageResourceName);
        }
    }
}