using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class ModifyRequest
    {
        public string ModifiedBy { get; set; }
        public string Msisdn { get; set; }
        public string EDahabName { get; set; }
        public string Currency { get; set; }
        public string AccountHolder { get; set; }
        [Required]
        public string Remarks { get; set; }
        public string AccountNo { get; set; }
        public string AccountType { get; set; }
        public decimal NewDailyLimit { get; set; }
        public decimal DailyLimit { get; set; }
        public string Active { get; set; }
        [Display(Name = "Reset Pin")]
        public bool ResetPin { get; set; }
    }
}