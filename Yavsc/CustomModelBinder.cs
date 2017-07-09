using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Yavsc
{
   public class MyDecimalModelBinder : IModelBinder
    {

        public async Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider
            .GetValue(bindingContext.ModelName);
            decimal actualValue ;

            try {
                actualValue = Decimal.Parse(valueResult.FirstValue,  System.Globalization.NumberStyles.AllowDecimalPoint);
                return await ModelBindingResult.SuccessAsync(bindingContext.ModelName,actualValue);
            }
            catch (Exception ) {
            }
            return await ModelBindingResult.FailedAsync(bindingContext.ModelName);
        }
    }

    public class MyDateTimeModelBinder : IModelBinder
    {
        public async Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
        {
           ValueProviderResult valueResult = bindingContext.ValueProvider
            .GetValue(bindingContext.ModelName);
            DateTime actualValue ;
            ModelStateEntry modelState = new ModelStateEntry();
                // DateTime are sent in the french format
                if (DateTime.TryParse(valueResult.FirstValue,new CultureInfo("fr-FR"), DateTimeStyles.AllowInnerWhite, out actualValue))
                {
                    return await ModelBindingResult.SuccessAsync(bindingContext.ModelName,actualValue);
                }
           
            return await ModelBindingResult.FailedAsync(bindingContext.ModelName);
        }
    }
}
