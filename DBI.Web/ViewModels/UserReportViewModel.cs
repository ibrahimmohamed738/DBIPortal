using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DBI_eDahab.Web.ViewModels
{
    public class UserReportViewModel
    {
        public string Platform { get; set; }

        public string Branch { get; set; }

        public int? UserId { get; set; }

        public List<SelectListItem> Branches { get; set; }

        public List<SelectListItem> Users { get; set; }

        public IPagedList<UserReportItem> Report { get; set; }
    }

    public class UserReportItem
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string MobileNumber { get; set; }

        public DateTime? RegisteredDate { get; set; }

        public string Branch { get; set; }

        public string BranchName { get; set; }

        public bool Active { get; set; }
    }

    public class BranchItem
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }

    public class UserDropdownItem
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }
    }
}