using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class CheckBalanceRequest
    {
        public long AccountNo { get; set; }
        public string AccountType { get; set; }
        public string BranchCode { get; set; }
        public string TransactionNo { get; set; }
        public string CallerID { get; set; }
    }
}