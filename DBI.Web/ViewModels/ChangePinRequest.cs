using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class ChangePinRequest
    {
        public string Msisdn { get; set; }
        public string NewPassword { get; set; }
        public string ModifiedBy { get; set; }
    }
}