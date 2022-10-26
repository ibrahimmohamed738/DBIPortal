using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class CreateTransactionResponse
    {
        public string StatusCode { get; set; }
        public string TransactionCode { get; set; }
        public string Message { get; set; }
        public string CustomerName { get; set; }
        public string AccountId { get; set; }
        public string OffSetAccountId { get; set; }
        public string ExternalTransactionId { get; set; }
    }
}