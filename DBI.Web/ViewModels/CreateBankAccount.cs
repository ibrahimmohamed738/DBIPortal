using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class CreateBankAccount
    {
        [Required]
        public string MSISDN { get; set; }
        [Required]
        public string EDahabName { get; set; }
        public string EDahabType { get; set; } = "";
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string PIN { get; set; }
        public string CreatedBy { get; set; }
        public string Active { get; set; } = "Y";
        public bool Verified { get; set; } = false;
        public string Remarks { get; set; }
    }
}