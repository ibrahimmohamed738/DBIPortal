using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBI_eDahab.Web.Helpers;

namespace DBI_eDahab.Web.ViewModels
{ 
    public class UserFormModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = base.BindModel(controllerContext, bindingContext);

            if (!(bindingContext.ModelType == typeof (UpdateUserForm))) return model;

            var userForm = model as UpdateUserForm;

            userForm.CurrentPermissions = ExtractEnum<Users.Permissions>(controllerContext, bindingContext,
                "CurrentPermissions");

            return userForm;
        }

        private T ExtractEnum<T>(ControllerContext controllerContext, ModelBindingContext bindingContext, string pattern)
            where T : struct
        {
            if (!typeof (T).IsEnum)
                throw new ArgumentException("Expected an enumeration.");

            var keys =
                controllerContext.HttpContext.Request.Form.AllKeys.Where(
                    k => System.Text.RegularExpressions.Regex.IsMatch(k, pattern + "\\[.+\\]"));

            var values = new List<T>();

            foreach (var key in keys)
            {
                values.Add((T) Enum.Parse(typeof (T), controllerContext.HttpContext.Request.Form[key]));
            }

            bindingContext.ModelState.Remove(pattern);

            return EnumHelpers.Combine(values);
        }
    }
}