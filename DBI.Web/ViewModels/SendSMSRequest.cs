using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class SendSMSRequest
    {
        public string Entity { get; set; }
        public string Mobile { get; set; }
        public string Message { get; set; }
    }
}