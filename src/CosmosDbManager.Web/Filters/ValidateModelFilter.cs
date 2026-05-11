using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CosmosDbManager.Web.Filters;

public sealed class ValidateModelFilter : IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ModelState.IsValid)
        {
            return next();
        }

        if (context.Controller is Controller controller)
        {
            var model = context.ActionArguments.Values.FirstOrDefault(argument => argument is not null);
            context.Result = controller.View(model);
            return Task.CompletedTask;
        }

        context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
        return Task.CompletedTask;
    }
}
