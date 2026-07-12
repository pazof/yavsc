using Microsoft.AspNetCore.Mvc;
using Yavsc.Models;

public static class ErrorViewHelpers
{
    public static IActionResult ErrorView<T>(this Controller controller, string message)
    {
        var logger = controller.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger<T>();
        
        logger.LogError(message);
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        
        if  (!controller.ModelState.IsValid)
        {
            foreach (var modelState in controller.ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    logger.LogError("ModelState error: {0}", error.ErrorMessage);
                    foreach (var key in controller.ModelState.Keys)
                    {
                        logger.LogError("ModelState key: {0}", key);
                        dictionary.Add(key, 
                        string.Join("\n", 
                        controller.ModelState[key].Errors.Select( e => e.ErrorMessage).ToArray()));
                    }
                }
            }
        }

        if (controller.HttpContext.Request.Headers.ContainsKey("Accept")
         && controller.HttpContext.Request.Headers["Accept"].ToString().Contains("application/json"))
        {
            return controller.Json(new
            {
                RequestId = controller.HttpContext.TraceIdentifier,
                Description = message,
                ModelErrors = dictionary
            });
        }

        return controller.View("Error",
            new ErrorViewModel
            {
                RequestId = controller.HttpContext.TraceIdentifier,
                Description = message,
                ModelErrors = dictionary
            }
        );
    }
}