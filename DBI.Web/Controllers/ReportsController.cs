using DBI_eDahab.Web.Helpers;
using DBI_eDahab.Web.Models;
using DBI_eDahab.Web.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;
using static DBI_eDahab.Web.ViewModels.Users;

namespace DBI_eDahab.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        // GET: Reports
        FluxCubeApi _fluxCubeApi = new FluxCubeApi();
        Repository _repository = new Repository();
        DahabApi _dahab = new DahabApi();

        private class CustomerReport
        {


            public string MSISDN { get; set; }
            public string eDahabName { get; set; }
            public string AccountHolder { get; set; }
            public string AccountNo { get; set; }
            public string AccountType { get; set; }
            public string Branch { get { return AccountNo.Substring(0, 3); } }
            public string Currency { get; set; }
            public decimal DailyLimit { get; set; }
            public DateTime CreatedOn { get; set; }
            public string CreatedBy { get; set; }
            public bool Verified { get; set; }
            public string Active { get; set; }
            public CustomerReport(CustomerForm customerForm)
            {
                AccountNo = customerForm.CustomerAccount.AccountNo;
                AccountHolder = customerForm.CustomerAccount.AccountHolder;
                AccountType = customerForm.CustomerAccount.AccountType;
                DailyLimit = customerForm.CustomerAccount.DailyLimit;
                CreatedBy = customerForm.CustomerAccount.CreatedBy;
                CreatedOn = customerForm.CustomerAccount.CreatedOn;
                Currency = customerForm.CustomerAccount.Currency;
                Verified = customerForm.CustomerAccount.Verified;
                MSISDN = customerForm.MSISDN;
                eDahabName = customerForm.eDahabName;
                Active = customerForm.Active;
            }
        }


        [PermissionRequired(Permissions.Customers_List)]
        public ActionResult Customers(string term, DateTime? DateFrom = null, DateTime? DateTo = null, int page = 1, string Currency = null, string Branch = null, string Active = "", bool? Verified = null)
        {
            List<CustomerForm> list = _repository.GetCustomers((Session["User"] as Users).UserName,Term: term, DateFrom: DateFrom, DateTo: DateTo, Currency: Currency, Branch: Branch, Active: Active, Verified: Verified);
            var customers = list.ToPagedList(page, 30);
            Session["CustomersSession"] = customers;
            return View(customers);
        }


        [PermissionRequired(Permissions.Transactions)]
        public ActionResult Transactions(FilterTransactions filter,int page = 1)
        {
            var transaction = new List<Transaction>();
            IPagedList<Transaction> transactions = new PagedList<Transaction>(transaction, page,30);
            if (filter.DateFrom.HasValue && filter.DateTo.HasValue || filter.term != null || filter.Status != null || filter.Currency != null || filter.Name != null || filter.Amount != null )
            {
                List<Transaction> list = _repository.Transactions(filter, (Session["User"] as Users).UserName);
                transactions = list.ToPagedList(page, list.Count > 0 ? list.Count : 1);
            }
            Session["TransactionsSession"] = transactions;
            return View(transactions);
        }


        [PermissionRequired(Permissions.ProcessFailures)]
        public async Task<ActionResult> ProcessFailedTransaction(string transactionID)
        {
            bool isNorth;
            bool.TryParse(ConfigurationManager.AppSettings["IsNorth"], out isNorth);
            var trans = _repository.GetPendingTransaction(transactionID, (Session["User"] as Users).UserName);
            var edahabInfo = await _dahab.GetUserInfo(trans.MSISDN);
            //var checkDBITrans = await _fluxCubeApi.GetDBITransaction(new CheckDBITransRequest { Entity="DBI", ExternalTransactionId= transactionID });
            
            if (trans != null && trans.TransactionType == "DEPOSIT")
            {
                var createTrans = new CreateTransactionRequest
                {
                    Entity = "DBI",
                    AlternateAccountId = trans.AccountId,
                    Amount = trans.Amount,
                    ExternalTransactionId = trans.EdahabTransactionId,
                    Narrative = transactionID,
                    Market = isNorth ? "North" : "South",
                    TransactionType = "Deposit",
                    Currency = trans.Currency
                };
                var res = await _fluxCubeApi.CreateTransaction(createTrans);
                if (res.StatusCode == "200")
                {
                    _repository.UpdateTransactionLog_DBISide(true, true, transactionID, res.TransactionCode,false);
                    TempData["Success"] = res.Message;
                    return Redirect(Request.UrlReferrer.ToString());
                }
                else
                {
                    TempData["Error"] = "Process Failed";
                    return Redirect(Request.UrlReferrer.ToString());
                }
            }

            if (trans != null && trans.TransactionType == "WITHDRAWAL")
            {
                var cashinreq = new CashinRequest
                {
                   TransactionId = trans.DBITransactionId,
                   Phone = trans.MSISDN,
                   Amount = trans.Amount,
                   Currency = trans.Currency == "USD" ? "101" : "102",
                   AgentLongCode = ConfigurationManager.AppSettings["AgentMsisdn"].ToString()
                };
                CashResponse dahabRes = null ;
                if (edahabInfo.CategoryCode.Equals("SUBS", StringComparison.OrdinalIgnoreCase))
                    dahabRes = await _dahab.SubscriberCashInAsync(cashinreq);
                else
                    dahabRes = await _dahab.MerchantInAsync(cashinreq);

                if (dahabRes.StatusCode == "200")
                {
                    _repository.UpdateTransactionLog_eDahabSide(true, true, trans.Narration, dahabRes.TransactionId, false);
                    TempData["Success"] = dahabRes.Message;
                    return Redirect(Request.UrlReferrer.ToString());
                }
                else 
                {
                    TempData["Error"] = dahabRes.Message;
                    return Redirect(Request.UrlReferrer.ToString());
                }
                
            }
            TempData["Error"] = "Transaction not found.";
            return Redirect(Request.UrlReferrer.ToString());
        }

        [PermissionRequired(Permissions.Reconciliation)]
        public async Task<ActionResult> Reconciliation(FilterTransactions filter, int page = 1)
        {
            var glBalance = await _fluxCubeApi.GetGLAccountBalance(new GLAccountRequest {
                Entity = "DBI",
                AccountId = "100200102",
                BranchId = ConfigurationManager.AppSettings["GLBranch"].ToString(),
                Currency = "USD"
            });
            ReconcilationBalances reconcilationBalances = new ReconcilationBalances();
            foreach (var item in glBalance)
            {
                if(item.Currency == "SLS")
                  reconcilationBalances.eDahabSLSAccountBalance = item.Balance;
                else
                  reconcilationBalances.eDahabUSDAccountBalance = item.Balance;
            }

            var agentBalance = await _dahab.GetAgentBalance(ConfigurationManager.AppSettings["AgentMsisdn"]);
            reconcilationBalances.DBIAgentSLSBalance = agentBalance.SLSBalance;
            reconcilationBalances.DBIAgentUSDBalance = agentBalance.USDBalance;
            if (!(filter.DateFrom.HasValue && filter.DateTo.HasValue || filter.term != null))
            {
                filter.DateFrom = new DateTime(2021, 1, 1);
                filter.DateTo = DateTime.Today;
            }
            filter.Status = "0";
            List<Transaction> list = _repository.Transactions(filter, (Session["User"] as Users).UserName);
            reconcilationBalances.Transactions = list.ToPagedList(1, list.Count > 0 ? list.Count : 1);
            Session["TransactionsSession"] = reconcilationBalances.Transactions;

            return View(reconcilationBalances);
        }



        public ActionResult ExportTransactionsExcel()
        {
            var result = (PagedList<Transaction>)Session["TransactionsSession"];
            if (result.Count == 0)
            {
                TempData["SessionNull"] = "Nothing to export";
            }
            else
            {
                GridView gv = new GridView();
                /*List<Transaction> ExportedData = result.ToList();
                for(int pageNumber = 2; pageNumber <= result.PageCount; pageNumber ++)
                {
                    List<Transaction> temp = (result.ToPagedList<Transaction>(pageNumber,result.PageSize).ToList());
                    ExportedData.AddRange(temp);
                }*/
                gv.DataSource = result;
                gv.DataBind();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Transactions.xls");
                Response.ContentType = "application/ms-excel";
                Response.Charset = "";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
                return RedirectToAction("Transactions");
            }
            return RedirectToAction("Transactions");
        }

        public ActionResult ExportCustomersExcel()
        {
            var result = (PagedList<CustomerForm>)Session["CustomersSession"];
            if (result.Count == 0)
            {
                TempData["SessionNull"] = "Nothing to export";
            }
            else
            {
                List<CustomerReport> data = new List<CustomerReport>();
                foreach (var item in result)
                {
                    data.Add(new CustomerReport(item));
                }
                GridView gv = new GridView();
                /*List<Transaction> ExportedData = result.ToList();
                for(int pageNumber = 2; pageNumber <= result.PageCount; pageNumber ++)
                {
                    List<Transaction> temp = (result.ToPagedList<Transaction>(pageNumber,result.PageSize).ToList());
                    ExportedData.AddRange(temp);
                }*/
                gv.DataSource = data;
                gv.DataBind();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Customers.xls");
                Response.ContentType = "application/ms-excel";
                Response.Charset = "";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
                return RedirectToAction("Customers");
            }
            return RedirectToAction("Customers");
        }

    }
}