using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBI_eDahab.Web.ViewModels;

namespace DBI_eDahab.Web.Helpers
{
    public class PermissionRequired : ActionFilterAttribute
    {
        Users.Permissions expectedPermissions;

        public PermissionRequired(Users.Permissions permissions = Users.Permissions.None)
        {
            this.expectedPermissions = permissions;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ActionResult unauthorizedActionResult = new ViewResult { ViewName = "Unauthorized" };
            ActionResult logoutActionResult = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { controller = "Authentication", action = "Logout", returnUrl = filterContext.HttpContext.Request.RawUrl }));

            var user = filterContext.HttpContext.Session["User"] as Users;

            if (user == null)
            {
                filterContext.Result = logoutActionResult;
                return;
            }

            //var userCurrentPermissions = user.CurrentPermissions;
            var userCurrentPermissions = (Users.Permissions)HttpContext.Current.Cache[string.Format("{0}'s CurrentPermissions", user.UserName)];

            if (((userCurrentPermissions & expectedPermissions) != expectedPermissions))
                filterContext.Result = unauthorizedActionResult;
        }
    }
}