using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class DBITransaction
    {
        public string TransferId { get; set; }
        public decimal Amount { get; set; }
        public string CELLID { get; set; }
        public string FTXNID { get; set; }

    }
}