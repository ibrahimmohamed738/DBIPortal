using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class AccountInfoRespone
    {
        public string AlternateAccountId { get; set; }
        public string AccountId { get; set; }
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public string BranchId { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string Currency { get; set; }
        public string Email { get; set; }
    }
}