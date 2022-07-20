using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class CustomerAccountForm
    {
        [Required, Display(Name = "Account No.")]
        public string AccountNo { get; set; }
        [Required, Display(Name = "Account Holder")]
        public string AccountHolder { get; set; }
        [Display(Name = "Account Type")]
        public string AccountType { get; set; }
        public string Branch { get; set; }
        public string Currency { get; set; }
        [Required, Display(Name = "Daily Limit")]
        public decimal DailyLimit { get; set; }
        [Required, Display(Name = "New Daily Limit")]
        public decimal NewDailyLimit { get; set; }
        [Required]
        public string MSISDN { get; set; }
        [Required, Display(Name = "eDahab Name")]
        public string eDahabName { get; set; }
        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
        [Required, Display(Name = "Verification Status")]
        public bool Verified { get; set; }
    }
}