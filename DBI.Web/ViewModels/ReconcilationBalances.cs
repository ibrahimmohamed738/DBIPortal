using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class ReconcilationBalances
    {
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "DBI Agent USD Balance")]
        public decimal DBIAgentUSDBalance;
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "DBI Agent SLS Balance")]
        public decimal DBIAgentSLSBalance;
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "eDahab USD Account Balance")]
        public decimal eDahabUSDAccountBalance;
        //[DisplayFormat(DataFormatString = "{0:C}")]
        [DataType(DataType.Currency)]
        [Display(Name = "eDahab SLS Account Balance")]
        public decimal eDahabSLSAccountBalance;
        [DataType(DataType.Currency)]
        [Display(Name = "USD Difference (eDahab - DBI")]
        public decimal USDDifference { get { return DBIAgentUSDBalance - eDahabUSDAccountBalance; } }
        [DataType(DataType.Currency)]
        [Display(Name = "SLS Difference (eDahab - DBI")]
        public decimal SLSDifference { get { return DBIAgentSLSBalance - eDahabSLSAccountBalance; } }

        public IPagedList<Transaction> Transactions;
    }
}