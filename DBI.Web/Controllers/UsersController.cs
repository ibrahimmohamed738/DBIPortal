using DBI_eDahab.Web.Models;
using DBI_eDahab.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DBI_eDahab.Web.Helpers;
using System.Web.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using static DBI_eDahab.Web.ViewModels.Users;

namespace DBI_eDahab.Web.Controllers
{
    public class UsersController : Controller
    {
        // GET: Users

        UsersRepository _usersRepository = new UsersRepository();
        FluxCubeApi _fluxCubeApi = new FluxCubeApi();

        [PermissionRequired(Permissions.Update_Users)]
        public ActionResult Index(UserSearch userSearchForm)
        {
            if (!string.IsNullOrWhiteSpace(userSearchForm.SearchTerm))
            {
                var searchResults = _usersRepository.SearchUsers(userSearchForm.SearchTerm, userSearchForm.FirstRow, userSearchForm.LastRow);
                userSearchForm.UsersCount = searchResults.Item1;
                userSearchForm.SearchResults = searchResults.Item2;


            }
            return View(userSearchForm);
        }



        [PermissionRequired(Permissions.Create_Users)]

        public ActionResult AddUsers()
        {
            return View();
        }
        [PermissionRequired(Permissions.Create_Users)]
        [HttpPost]
        public async Task<ActionResult> AddUsers(string userId, Users addUser)
        {
            if (ModelState.IsValid)
            {
                if (_usersRepository.GetUser(addUser.UserName) != null)
                {
                    TempData["ErrorMessage"] = "This User Already Exists, Please choose different LoginId Or MobileNumber";
                    return View("AddUsers", addUser);
                }
                else
                {
                    var randomPassword = new Random().Next(10000, 99999).ToString();
#pragma warning disable 618
                    var hashedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(randomPassword, "SHA1");
                    addUser.Password = hashedPassword;
                    _usersRepository.AddUser(addUser);
                    await _fluxCubeApi.SendSmsAsync("DBI", addUser.MobileNumber, $"Your Username is {addUser.UserName} and Password is: {randomPassword}");
                    TempData["SuccessMessage"] = "User has been successfully created.";
                    AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), ActivityType = "CreateUser", Description = $"Create new user: {addUser.FullName},{addUser.MobileNumber}", AffectedParty = addUser.UserName };
                    _usersRepository.LogUserAction(auditLogRecord);
                    return RedirectToAction("UpdateUser", new { UserId = addUser.UserName });
                }
            }
            return View(addUser);
        }

        [PermissionRequired(Permissions.Update_Users)]
        public ActionResult UpdateUser(string userId)
        {
            var user = _usersRepository.GetUser(userId);

            if (user == null) return View("UserNotFound");

            var userForm = new UpdateUserForm(user);

            ViewBag.Permissions = EnumHelpers.ToDictionary<Permissions>();
            return View(userForm);
        }

        [PermissionRequired(Permissions.Update_Users)]
        [HttpPost]
        public async Task<ActionResult> UpdateUser([ModelBinder(typeof(UserFormModelBinder))] UpdateUserForm updateUserForm)
        {
            if (_usersRepository != null)
            {
                _usersRepository.UpdateUser(updateUserForm);
                HttpContext.Cache[string.Format("{0}'s CurrentPermissions", updateUserForm.UserName)] = updateUserForm.CurrentPermissions;
                ViewBag.Permissions = EnumHelpers.ToDictionary<Permissions>();
                TempData["SuccessMessage"] = "User updated successfully.";
                if (updateUserForm.ResetPassword)
                {
                    var randomPassword = new Random().Next(10000, 99999).ToString();
#pragma warning disable 618
                    var hashedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(randomPassword, "SHA1");
#pragma warning restore 618
                    _usersRepository.ChangePassword(updateUserForm.UserName, hashedPassword);
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    await _fluxCubeApi.SendSmsAsync("DBI", updateUserForm.MobileNumber, $"Your Password is: {randomPassword}");
                    AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), ActivityType = "ResetUserPassword", Description = $"Reset password of: {updateUserForm.Name},{updateUserForm.MobileNumber}", AffectedParty = updateUserForm.UserName };
                    _usersRepository.LogUserAction(auditLogRecord);
                }
            }
            return RedirectToAction("UpdateUser", new { UserId = updateUserForm.UserName });
        }


        public ActionResult ChangePassword()
        {
            var changePasswordForm = new ChangePasswordForm();

            return View(changePasswordForm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordForm changePasswordForm)
        {
            if (ModelState.IsValid)
            {
                var identity = User.Identity.Name.ToString();
                _usersRepository.ChangePassword(User.Identity.Name, ChangePasswordForm.HashedPassword(changePasswordForm.NewPassword));
                TempData["SuccessMessage"] = "Successfully changed the password.";

                return RedirectToAction("ChangePassword");
            }

            return View(changePasswordForm);
        }

    }


}