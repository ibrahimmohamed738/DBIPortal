using DBI_eDahab.Web.Helpers;
using DBI_eDahab.Web.Models;
using DBI_eDahab.Web.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using static DBI_eDahab.Web.ViewModels.Users;

namespace DBI_eDahab.Web.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        FluxCubeApi _fluxCubeApi = new FluxCubeApi();
        Repository _repository = new Repository();
        DahabApi _dahab = new DahabApi();
        UsersRepository _usersRepository = new UsersRepository();

        public async Task<ActionResult> GetEdahabName(string MSISDN)
        {
            var edahabName = await _dahab.GetUserInfo(MSISDN);
            return Json(edahabName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEdahabNameFromDB(string MSISDN)
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
            req.AlternateAccountId = AccountNo;
            AccountInfoRespone AccountHolderName = await getAccountInfo(req);
            return Json(AccountHolderName, JsonRequestBehavior.AllowGet);
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


        public async Task<ActionResult> GetEdahabCustomerPhoto(string MSISDN)
        {
            var photo = await _dahab.GetSubscriberPhoto(MSISDN);
            return File(photo, "image/jpg");
        }

        [PermissionRequired(Permissions.Register_customers)]
        public ActionResult RegisterCustomer()
        {
            string userName = User.Identity.Name.ToString();
            return View();
        }

        [PermissionRequired(Permissions.Register_customers)]
        [HttpPost]
        public async Task<ActionResult> RegisterCustomer(CustomerForm custForm)
        {
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

                        await _fluxCubeApi.SendSmsAsync("DBI", custForm.MSISDN, $"Macmiil, Dahabshil Bank International waxay kuu furtay adeega Dahabi, short-code-ka adeegu waa *777# PIN-kaagu waa {pin}");
                        TempData["Success"] = "Successfully Saved Customer";
                        AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), ActivityType = "RegisterCustomerMSISDN", Description = "Successfully registered customer MSISDN: " + custForm.MSISDN, AffectedParty = custForm.MSISDN };
                        _usersRepository.LogUserAction(auditLogRecord); 
                        if (Request.UrlReferrer != null)
                            return Redirect(Request.UrlReferrer.ToString());
                    }
                }
            return View();
        }


        [PermissionRequired(Permissions.Register_customers)]
        public ActionResult AddCustomerAccounts()
        {
            string userName = User.Identity.Name.ToString();
            return View();
        }

        [PermissionRequired(Permissions.Register_customers)]
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

        [PermissionRequired(Permissions.ModifyCustomers)]
        public ActionResult ModifyCustomer(string MSISDN, string AccountNo, string AccountType)
        {
            var AccountInfo = _repository.GetAccountInfo(MSISDN, AccountNo, AccountType);
            TempData["Active"] = AccountInfo.Active;
            return View(AccountInfo);
        }

        [PermissionRequired(Permissions.ModifyCustomers)]
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
                var hasher = System.Security.Cryptography.SHA1.Create();
                var hash = hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(randomPassword));
                string hashedPassword = string.Join("", hash.Select(b => b.ToString("x2")));

                _repository.ChangePin(editCustomer.MSISDN, hashedPassword);

                await _fluxCubeApi.SendSmsAsync("DBI", editCustomer.MSISDN, $"Macmiil, Pin-kaagii ayaa laguu badalay , Pin-ka cusubi waa : {randomPassword}");
                auditLogRecord.ActivityType = "ResetCustomerPIN";
                auditLogRecord.Description = "Successfully reseted customer(" + editCustomer.MSISDN + ") PIN";
                auditLogRecord.AffectedParty = editCustomer.MSISDN;
            }
            _usersRepository.LogUserAction(auditLogRecord);
            return RedirectToAction("Customers", "Reports");

        }


        [PermissionRequired(Permissions.Verify)]
        public ActionResult VerifyCustomer(string MSISDN, string AccountNo, string AccountType)
        {
            var AccountInfo = _repository.GetAccountInfo(MSISDN, AccountNo, AccountType);
            return View(AccountInfo);
        }


        [PermissionRequired(Permissions.Verify)]
        [HttpPost]
        public async Task<ActionResult> VerifyCustomer(CustomerForm customer, string submitButton)
        {
            AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), AffectedParty = customer.CustomerAccount.AccountNo };
            switch (submitButton)
            {
                case "Verify":
                    _repository.VerifyCustomer(customer.CustomerAccount.AccountNo, customer.CustomerAccount.AccountType);
                    await _fluxCubeApi.SendSmsAsync("DBI", customer.MSISDN, $"Dear customer your DBI account {customer.CustomerAccount.AccountNo} has been successfully verified");
                  
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