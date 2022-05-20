using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class UpdateUserForm
    {
        public UpdateUserForm() : this(null) { }

        public UpdateUserForm(Users user)
        {
            if (user == null) return;
            this.Id = user.Id;
            //this.Title = user.Title;
            this.Name = user.FullName;
            this.UserName = user.UserName;
            //   this.Department = user.Department;
            this.Password = user.Password;
            this.Email = user.Email;
            this.MobileNumber = user.MobileNumber;
            this.CurrentPermissions = user.CurrentPermissions;
            this.Active = user.Active;
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public Users.Permissions CurrentPermissions { get; set; }

        public bool Active { get; set; }

        //public string Title { get; set; }
        [Required]
        public string UserName { get; set; }

        public string MobileNumber { get; set; }
        [Required]
        // public string Department { get; set; }
        public string Password { get; set; }


        [Display(Name = "Reset Password")]
        public bool ResetPassword { get; set; }

        public string HashedPassword(string Password)
        {
#pragma warning disable 618
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(Password, "SHA1");
#pragma warning restore 618
        }

        public string Email { get; set; }
    }
}