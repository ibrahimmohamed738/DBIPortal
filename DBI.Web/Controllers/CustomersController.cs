using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBI_eDahab.Web.Helpers;
using DBI_eDahab.Web.Models;
using DBI_eDahab.Web.ViewModels;
using System.Web.Security;
using DBI_eDahab.Web.DBIWebService;
using System.Net;
using System.Web.Configuration;
using System.Collections.Specialized;

namespace DBI_eDahab.Web.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        Repository _repository = new Repository();
        UsersRepository _usersRepository = new UsersRepository();
        //PentaServiceApi.PentaServiceClient _pentaServiceApi = new PentaServiceApi.PentaServiceClient();
        DBIWebserviceClient _pentBankApi = new DBIWebserviceClient("BasicHttpsBinding_IService1");
        eDahabServiceApi.EDahabApiSouthSoapClient _eDahabApi = new eDahabServiceApi.EDahabApiSouthSoapClient("eDahabServiceSoap");




        public ActionResult GetEdahabName(string MSISDN, string Category)
        {
            eDahabServiceApi.AuthHeader authHeader = new eDahabServiceApi.AuthHeader()
            {
                Username = WebConfigurationManager.AppSettings["eDahabUser"].ToString(),
                Password = WebConfigurationManager.AppSettings["eDahabPassword"].ToString()
            };
            var edahabName = _eDahabApi.GetCustomerInfo(authHeader, MSISDN);
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


        public ActionResult GetDBIAccountHolder(AccountInfoRequest req, string AccountNo)
        {

            AccountInfo AccountHolderName = getAccountInfo(req);
            return Json(AccountHolderName, JsonRequestBehavior.AllowGet);
        }

        private AccountInfo getAccountInfo(AccountInfoRequest req)
        {
            AccountInfo result = new AccountInfo();
            req.BranchCode = GetUser().Branch.ToString();
            //This has to be changed for the HO of each reqion
            if (req.BranchCode == "200")
                req.BranchCode = req.AccountNo.ToString().Substring(0, 3);
            req.CallerID = WebConfigurationManager.AppSettings["APIUser"].ToString();
            req.CallerPassword = WebConfigurationManager.AppSettings["APIPassword"].ToString();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            AccountInfo[] list = _pentBankApi.SearchAccount(req);
            switch(list.Length)
            {
                case 0:
                    result.Status = -1;
                    result.Message = "Account not found";
                    break;
                case 1:
                    result = list[0];
                    break;
                default:
                    result.Status = 1;
                    result.Message = "conflict of accounts please contract IT department";
                    break;
            }
            return result;
        }

        private Users GetUser()
        {
            return _usersRepository.GetUser(User.Identity.Name.ToString());
        }

        public ActionResult AccountTypes()
        {
            var getTypes = _pentBankApi.GetCustomerAccountTypes();
            return Json(getTypes.List, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBranchs()
        {
            var getcodes = _pentBankApi.GetBranchesCodes();
            return Json(getcodes.List, JsonRequestBehavior.AllowGet);
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
        public ActionResult RegisterCustomer(CustomerForm custForm)
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
                    SendSMSRequest sms = new SendSMSRequest()
                    {
                        MSG = $"Macmiil, Dahabshil Bank International waxay kuu furtay adeega Dahabi, short-code-ka adeegu waa *777# PIN-kaagu waa {pin}",
                        MSISDN = custForm.MSISDN,
                        CallerID = WebConfigurationManager.AppSettings["SMSUser"].ToString(),
                        CallerPassword = WebConfigurationManager.AppSettings["SMSPassword"].ToString()
                    };
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    _pentBankApi.Send_SMS(sms);
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
        public ActionResult ModifyCustomer(string userId, CustomerForm editCustomer)
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
                SendSMSRequest sms = new SendSMSRequest()
                {
                    MSG = string.Format("Macmiil, Pin-kaagii ayaa laguu badalay , Pin-ka cusubi waa : {0}", randomPassword),
                    MSISDN = editCustomer.MSISDN,
                    CallerID = WebConfigurationManager.AppSettings["SMSUser"].ToString(),
                    CallerPassword = WebConfigurationManager.AppSettings["SMSPassword"].ToString()
                };
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                _pentBankApi.Send_SMS(sms);
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
        public ActionResult VerifyCustomer(CustomerForm customer, string submitButton)
        {
            AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), AffectedParty = customer.CustomerAccount.AccountNo };
            switch (submitButton)
            {
                case "Verify":
                    _repository.VerifyCustomer(customer.CustomerAccount.AccountNo, customer.CustomerAccount.AccountType);
                    SendSMSRequest sms = new SendSMSRequest()
                    {
                        MSG = string.Format("Dear customer your DBI account {0} has been successfully verified", customer.CustomerAccount.AccountNo),
                        MSISDN = customer.MSISDN,
                        CallerID = WebConfigurationManager.AppSettings["SMSUser"].ToString(),
                        CallerPassword = WebConfigurationManager.AppSettings["SMSPassword"].ToString()
                    };
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    _pentBankApi.Send_SMS(sms);
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