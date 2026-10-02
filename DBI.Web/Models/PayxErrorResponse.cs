using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.Models
{
    public class PayxTransactionResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class PayxErrorResponse
    {
        public string Message { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string Detail { get; set; }
    }
}