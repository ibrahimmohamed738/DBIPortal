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
            List<CustomerForm> list = _repository.GetCustomers(Term: term, DateFrom: DateFrom, DateTo: DateTo, Currency: Currency, Branch: Branch, Active: Active, Verified: Verified);
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
                List<Transaction> list = _repository.Transactions(filter);
                transactions = list.ToPagedList(page, list.Count > 0 ? list.Count : 1);
            }
            Session["TransactionsSession"] = transactions;
            return View(transactions);
        }


        //[PermissionRequired(Permissions.ProcessFailures)]
        //public ActionResult ProcessFailedTransaction(string TransactionID)
        //{
        //    try
        //    {
        //        CorrectTransactionRequest req = new CorrectTransactionRequest();
        //        req.TransactionID = TransactionID;
        //        req.CallerID = WebConfigurationManager.AppSettings["APIUser"].ToString();
        //        req.CallerPassword = WebConfigurationManager.AppSettings["APIPassword"].ToString();
        //        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        //        CorrectTransactionResponse res = _DBIApi.ProcessFailedTransaction(req);
        //        if (res.Status == 1 || res == null)
        //            TempData["Error"] = "Exception Occured";
        //        else
        //            TempData["Success"] = res.Message;
        //        return Redirect(Request.UrlReferrer.ToString());
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        TempData["Error"] = e.Message;
        //        return Redirect(Request.UrlReferrer.ToString());
        //    }
        //}

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
            List<Transaction> list = _repository.Transactions(filter);
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