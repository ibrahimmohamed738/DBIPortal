using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DBI_eDahab.Web.Models;
using DBI_eDahab.Web.ViewModels;
using DBI_eDahab.Web.DBIWebService;
using System.Web.Configuration;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DBI_eDahab.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        // GET: Authentication

        UsersRepository _usersRepository = new UsersRepository();
        eDahabServiceApi.EDahabApiSouthSoapClient _eDahabApi = new eDahabServiceApi.EDahabApiSouthSoapClient("EDahabApiSouthSoap");
        DBIWebserviceClient _pentBankApi = new DBIWebserviceClient("BasicHttpsBinding_IDBIWebservice");

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginForm loginForm)
        {
            if (ModelState.IsValid)
            {
                var hasher = System.Security.Cryptography.SHA1.Create();
                var hash = hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginForm.Password));
                string hashedPassword = string.Join("", hash.Select(b => b.ToString("x2")));
                //var hashedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(loginForm.Password, "SHA1");
                var user = _usersRepository.AuthenticateUser(loginForm.UserName, hashedPassword);
                var credentialsAreValid = user != null;
                bool use2FA;
                bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["Use2FA"], out use2FA);
                if (credentialsAreValid && use2FA)
                {
                    var token = new Random().Next(1000, 9999).ToString();
                    Session["User"] = user;
                    HttpContext.Cache[string.Format("{0}'s CurrentPermissions", user.UserName)] = user.CurrentPermissions;
                    await SendSmsAsync("DBI", user.MobileNumber, token);
                    //  _usersRepository.SendSomtelSms("252" + user.MobileNumber, token, asFlash: true);

                    //SendSMSRequest sms = new SendSMSRequest()
                    //{
                    //    MSG = token,
                    //    MSISDN = user.MobileNumber,
                    //    CallerID = WebConfigurationManager.AppSettings["APIUser"].ToString(),
                    //    CallerPassword = WebConfigurationManager.AppSettings["APIPassword"].ToString()
                    //};
                    //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    //_pentBankApi.Send_SMS(sms);
                    Session["Token"] = token;
                    return RedirectToAction("Token");
                }
                else if (credentialsAreValid)
                {
                    Session["User"] = user;
                    HttpContext.Cache[string.Format("{0}'s CurrentPermissions", user.UserName)] = user.CurrentPermissions;
                    FormsAuthentication.SetAuthCookie(user.UserName, false);
                    _usersRepository.LogUserAction(new AuditLog { UserName = user.UserName, ActivityType = "Login", Description = "Successful login attempt", AffectedParty = user.UserName }); 
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid Username or Password.";
                }
            }
            return View(loginForm);
        }
        public async Task<ActionResult> ResendToken()
        {
            var user = Session["User"] as Users;
            var expectedToken = Session["Token"] as string;
            if (user != null && expectedToken != null)
            {
                var token = new Random().Next(1000, 9999).ToString();
                await SendSmsAsync("DBI",user.MobileNumber,token);
                //     _usersRepository.SendSomtelSms("252" + user.MobileNumber, token, asFlash: true);
                //SendSMSRequest sms = new SendSMSRequest()
                //{
                //    MSG = token,
                //    MSISDN = user.MobileNumber,
                //    CallerID = WebConfigurationManager.AppSettings["APIUser"].ToString(),
                //    CallerPassword = WebConfigurationManager.AppSettings["APIPassword"].ToString()
                //};
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //_pentBankApi.Send_SMS(sms);

                Session["Token"] = token;
                TempData["SuccessMessage"] = "Token has been resent.";
                return RedirectToAction("Token");
            }
            else
            {
                TempData["ErrorMessage"] = "Please login to receive a new token.";
                return RedirectToAction("Login");
            }
        }

        public ActionResult Token()
        {
            var user = Session["User"] as Users;
            var expectedToken = Session["Token"] as string;

            if (user == null || expectedToken == null)
            {
                TempData["ErrorMessage"] = "Please login to receive a new token.";

                return RedirectToAction("Login");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Token(string token)
        {
            var user = Session["User"] as Users;
            var expectedToken = Session["Token"] as string;
            if (user != null && !string.IsNullOrWhiteSpace(token) && token.Equals(expectedToken, StringComparison.OrdinalIgnoreCase))
            {
                Session.Remove("Token");
                if (user.UserName != null) FormsAuthentication.SetAuthCookie(user.UserName, false);
                _usersRepository.LogUserAction(new AuditLog { UserName = user.UserName, ActivityType = "Login", Description = "Successful login attempt", AffectedParty = user.UserName }); 
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Session.Clear();
                TempData["ErrorMessage"] = "Invalid token";
                return RedirectToAction("Login");
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            Session["User"] = null;

            TempData["SuccessMessage"] = "You have been logged out of the system.";

            return RedirectToAction("Login");
        }


        public async Task SendSmsAsync(string title, string phone, string message)
        {
            using (var client = new HttpClient())
            {
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