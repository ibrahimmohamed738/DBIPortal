using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class GetCustomerAccountResponse
    {
        public List<GetCustomerAccount> Accounts { get; set; } = new List<GetCustomerAccount>();
        public int Pages { get; set; }
        public int CurrentPage { get; set; }
    }
}