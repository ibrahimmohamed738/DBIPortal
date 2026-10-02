using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.Models
{
    public class PayxTransactionDetailsResponse
    {
        public string Status { get; set; }
        public string ServiceFlow { get; set; }
        public string TransactionId { get; set; }
        public string TransactionDate { get; set; }
        public string UserProfileId { get; set; }
        public string MerchantFullName { get; set; }
        public string TransactionStatus { get; set; }
        public string TransferValue { get; set; }
        public string RequestedValue { get; set; }
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
        public string InitiatorMsisdn { get; set; }
        public string InitiatorName { get; set; }
        public string EntryType { get; set; }
        public string ServiceRequestId { get; set; }
        public string Narration { get; set; }

        public TransactionParty Sender { get; set; }
        public TransactionParty Receiver { get; set; }
    }

    public class TransactionParty
    {
        public string Currency { get; set; }
        public string UserProfileId { get; set; }
        public string UserName { get; set; }
        public string AccountType { get; set; }
        public int ProductId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string IdentifierType { get; set; }
        public string IdentifierValue { get; set; }
        public string PreBalance { get; set; }
        public string PostBalance { get; set; }
        public string NetTransferValue { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }
    }
}