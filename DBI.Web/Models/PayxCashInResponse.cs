using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.Models
{
    public class PayxCashInResponse
    {
        public string ExternalReferenceId { get; set; }
        public string TxnStatus { get; set; }
        public string Code { get; set; }
        public string ServiceRequestId { get; set; }
        public string MfsTenantId { get; set; }
        public string Language { get; set; }
        public string ServiceFlow { get; set; }
        public string TransactionTimeStamp { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
        public string OriginalServiceRequestId { get; set; }
        public string Status { get; set; }
    }
}