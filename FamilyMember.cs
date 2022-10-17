//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PPCPWebApiServices
{
    using System;
    using System.Collections.Generic;
    
    public partial class FamilyMember
    {
        public int FamilyMemberID { get; set; }
        public int ParentID { get; set; }
        public Nullable<int> RelationshipID { get; set; }
        public string RelationshipName { get; set; }
        public string MemberCode { get; set; }
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
        public Nullable<int> OrganizationID { get; set; }
        public string Organizationname { get; set; }
        public Nullable<int> OrganizationType { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsTwofactorAuthentication { get; set; }
        public string PreferredIP { get; set; }
        public Nullable<int> TwoFactorType { get; set; }
    
        public virtual Member Member { get; set; }
    }
}