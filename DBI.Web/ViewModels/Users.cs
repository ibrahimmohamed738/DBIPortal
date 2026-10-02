using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class Users
    {
        public int Id { get; set; }
        [Required,Display(Name ="Full Name")]
        public string FullName { get; set; }
        [Required,Display(Name ="User Name")]
        public string UserName { get; set; }
        public string Password { get; set; }
        [Required,Display(Name ="Mobile Number")]
        public string MobileNumber { get; set; }
        public int Branch { get; set; }

        [Required,DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public DateTime RegisteredDate { get; set; }
        public Permissions CurrentPermissions { get; set; }
        public bool Active { get; set; }

        public enum Permissions
        {
            None = 0,
            Create_Users = 1 << 0,
            Update_Users = 1 << 1,
            Register_customers = 1 << 2,
            Customers_List = 1 << 3,
            Transactions = 1 << 4,
            ModifyCustomers = 1 <<5,
            Verify = 1 << 6,
            Reconciliation = 1 << 7,
            ProcessFailures = 1 << 8,
            Update_Limit = 1 << 9,
            Registration_Report = 1 << 10,
            Bulk_Update = 1 << 11,
            Users_Branch_Report = 1 << 12,
        }
    }
}