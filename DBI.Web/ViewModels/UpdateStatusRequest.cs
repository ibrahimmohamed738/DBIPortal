using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class UpdateStatusRequest
    {
        [Required]
        public string AccountNo { get; set; }
        [Required]
        public bool Status { get; set; }
    }
}