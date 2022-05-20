using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DBI_eDahab.Web.Models;

namespace DBI_eDahab.Web.ViewModels
{
    public class ChangePasswordForm
    {
        UsersRepository usersRepository = new UsersRepository();


        public string UserName { get; set; }

        [Required, Display(Name = "Old Password")]
        public string OldPassword { get; set; }

        [Required, StringLength(32, MinimumLength = 8), Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required, StringLength(32, MinimumLength = 8), Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; }

        public static string HashedPassword(string password)
        {

            var hasher = System.Security.Cryptography.SHA1.Create();
            var hash = hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return string.Join("", hash.Select(b => b.ToString("x2")));
            //return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var username = HttpContext.Current.User.Identity.Name;

            if (!HttpContext.Current.User.Identity.Name.Equals(username))
            {
                yield return new ValidationResult("You can only change your own username.", new string[] { "Username" });
            }
            var identity = HttpContext.Current.User.Identity.Name.ToString();
            var user = usersRepository.GetUserByLogin(identity);

            if (!user.Password.Equals(HashedPassword(this.OldPassword)))
            {
                yield return new ValidationResult("Wrong old password.", new string[] { "OldPassword" });
            }

            if (this.NewPassword != null && !this.NewPassword.Equals(this.ConfirmNewPassword))
            {
                yield return new ValidationResult("New Password and Confirm New Password fields must match", new string[] { "ConfirmNewPassword" });
            }
        }

    }
}