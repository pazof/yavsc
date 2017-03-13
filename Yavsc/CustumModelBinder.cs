using System;
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
            ModelStateEntry modelState = new ModelStateEntry();
            try {
                actualValue = Decimal.Parse(valueResult.FirstValue,  System.Globalization.NumberStyles.AllowDecimalPoint);

                return await ModelBindingResult.SuccessAsync(bindingContext.ModelName,actualValue);
            }
            catch (Exception ) {
            }
            return await ModelBindingResult.FailedAsync(bindingContext.ModelName);
        }
    }
}