using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class UserSearch
    {
        public UserSearch()
        {
            this.Page = 1;
            this.PageSize = 10;
        }

        [Display(Name = "Search Term")]
        public string SearchTerm { get; set; }


        public List<Users> SearchResults { get; set; }

        public int Page { get; set; }

        [Display(Name = "Page Size")]
        public int PageSize { get; set; }

        public int UsersCount { get; set; }

        public int FirstRow
        {
            get
            {
                return this.Page * this.PageSize - (this.PageSize - 1);
            }
        }

        public int LastRow
        {
            get
            {
                return this.Page * this.PageSize;
            }
        }

        public int PageCount
        {
            get
            {
                return Convert.ToInt32(Math.Ceiling(this.UsersCount / (this.PageSize * 1M)));
            }
        }
    }
}