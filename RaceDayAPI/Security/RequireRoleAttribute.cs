using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayAPI.Security
{
    /*
     * This attribute is used to restrict access to certain controller actions based on the user's role.
     * It checks the user's role stored in the session and compares it against the allowed roles specified in the attribute.
     * If the user is not logged in or does not have the required role, it returns an appropriate HTTP status code (401 or 403).
     */
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private string[] allowedRoles;

        public RequireRoleAttribute(params string[] roles)
        {
            allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            int? userId = context.HttpContext.Session.GetInt32("UserId");
            string role = context.HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
            {
                context.Result = new UnauthorizedObjectResult(
                    new { error = "You must log in first." });
                return;
            }

            bool allowed = false;
            if (allowedRoles.Length == 0)
            {
                allowed = true;
            }

            for (int i = 0; i < allowedRoles.Length; i++)
            {
                if (allowedRoles[i] == role)
                {
                    allowed = true;
                }
            }

            if (allowed == false)
            {
                context.Result = new ObjectResult(
                    new { error = "Your role is not allowed to do this." })
                {
                    StatusCode = 403
                };
            }
        }
    }
}