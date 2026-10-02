using DBI_eDahab.Web.Helpers;
using DBI_eDahab.Web.Services;
using DBI_eDahab.Web.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static DBI_eDahab.Web.ViewModels.Users;

namespace DBI_eDahab.Web.Controllers
{
    [PermissionRequired(Permissions.Users_Branch_Report)]
    public class UserReportController : Controller
    {
        private readonly UserReportService _userService;
        private readonly BranchService _branchService;

        public UserReportController()
        {
            _userService = new UserReportService();
            _branchService = new BranchService();
        }

        public ActionResult Index(
     string branch = "",
     int? userId = null,
     int page = 1)
        {
            const int pageSize = 20;

            bool isNorth;

            bool.TryParse(
                System.Configuration.ConfigurationManager
                    .AppSettings["IsNorth"],
                out isNorth);

            var platform = isNorth
                ? "North"
                : "South";

            var branches =
                _branchService.GetBranches(platform);

            var model = new UserReportViewModel
            {
                Platform = platform,
                Branch = branch,
                UserId = userId,

                Branches = branches
                    .Select(x => new SelectListItem
                    {
                        Value = x.Code,
                        Text = x.Name,
                        Selected = x.Code == branch
                    })
                    .ToList(),

                Users = new List<SelectListItem>()
            };

            List<UserReportItem> report;

            if (!string.IsNullOrWhiteSpace(branch))
            {
                model.Users =
                    _userService
                        .GetUsersByBranch(branch)
                        .Select(x => new SelectListItem
                        {
                            Value = x.Id.ToString(),

                            Text =
                                string.IsNullOrWhiteSpace(x.FullName)
                                    ? x.UserName
                                    : x.FullName + " (" +
                                      x.UserName + ")",

                            Selected =
                                userId.HasValue &&
                                x.Id == userId.Value
                        })
                        .ToList();

                report =
                    _userService.GetReport(
                        branch,
                        userId);
            }
            else
            {
                var branchCodes =
                    branches
                        .Select(x => x.Code)
                        .ToList();

                report =
                    _userService
                        .GetReportByBranches(
                            branchCodes,
                            userId);
            }

            foreach (var item in report)
            {
                item.BranchName =
                    _branchService
                        .GetBranchName(item.Branch);
            }

            model.Report =
                report.ToPagedList(page, pageSize);

            return View(model);
        }

        [HttpGet]
        public JsonResult GetUsers(string branch)
        {
            bool isNorth;

            bool.TryParse(
                ConfigurationManager.AppSettings["IsNorth"],
                out isNorth);

            var platform = isNorth
                ? "North"
                : "South";

            if (string.IsNullOrWhiteSpace(branch))
            {
                return Json(
                    new object[0],
                    JsonRequestBehavior.AllowGet);
            }

            if (!_branchService.IsValidBranch(
                platform,
                branch))
            {
                return Json(
                    new object[0],
                    JsonRequestBehavior.AllowGet);
            }

            var users =
                _userService
                    .GetUsersByBranch(branch)
                    .Select(x => new
                    {
                        id = x.Id,

                        text =
                            string.IsNullOrWhiteSpace(x.FullName)
                                ? x.UserName
                                : x.FullName + " (" +
                                  x.UserName + ")"
                    })
                    .ToList();

            return Json(
                users,
                JsonRequestBehavior.AllowGet);
        }


        public ActionResult ExportExcel(
    string branch = "",
    int? userId = null)
        {
            bool isNorth;

            bool.TryParse(
                System.Configuration.ConfigurationManager
                    .AppSettings["IsNorth"],
                out isNorth);

            var platform =
                isNorth
                    ? "North"
                    : "South";

            List<UserReportItem> report;

            if (!string.IsNullOrWhiteSpace(branch))
            {
                report =
                    _userService.GetReport(
                        branch,
                        userId);
            }
            else
            {
                var branchCodes =
                    _branchService
                        .GetBranches(platform)
                        .Select(x => x.Code)
                        .ToList();

                report =
                    _userService
                        .GetReportByBranches(
                            branchCodes,
                            userId);
            }


            foreach (var item in report)
            {
                item.BranchName =
                    _branchService
                        .GetBranchName(item.Branch);
            }


            var grid =
                new System.Web.UI.WebControls.GridView();

            grid.DataSource =
                report.Select(x => new
                {
                    x.UserName,

                    x.FullName,

                    x.Email,

                    x.MobileNumber,

                    Branch = x.BranchName,

                    BranchCode = x.Branch,

                    RegisteredDate =
                        x.RegisteredDate.HasValue
                            ? x.RegisteredDate.Value
                                .ToString("dd/MM/yyyy HH:mm")
                            : "",
                    Status =
                        x.Active
                            ? "Active"
                            : "Inactive"
                });

            grid.DataBind();


            Response.ClearContent();

            Response.Buffer = true;

            Response.AddHeader(
                "content-disposition",
                "attachment; filename=UserReport_" +
                DateTime.Now.ToString("yyyyMMddHHmmss") +
                ".xls");

            Response.ContentType =
                "application/vnd.ms-excel";


            using (var sw =
                   new System.IO.StringWriter())
            {
                using (var htw =
                       new System.Web.UI.HtmlTextWriter(sw))
                {
                    grid.RenderControl(htw);

                    Response.Output.Write(
                        sw.ToString());

                    Response.Flush();

                    Response.End();
                }
            }

            return null;
        }
    }
}