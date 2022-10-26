using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class CreateTransactionRequest
    {
        public string AccountId { get; set; }
        public string AlternateAccountId { get; set; }
        public string Narrative { get; set; }
        public string BranchId { get; set; }
        public decimal Amount { get; set; }
        public string Entity { get; set; }
        public string ExternalTransactionId { get; set; }
        public string Market { get; set; }
        public string TransactionType { get; set; }
        public string Currency { get; set; }
    }
}