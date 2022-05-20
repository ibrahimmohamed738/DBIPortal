using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class FilterTransactions
    {
        public string term { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Status { get; set; }
        public string Currency { get; set; }
        public string Branch { get; set; }
        public string Name { get; set; }
        public Decimal? Amount { get; set; }
    }
}