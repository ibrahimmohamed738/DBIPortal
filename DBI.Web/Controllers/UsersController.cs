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

namespace DBI_eDahab.Web.Controllers
{
    public class UsersController : Controller
    {
        // GET: Users

        UsersRepository _usersRepository = new UsersRepository();
        eDahabServiceApi.eDahabServiceSoapClient _eDahabApi = new eDahabServiceApi.eDahabServiceSoapClient("eDahabServiceSoap");


        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Update_Users)]
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



        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Create_Users)]

        public ActionResult AddUsers()
        {
            return View();
        }
        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Create_Users)]
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
                    //SendSMSRequest sms = new SendSMSRequest()
                    //{
                    //    MSG = string.Format("Your Username is {0} and Password is: {1}", addUser.UserName, randomPassword),
                    //    MSISDN = addUser.MobileNumber,
                    //    CallerID = WebConfigurationManager.AppSettings["APIUser"].ToString(),
                    //    CallerPassword = WebConfigurationManager.AppSettings["APIPassword"].ToString()
                    //};
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    await SendSmsAsync("DBI", addUser.MobileNumber, $"Your Username is {addUser.UserName} and Password is: {randomPassword}");
                    //_pentBankApi.Send_SMS(sms);
                    TempData["SuccessMessage"] = "User has been successfully created.";
                    AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), ActivityType = "CreateUser", Description = $"Create new user: {addUser.FullName},{addUser.MobileNumber}", AffectedParty = addUser.UserName };
                    _usersRepository.LogUserAction(auditLogRecord);
                    return RedirectToAction("UpdateUser", new { UserId = addUser.UserName });
                }
            }
            return View(addUser);
        }

        //[PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Add_Branch)]

        //public ActionResult AddBranchs()
        //{
        //    return View();
        //}
        //[PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Add_Branch)]
        //[HttpPost]
        //public ActionResult AddBranchs(Branchs branch)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _usersRepository.AddBranch(branch);
        //        if (Request.UrlReferrer != null) return Redirect(Request.UrlReferrer.ToString());
        //    }
        //    return View();
        //}

        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Update_Users)]
        public ActionResult UpdateUser(string userId)
        {
            var user = _usersRepository.GetUser(userId);

            if (user == null) return View("UserNotFound");

            var userForm = new UpdateUserForm(user);

            ViewBag.Permissions = DBI_eDahab.Web.Helpers.EnumHelpers.ToDictionary<DBI_eDahab.Web.ViewModels.Users.Permissions>();
            return View(userForm);
        }

        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Update_Users)]
        [HttpPost]
        public async Task<ActionResult> UpdateUser([ModelBinder(typeof(UserFormModelBinder))] UpdateUserForm updateUserForm)
        {
            if (_usersRepository != null)
            {
                _usersRepository.UpdateUser(updateUserForm);
                HttpContext.Cache[string.Format("{0}'s CurrentPermissions", updateUserForm.UserName)] = updateUserForm.CurrentPermissions;
                ViewBag.Permissions = DBI_eDahab.Web.Helpers.EnumHelpers.ToDictionary<DBI_eDahab.Web.ViewModels.Users.Permissions>();
                TempData["SuccessMessage"] = "User updated successfully.";
                if (updateUserForm.ResetPassword)
                {
                    var randomPassword = new Random().Next(10000, 99999).ToString();
#pragma warning disable 618
                    var hashedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(randomPassword, "SHA1");
#pragma warning restore 618
                    _usersRepository.ChangePassword(updateUserForm.UserName, hashedPassword);
                    //SendSMSRequest sms = new SendSMSRequest()
                    //{
                    //    MSG = string.Format("Your Password is: {0}", randomPassword),
                    //    MSISDN = updateUserForm.MobileNumber,
                    //    CallerID = WebConfigurationManager.AppSettings["APIUser"].ToString(),
                    //    CallerPassword = WebConfigurationManager.AppSettings["APIPassword"].ToString()
                    //};
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    await SendSmsAsync("DBI", updateUserForm.MobileNumber, $"Your Password is: {randomPassword}");
                    //_pentBankApi.Send_SMS(sms);
                    AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), ActivityType = "ResetUserPassword", Description = $"Reset password of: {updateUserForm.Name},{updateUserForm.MobileNumber}", AffectedParty = updateUserForm.UserName };
                    _usersRepository.LogUserAction(auditLogRecord);
                    //TempData["SuccessMessage"] += string.Format(" New Password is {0}.", randomPassword);
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

        public async Task SendSmsAsync(string title, string phone, string message)
        {
            using (var client = new HttpClient())
            {
                if (phone.StartsWith("65") || phone.StartsWith("66"))
                    client.BaseAddress = new Uri("http://192.168.21.45:50030/");
                else
                    client.BaseAddress = new Uri("http://192.168.23.90:5000/");

                var request = new
                {
                    phone,
                    title,
                    message
                };
                var json = JsonConvert.SerializeObject(request);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync("SMS", data);
            }
        }


    }


}