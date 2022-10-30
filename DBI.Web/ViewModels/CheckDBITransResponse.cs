using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class CheckDBITransResponse
    {
        public string ExternalTransactionId { get; set; }
        public string FCUBSTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string SourceAccountId { get; set; }
        public string RecipientAccountId { get; set; }
        public string ProductCode { get; set; }
        public string BranchCode { get; set; }
        public string Currency { get; set; }
        public string TransactionCode { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}