using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBI_eDahab.Web.Models;
using DBI_eDahab.Web.ViewModels;

namespace DBI_eDahab.Web.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {
        Repository _repository = new Repository();
        UsersRepository _usersRepository = new UsersRepository();
        public ActionResult RegisterCustomer()
        {
            string userName = User.Identity.Name.ToString();
            //ViewBag.UserBranch = _usersRepository.GetUser(userName);
            return View();
        }
        [HttpPost]
        public ActionResult RegisterCustomer(CustomerForm branch)
        {
            if (ModelState.IsValid)
            {
                ViewBag.UserBranch = _usersRepository.GetUser(User.Identity.Name.ToString());
                 _repository.RegisterCustomer(branch);
                if (Request.UrlReferrer != null) return Redirect(Request.UrlReferrer.ToString());
            }
            return View();
        }
    }
}