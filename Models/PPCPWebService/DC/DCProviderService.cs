﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PPCPWebApiServices.Models.PPCPWebService.DC
{
    public class DCProviderService
    {
        public class ProviderDetails
        {
            public int ProviderID { get; set; }
            public Nullable<int> OrganizationID { get; set; }
            public string OrganizationName { get; set; }
            public string NPI { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Salutation { get; set; }
            public Nullable<System.DateTime> DOB { get; set; }
            public string Gender { get; set; }
            public string Email { get; set; }
            public string CountryCode { get; set; }
            public string MobileNumber { get; set; }
            public Nullable<int> CountryID { get; set; }
            public string CountryName { get; set; }
            public Nullable<int> StateID { get; set; }
            public string StateName { get; set; }
            public Nullable<int> CityID { get; set; }
            public string CityName { get; set; }
            public string Address { get; set; }
            public string Zip { get; set; }
            public Nullable<bool> IsActive { get; set; }
            public Nullable<int> CreatedBy { get; set; }
            public Nullable<System.DateTime> CreatedDate { get; set; }
            public Nullable<int> ModifiedBy { get; set; }
            public Nullable<System.DateTime> ModifiedDate { get; set; }
            public Nullable<bool> IsTwofactorAuthentication { get; set; }
            public string PreferredIP { get; set; }
            public Nullable<int> TwoFactorType { get; set; }
            public Nullable<bool> IS_TERMS_ACCECPTED { get; set; }
            public string Tax_No { get; set; }
            public Nullable<int> No_of_Patients_Accepting { get; set; }
            public string Specialization { get; set; }
            public string SpecializationName { get; set; }
            public string SpecializationID { get; set; }

            public string OTP { get; set; }
            public int MemberID { get; set; }
            public string ProviderName { get; set; }

            public string Fax { get; set; }

            public string Degree { get; set; }



            public string OrgEmail { get; set; }
            public string OrgPhone { get; set; }
        }
        
    }
}