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
        MobileBankAPI _mobileBankAPI = new MobileBankAPI();
        public async Task<ActionResult> GetEdahabName(string MSISDN)
        {
            var edahabName = await _dahab.GetUserInfo(MSISDN);
            return Json(edahabName, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetExternalName(string MSISDN)
        {
            var result = await _mobileBankAPI.GetCustomerAccount(MSISDN);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetCustomerSign(string AccountNo)
        {
            var result = await _fluxCubeApi.GetCustomerSignature(new AccountInfoRequest { AlternateAccountId = AccountNo, Entity="DBI" });
            return File(result, "image/jpg");
        }

        public async Task<ActionResult> GetCustomerPhoto(string AccountNo)
        {
            var result = await _fluxCubeApi.GetCustomerPhoto(new AccountInfoRequest { AlternateAccountId = AccountNo, Entity = "DBI" });
            return File(result, "image/jpg");
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
        public async Task<ActionResult> RegisterCustomer(CreateBankAccount custForm)
        {       
            custForm.MSISDN = 252 + custForm.MSISDN;
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "This account does not exist";
                }
                else
                {
                var pin = new Random().Next(1000, 9999).ToString();
                string userName = User.Identity.Name.ToString();
                custForm.PIN = pin;
                custForm.CreatedBy = userName;
                custForm.Remarks = userName + " new entry";
                custForm.EDahabType = "";
                var response = await _mobileBankAPI.CreateAccount(custForm);

                if (response.Success)
                {
                    await _fluxCubeApi.SendSmsAsync("DBI", custForm.MSISDN, $"Macmiil, Dahabshil Bank International waxay kuu furtay adeega Dahabi, short-code-ka adeegu waa *777# PIN-kaagu waa {pin}");
                    TempData["Success"] = response.Message;
                }
                    
                else
                    TempData["Error"] = response.Message;

                AuditLog auditLogRecord = new AuditLog { UserName = userName, ActivityType = "RegisterCustomerMSISDN", Description = "Successfully registered customer MSISDN: " + custForm.MSISDN, AffectedParty = custForm.MSISDN };
                 _usersRepository.LogUserAction(auditLogRecord); 
                 if (Request.UrlReferrer != null)
                    return Redirect(Request.UrlReferrer.ToString());
                }
            return View();
        }


        [PermissionRequired(Permissions.Register_customers)]
        public ActionResult RegisterExternalCustomer()
        {
            string userName = User.Identity.Name.ToString();
            return View();
        }


        [PermissionRequired(Permissions.Register_customers)]
        [HttpPost]
        public async Task<ActionResult> RegisterExternalCustomer(CreateBankAccountExternal custForm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "This account does not exist";
            }
            else
            {
                var pin = new Random().Next(1000, 9999).ToString();
                string userName = User.Identity.Name.ToString();
                custForm.PIN = pin;
                custForm.CreatedBy = userName;
                custForm.Remarks = userName + " new entry";
                custForm.EDahabType = "";

                var model = new CreateBankAccount
                {
                    MSISDN = custForm.MSISDN,
                    PIN = custForm.PIN,
                    CreatedBy = custForm.CreatedBy,
                    Remarks = custForm.Remarks,
                    EDahabName = custForm.Name,
                    EDahabType = custForm.EDahabType,
                    Active = custForm.Active,
                    Verified = custForm.Verified,
                    CreatedOn = custForm.CreatedOn
                };
                var response = await _mobileBankAPI.CreateAccount(model);

                if (response.Success)
                {
                    await _fluxCubeApi.SendSMS(new SendSMSRequest { Entity = "DBI", Message= $"Macmiil,Dahabshil Bank International waxay kuu furtay adeega Dahabi,PIN-kaagu waa {pin}", Mobile = custForm.MSISDN });
                    TempData["Success"] = response.Message;
                }

                else
                    TempData["Error"] = response.Message;

                AuditLog auditLogRecord = new AuditLog { UserName = userName, ActivityType = "RegisterExternalCustomer", Description = "Successfully registered customer MSISDN: " + custForm.MSISDN, AffectedParty = custForm.MSISDN };
                _usersRepository.LogUserAction(auditLogRecord);
                if (Request.UrlReferrer != null)
                    return Redirect(Request.UrlReferrer.ToString());
            }
            return View();
        }


        [PermissionRequired(Permissions.Register_customers)]
        public ActionResult AddCustomerAccounts()
        {
            return View();
        }

        [PermissionRequired(Permissions.Register_customers)]
        [HttpPost]
        public async Task<ActionResult> AddCustomerAccounts(LinkAccount custForm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Error in data";
            }
            else
            {
                var username = User.Identity.Name.ToString();
                AuditLog auditLogRecord = new AuditLog { UserName = username, ActivityType = "AddCustomerAccount", AffectedParty = custForm.AccountNo };
                custForm.AccountNo = (await _fluxCubeApi.GetCustomerInfo(new AccountInfoRequest { AlternateAccountId = custForm.AccountNo, Entity = "DBI" })).AccountId;
                custForm.CreatedBy = username;
                custForm.EDahabType = "";
                custForm.DailyLimit = custForm.NewDailyLimit;
                custForm.Remarks = username + " New entry.";
                var response = await _mobileBankAPI.LinkAccount(custForm);
                if (response.Success)
                {
                    auditLogRecord.Description = "Successful attempt to Add account no. to MSISDN: " + custForm.MSISDN;
                    _usersRepository.LogUserAction(auditLogRecord);
                    TempData["Success"] = response.Message;
                }
                else
                {
                    auditLogRecord.Description = response.Message;
                     _usersRepository.LogUserAction(auditLogRecord);
                    TempData["Error"] = response.Message;
                }
                   
                
              
                if (Request.UrlReferrer != null) return Redirect(Request.UrlReferrer.ToString());
            }
            return View();
        }


        [PermissionRequired(Permissions.ModifyCustomers)]
        public async Task<ActionResult> ModifyCustomer(string MSISDN, string AccountNo, string AccountType)
        {
            var accountInfo = await _mobileBankAPI.GetByAccountByMsisdn(MSISDN, AccountNo);
            var modify = new ModifyRequest 
            {
                Msisdn = accountInfo.MSISDN,
                EDahabName = accountInfo.EDahabName,
                Currency = accountInfo.Currency,
                AccountHolder = accountInfo.AccountHolder,
                AccountType = accountInfo.AccountType,
                AccountNo = accountInfo.AccountNo,
                Active = accountInfo.Active,
                DailyLimit = accountInfo.DailyLimit,
                NewDailyLimit = accountInfo.NewDailyLimit,
                Remarks = accountInfo.Remarks,
            };
            if (string.IsNullOrEmpty(modify.Remarks))
                modify.Remarks = User.Identity.Name.ToString();

            //var AccountInfo = _repository.GetAccountInfo(MSISDN, AccountNo, AccountType);
            TempData["Active"] = accountInfo.Active;
            return View(modify);
        }

        [PermissionRequired(Permissions.ModifyCustomers)]
        [HttpPost]
        public async Task<ActionResult> ModifyCustomer(string userId, ModifyRequest editCustomer)
        {
            AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), ActivityType = "ModifyCustomerAccount" };
            editCustomer.ModifiedBy = User.Identity.Name.ToString();
            editCustomer.Remarks = User.Identity.Name.ToString() + editCustomer.Remarks;
            var updated =  await _mobileBankAPI.UpdateAccount(editCustomer);
            //_repository.UpdateCustomer(editCustomer, TempData["Active"].ToString());
            if (updated.Success)
            {
                auditLogRecord.AffectedParty = editCustomer.Msisdn;
                auditLogRecord.Description = "Successully updated customer(" + editCustomer.Msisdn + ") status to: " + editCustomer.Active;
            }
            TempData["SuccessMessage"] = updated.Message;
            if (editCustomer.ResetPin)
            {
                var randomPassword = new Random().Next(1000, 9999).ToString();

                var result = await _mobileBankAPI.ChangePin(new ChangePinRequest { Msisdn = editCustomer.Msisdn, ModifiedBy = User.Identity.Name.ToString(), NewPassword = randomPassword });
                if (result.Success)
                {
                    await _fluxCubeApi.SendSmsAsync("DBI", editCustomer.Msisdn.Substring(3), $"Macmiil, Pin-kaagii ayaa laguu badalay , Pin-ka cusubi waa : {randomPassword}");
                    auditLogRecord.ActivityType = "ResetCustomerPIN";
                    auditLogRecord.Description = "Successfully reseted customer(" + editCustomer.Msisdn + ") PIN";
                    auditLogRecord.AffectedParty = editCustomer.Msisdn;
                }
                else
                {
                    auditLogRecord.ActivityType = "ResetCustomerPIN";
                    auditLogRecord.Description = "Customer(" + editCustomer.Msisdn + ") PIN not reseted";
                    auditLogRecord.AffectedParty = editCustomer.Msisdn;
                }
               
            }
            _usersRepository.LogUserAction(auditLogRecord);
            return RedirectToAction("Customers", "Reports");

        }


        [PermissionRequired(Permissions.Verify)]
        public async Task<ActionResult> VerifyCustomer(string MSISDN, string AccountNo, string AccountType)
        {
            var AccountInfo = await _mobileBankAPI.GetByAccountByMsisdn(MSISDN, AccountNo);
            //var AccountInfo = _repository.GetAccountInfo(MSISDN, AccountNo, AccountType);
            return View(AccountInfo);
        }


        [PermissionRequired(Permissions.Verify)]
        [HttpPost]
        public async Task<ActionResult> VerifyCustomer(GetCustomerAccount customer, string submitButton)
        {
            AuditLog auditLogRecord = new AuditLog { UserName = User.Identity.Name.ToString(), AffectedParty = customer.AccountNo };
            switch (submitButton)
            {
                case "Verify":
                    var response = await _mobileBankAPI.VerifyAccount(new VerifyCustomer { AccountNo = customer.AccountNo, VerifiedBy = User.Identity.Name.ToString() });
                    if (response.Success)
                    {
                        await _fluxCubeApi.SendSmsAsync("DBI", customer.MSISDN, $"Dear customer your DBI account {customer.AccountNo} has been successfully verified");
                        TempData["SuccessMessage"] = response.Message;
                        auditLogRecord.ActivityType = "VerifyCustomerAccount";
                        auditLogRecord.Description = "Successfully verified customer(" + customer.MSISDN + ") account";
                    }
                    else 
                    {
                        TempData["SuccessMessage"] = response.Message;
                        auditLogRecord.ActivityType = "VerifyCustomerAccount";
                        auditLogRecord.Description = "Failed to verify customer(" + customer.MSISDN + ") account";
                    }
                    break;
                case "Delete":
                    //_repository.DeleteCustomer(customer);
                    var result = await _mobileBankAPI.DeleteAccount(customer.AccountNo);
                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = result.Message;
                        auditLogRecord.ActivityType = "DeleteCustomerAccount";
                        auditLogRecord.Description = $"Successfully deleted customer({customer.AccountNo}-{customer.MSISDN}) account";
                    }
                    else
                    {
                        TempData["Error"] = result.Message;
                        auditLogRecord.ActivityType = "DeleteCustomerAccount";
                        auditLogRecord.Description = $"Failed to delete customer({customer.AccountNo}-{customer.MSISDN}) account";
                    }
                    break;
            }
            _usersRepository.LogUserAction(auditLogRecord);
            return RedirectToAction("Customers", "Reports");

            //return View();
        }
    }
}