using DBI_eDahab.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.Services
{
    public class BranchService
    {
        private static readonly List<BranchItem> SouthBranches =
            new List<BranchItem>
            {
                new BranchItem { Code = "300", Name = "RO2 Somalia" },
                new BranchItem { Code = "301", Name = "Mogadishu Branch" },
                new BranchItem { Code = "303", Name = "Beladweyn Branch" },
                new BranchItem { Code = "304", Name = "Baidao Branch" },
                new BranchItem { Code = "305", Name = "Kismayo Branch" },
                new BranchItem { Code = "308", Name = "Bakaro Branch" },
                new BranchItem { Code = "309", Name = "Hamarweyne Branch" },
                new BranchItem { Code = "310", Name = "Suuq baad" },
                new BranchItem { Code = "311", Name = "Jowhar Branch" },
                new BranchItem { Code = "312", Name = "Beledhawo Branch" },
                new BranchItem { Code = "313", Name = "Abud waaq" },
                new BranchItem { Code = "314", Name = "Dusamareb Branch" },
                new BranchItem { Code = "315", Name = "Banadir Branch" }
            };

        private static readonly List<BranchItem> NorthBranches =
            new List<BranchItem>
            {
                new BranchItem { Code = "200", Name = "RO1 Somaliland" },
                new BranchItem { Code = "201", Name = "Hargeisa Branch" },
                new BranchItem { Code = "202", Name = "Burao Branch" },
                new BranchItem { Code = "203", Name = "Berbera Branch" },
                new BranchItem { Code = "204", Name = "Borama Branch" },
                new BranchItem { Code = "205", Name = "Lasanod Branch" },
                new BranchItem { Code = "206", Name = "Erigavo Branch" },
                new BranchItem { Code = "207", Name = "Wajaale Branch" },
                new BranchItem { Code = "208", Name = "Star Branch" },
                new BranchItem { Code = "209", Name = "Jigjiga Yar Branch" },
                new BranchItem { Code = "210", Name = "Theatre Branch" },
                new BranchItem { Code = "302", Name = "Bosaso Branch" },
                new BranchItem { Code = "306", Name = "Garowe Branch" },
                new BranchItem { Code = "307", Name = "Galkayo Branch" }
            };

        public List<BranchItem> GetBranches(string platform)
        {
            if (string.Equals(
                platform,
                "North",
                StringComparison.OrdinalIgnoreCase))
            {
                return NorthBranches;
            }

            if (string.Equals(
                platform,
                "South",
                StringComparison.OrdinalIgnoreCase))
            {
                return SouthBranches;
            }

            return new List<BranchItem>();
        }

        public string GetBranchName(string branchCode)
        {
            return NorthBranches
                       .Concat(SouthBranches)
                       .Where(x => x.Code == branchCode)
                       .Select(x => x.Name)
                       .FirstOrDefault()
                   ?? branchCode;
        }

        public bool IsValidBranch(
            string platform,
            string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return true;

            return GetBranches(platform)
                .Any(x => x.Code == branchCode);
        }
    }
}