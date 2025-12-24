namespace MARS_Project.CreateFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class SessionAuthorize : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var email = context.HttpContext.Session.GetString("EmailID");

        if (string.IsNullOrEmpty(email))
        {
            context.Result = new RedirectToActionResult("Login", "User", null);
        }
    }
}

