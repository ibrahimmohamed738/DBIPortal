using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.Models
{
    public class PayxBalanceResponse
    {
        public string Id { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Category { get; set; }

        public List<PayxBalance> Balances { get; set; }
    }

    public class PayxBalance
    {
        public decimal Balance { get; set; }
        public decimal FrozenAmount { get; set; }
        public decimal FicAmount { get; set; }
        public string Currency { get; set; }
    }
}