using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class CustomerForm
    {
        public int Id { get; set; }
        [Required]
        public string MSISDN { get; set; }
        [Required]
        [Display(Name = "eDahab Name")]
        public string eDahabName { get; set; }
        public string PIN { get; set; }
        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }

        public string CreatedBy { get; set; }
        [Required, Display(Name = "eDahab Type")]
        public string eDahabType { get; set; }

        [Display(Name = "Reset Pin")]
        public bool ResetPin{ get; set; }
        public string Active { get; set; }
        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        public CustomerAccountForm  CustomerAccount { get; set; }
        public CustomerForm()
        {
            CustomerAccount = new CustomerAccountForm();
        }
    }
}