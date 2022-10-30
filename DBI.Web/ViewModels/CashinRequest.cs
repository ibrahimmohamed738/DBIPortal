using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class CashResponse
    {
        public string StatusCode { get; set; }

        public string Message { get; set; }
        public string TransactionId { get; set; }
    }

    public class CashinRequest
    {
        public string Phone { get; set; }

        public decimal Amount { get; set; }

        public string AgentLongCode { get; set; }

        public string Currency { get; set; }

        public bool BlockSMS { get; set; } = false;

        public string Category { get; set; } = "SUBS";

        public string Description { get; set; } = "DBI Process";

        public string TransactionId { get; set; }

        public string CellId { get; set; }

        public string PIN { get; set; }
    }
}