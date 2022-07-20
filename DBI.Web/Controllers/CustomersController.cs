using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBI_eDahab.Web.Helpers;
using DBI_eDahab.Web.Models;
using DBI_eDahab.Web.ViewModels;
using System.Web.Security;
using System.Net;
using System.Web.Configuration;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace DBI_eDahab.Web.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        eDahabServiceApi.eDahabServiceSoapClient _eDahabApi = new eDahabServiceApi.eDahabServiceSoapClient("eDahabServiceSoap");
        FluxCubeApi _fluxCubeApi = new FluxCubeApi();
        Repository _repository = new Repository();
        UsersRepository _usersRepository = new UsersRepository();

        public ActionResult GetEdahabName(string MSISDN, string Category)
        {
            eDahabServiceApi.AuthHeader authHeader = new eDahabServiceApi.AuthHeader()
            {
                Username = WebConfigurationManager.AppSettings["eDahabUser"].ToString(),
                Password = WebConfigurationManager.AppSettings["eDahabPassword"].ToString()
            };
            var edahabName = _eDahabApi.GetEDahabInfo(MSISDN);
            return Json(edahabName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEdahabNameFromDB(string MSISDN, string Category)
        {
            var edahabName = _repository.CheckIfMSISDNExists(MSISDN);
            if (edahabName == null)
                return Json("N/A", JsonRequestBehavior.AllowGet);
            else
                return Json(edahabName.eDahabName, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> GetDBIAccountHolder(AccountInfoRequest req, string AccountNo)
        {
            req.Entity = "DBI";
            req.AccountId = AccountNo;
            req.Currency = "USD";
            req.AlternateAccountId = AccountNo;
            AccountInfoRespone AccountHolderName = await getAccountInfo(req);
            return Json(AccountHolderName.Name, JsonRequestBehavior.AllowGet);
        }

        private async Task<AccountInfoRespone> getAccountInfo(AccountInfoRequest req)
        {
            AccountInfoRespone account = await _fluxCubeApi.GetCustomerInfo(req);
            return account;
        }

        private Users GetUser()
        {
            return _usersRepository.GetUser(User.Identity.Name.ToString());
        }

        public string GetDatetime()
        {
            DateTime d = new DateTime();
            d = DateTime.Now;
            string ds = d.ToString("yyyyMMddHHmmss");
            return ds;
        }


        public ActionResult GetEdahabCustomerPhoto(string MSISDN)
        {
            var customer = _repository.GetSubscriberByMobileNumber(MSISDN);
            var photo = _repository.GetCustomerPhotoByUserId(customer);
            return File(photo, "image/jpg");
        }

        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Register_customers)]
        public ActionResult RegisterCustomer()
        {
            //MSISDN = "659969009";
            string userName = User.Identity.Name.ToString();
            return View();
        }

        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Register_customers)]
        [HttpPost]
        public async Task<ActionResult> RegisterCustomer(CustomerForm custForm)
        {
            //if (ModelState.IsValid)
            //{
               // var accountholder = GetAccountInfo(req);
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "This account does not exist";
                }
                else
                {
                    if (_repository.CheckIfMSISDNExists(custForm.MSISDN) != null)
                    {
                        TempData["MSISDNInUse"] = "Customer With this Mobile Number Exists";
                    }
                    else
                    {
                        var pin = new Random().Next(1000, 9999).ToString();
                        var hasher = System.Security.Cryptography.SHA1.Create();
                        var hash = hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
                        string hashedPassword = string.Join("", hash.Select(b => b.ToString("x2")));
                        custForm.PIN = hashedPassword;
                        _repository.RegisterCustomer(custForm);

                        await _fluxCubeApi.SendSmsAsync("DBI-MobileBanking", custForm.MSISDN, $"Macmiil, Dahabshil Bank International waxay kuu furtay adeega Dahabi, short-code-ka adeegu waa *777# PIN-kaagu waa {pin}");
                        TempData["Success"] = "Successfully Saved Customer";
                        AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), ActivityType = "RegisterCustomerMSISDN", Description = "Successfully registered customer MSISDN: " + custForm.MSISDN, AffectedParty = custForm.MSISDN };
                        _usersRepository.LogUserAction(auditLogRecord); 
                        if (Request.UrlReferrer != null)
                            return Redirect(Request.UrlReferrer.ToString());
                    }
                }
            //}
            return View();
        }


        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Register_customers)]
        public ActionResult AddCustomerAccounts()
        {
            string userName = User.Identity.Name.ToString();
            return View();
        }

        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Register_customers)]
        [HttpPost]
        public ActionResult AddCustomerAccounts(CustomerAccountForm custForm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Error in data";
            }
            else
            {
                AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), ActivityType = "AddCustomerAccount", AffectedParty = custForm.AccountNo };
                if (_repository.CheckIfMSISDNExists(custForm.MSISDN) == null)
                {
                    TempData["MSISDNInUse"] = "Register eDahab account first";
                }
                else
                {
                    switch(_repository.RegisterCustomerAccount(custForm))
                    {
                        case -2:
                            TempData["Error"] = "This account is already exists";
                            auditLogRecord.Description = "Failed attempt to Add account no. to MSISDN: " + custForm.MSISDN;
                            break;
                        case 0:
                            auditLogRecord.Description = "Failed attempt to Add account no. to MSISDN: " + custForm.MSISDN;
                            TempData["Error"] = "SQL Exception occured";
                            break;
                        case 1:
                            auditLogRecord.Description = "Successful attempt to Add account no. to MSISDN: " + custForm.MSISDN;
                            TempData["Success"] = "Successfully added account, please verify it";
                            break;
                        case -1: 
                        default: TempData["Error"] = "Exception occured";
                        break;
                    }
                    _usersRepository.LogUserAction(auditLogRecord);
                    if (Request.UrlReferrer != null) return Redirect(Request.UrlReferrer.ToString());
                }
            }
            return View();
        }
        /*
        // [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.AddCustomerAccounts)]
        [HttpPost]
        public ActionResult AddCustomerAccounts(CustomerAccountForm custForm, AccountInfoRequest req)
        {
            //if (ModelState.IsValid)
            //{
            var accountholder = GetAccountInfo(req);
            if (_pentBankApi.GetAccountInfo(req).Status != 0)
            {
                TempData["Error"] = "This account does not exist";
            }
            else
            {
                if (_repository.CheckIfMSISDNExists(custForm.MSISDN) == null)
                {
                    TempData["MSISDNInUse"] = "Register Customer eDahab First";
                }
                else
                {
                    //var pin = new Random().Next(1000, 9999).ToString();
                    //var hashedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(pin, "SHA1");
                    //custForm.PIN = hashedPassword;
                    //custForm.Branch = GetUser().Branch.ToString();
                    _repository.RegisterCustomerAccount(custForm);
                    //_eDahabApi.SendSMS(custForm.MSISDN, string.Format("Macmiil, Dahabshil Bank International waxay kuu furtay adeega Dahabi, short-code-ka adeegu waa *885# PIN-kaagu waa {0}", pin));
                    TempData["Success"] = "Successfully Saved Customer Account";
                    if (Request.UrlReferrer != null) return Redirect(Request.UrlReferrer.ToString());
                }
            }
            // }
            return View();
        }
        */

        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.ModifyCustomers)]
        public ActionResult ModifyCustomer(string MSISDN, string AccountNo, string AccountType)
        {
            var AccountInfo = _repository.GetAccountInfo(MSISDN, AccountNo, AccountType);
            //AccountInfo.CustomerAccount.AccountType = ((NameValueCollection)WebConfigurationManager.GetSection("accountsTypes"))[AccountInfo.CustomerAccount.AccountType];
            TempData["Active"] = AccountInfo.Active;
            return View(AccountInfo);
        }

        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.ModifyCustomers)]
        [HttpPost]
        public async Task<ActionResult> ModifyCustomer(string userId, CustomerForm editCustomer)
        {
            AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), ActivityType = "ModifyCustomerAccount" };
            _repository.UpdateCustomer(editCustomer, TempData["Active"].ToString());
            if (!(TempData["Active"].ToString().Equals(editCustomer.Active)))
            {
                auditLogRecord.AffectedParty = editCustomer.MSISDN;
                auditLogRecord.Description = "Successully updated customer(" + editCustomer.MSISDN + ") status to: " + editCustomer.Active;
            }
            else if(editCustomer.CustomerAccount.NewDailyLimit != editCustomer.CustomerAccount.DailyLimit && editCustomer.CustomerAccount.NewDailyLimit > 0)
            {
                auditLogRecord.AffectedParty = editCustomer.CustomerAccount.AccountNo;
                auditLogRecord.Description = "Successully updated customer(" + editCustomer.MSISDN + ") daily limit to: " + editCustomer.CustomerAccount.NewDailyLimit;
            }
            TempData["SuccessMessage"] = "Successfully Updated Customer";
            if (editCustomer.ResetPin)
            {
                var randomPassword = new Random().Next(1000, 9999).ToString();
#pragma warning disable 618
                var hashedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(randomPassword, "SHA1");
#pragma warning restore 618
                _repository.ChangePin(editCustomer.MSISDN, hashedPassword);

                await _fluxCubeApi.SendSmsAsync("DBI-MobileBanking", editCustomer.MSISDN, $"Macmiil, Pin-kaagii ayaa laguu badalay , Pin-ka cusubi waa : {randomPassword}");
                auditLogRecord.ActivityType = "ResetCustomerPIN";
                auditLogRecord.Description = "Successfully reseted customer(" + editCustomer.MSISDN + ") PIN";
                auditLogRecord.AffectedParty = editCustomer.MSISDN;
                //TempData["SuccessMessage"] += string.Format(" New Password is {0}.", randomPassword);
            }
            _usersRepository.LogUserAction(auditLogRecord);
            return RedirectToAction("Customers", "Reports");
            //return View(editCustomer);
        }


        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Verify)]
        public ActionResult VerifyCustomer(string MSISDN, string AccountNo, string AccountType)
        {
            var AccountInfo = _repository.GetAccountInfo(MSISDN, AccountNo, AccountType);
            return View(AccountInfo);
        }


        [PermissionRequired(DBI_eDahab.Web.ViewModels.Users.Permissions.Verify)]
        [HttpPost]
        public async Task<ActionResult> VerifyCustomer(CustomerForm customer, string submitButton)
        {
            AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), AffectedParty = customer.CustomerAccount.AccountNo };
            switch (submitButton)
            {
                case "Verify":
                    _repository.VerifyCustomer(customer.CustomerAccount.AccountNo, customer.CustomerAccount.AccountType);
                    await _fluxCubeApi.SendSmsAsync("DBI-MobileBanking", customer.MSISDN, $"Dear customer your DBI account {customer.CustomerAccount.AccountNo} has been successfully verified");
                  
                    TempData["SuccessMessage"] = "Successfully Verfied Customer";
                    auditLogRecord.ActivityType = "VerifyCustomerAccount";
                    auditLogRecord.Description = "Successfully verified customer("+ customer.MSISDN+") account";
                    break;
                case "Delete":
                    _repository.DeleteCustomer(customer);
                    TempData["SuccessMessage"] = "Successfully deleted customer account";
                    auditLogRecord.ActivityType = "DeleteCustomerAccount";
                    auditLogRecord.Description = "Successfully deleted customer(" + customer.MSISDN + ") account";
                    break;
            }
            _usersRepository.LogUserAction(auditLogRecord);
            return RedirectToAction("Customers", "Reports");

            //return View();
        }
    }
}