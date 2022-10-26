using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class UserInfo
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string CategoryCode { get; set; }
        public string Gender { get; set; }
        public string Msisdn { get; set; }
        public string Status { get; set; }
    }
}