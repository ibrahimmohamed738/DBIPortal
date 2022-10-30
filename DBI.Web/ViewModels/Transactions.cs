using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class Transaction
    {
        public int Id { get; set; }
        public string AccountId { get; set; }
        public string AccountType { get; set; }
        public string Branch { get; set; }
        public string MSISDN { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime Dated { get; set; }
        public bool Status { get; set; }
        public string TransactionType { get; set; }
        public string Narration { get; set; }
        public string EdahabTransactionId { get; set; }
        public string DBITransactionId { get; set; }
    }
}