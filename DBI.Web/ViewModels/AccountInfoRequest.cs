using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class AccountInfoRequest
    {
        public string Entity { get; set; }
        public string AccountId { get; set; }
        public string AlternateAccountId { get; set; }
        public string Currency { get; set; }
    }
}