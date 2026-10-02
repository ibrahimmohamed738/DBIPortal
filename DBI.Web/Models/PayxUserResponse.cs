using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.Models
{
    public class PayxUserResponse
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Phone { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string Language { get; set; }
        public string Category { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string IsBarred { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Nationality { get; set; }
        public string ResidenceCountry { get; set; }
        public string MaritalStatus { get; set; }
        public string EmailId { get; set; }
        public string ProfilePhotoURI { get; set; }
        public string LoginId { get; set; }
        public string BarredType { get; set; }
        public string BarredReason { get; set; }
        public string BarredRemarks { get; set; }
        public string MotherName { get; set; }
        public string InheritorName { get; set; }
        public string InheritorPhone { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public string EmployerName { get; set; }
        public string Occupation { get; set; }
        public string RegistrationDate { get; set; }
    }
}