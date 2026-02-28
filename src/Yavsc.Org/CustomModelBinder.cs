using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Yavsc
{
   public class MyDecimalModelBinder : IModelBinder
    {

        async Task IModelBinder.BindModelAsync(ModelBindingContext bindingContext)
        {
              ValueProviderResult valueResult = bindingContext.ValueProvider
            .GetValue(bindingContext.ModelName);
            decimal actualValue ;

            try {
                bindingContext.Result = ModelBindingResult.Success(
                    Decimal.Parse(valueResult.FirstValue,  System.Globalization.NumberStyles.AllowDecimalPoint));
            }
            catch (Exception ) {
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }

    public class MyDateTimeModelBinder : IModelBinder
    {
        async Task IModelBinder.BindModelAsync(ModelBindingContext bindingContext)
        {
           ValueProviderResult valueResult = bindingContext.ValueProvider
            .GetValue(bindingContext.ModelName);
            DateTime actualValue ;
            // DateTime are sent in the french format
            if (DateTime.TryParse(valueResult.FirstValue,new CultureInfo("fr-FR"), DateTimeStyles.AllowInnerWhite, out actualValue))
            {
                bindingContext.Result = ModelBindingResult.Success(actualValue);
            }
            else bindingContext.Result = ModelBindingResult.Failed();
            
        }
    }
}
