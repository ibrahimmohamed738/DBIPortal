using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class UpdateLimitRequest
    {
        [Required]
        [Display(Name = "Account No")]
        public string AccountNo { get; set; }
        [Required]
        [Display(Name = "New Limit")]
        public decimal NewLimit { get; set; }
    }
}