using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class GLAccountResponse
    {
        public string AccountId { get; set; }
        public string BranchId { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
    }
}