using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace MARS_Project.Filters
{
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(string roles)
        {
            _roles = string.IsNullOrEmpty(roles) ? new string[0] : roles.Split(',');
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var email = context.HttpContext.Session.GetString("EmailID");
            var userRole = context.HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
            {
                context.Result = new RedirectToActionResult("Login", "User", null);
            }
        }
    }
}