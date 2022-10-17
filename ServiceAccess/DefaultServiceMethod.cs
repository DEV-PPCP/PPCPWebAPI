﻿using Newtonsoft.Json;
using PPCPWebApiServices.Controllers;
using PPCPWebApiServices.Models.PPCPWebService.DAL;
using PPCPWebApiServices.Models.PPCPWebService.DC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PPCPWebApiServices.ServiceAccess
{
   
    public class DefaultServiceMethod
    {
        DALDefaultService objdal = new DALDefaultService();
        /// <summary>
        /// Get Countries -vinod(30/7/2018)
        /// </summary>
        /// <returns></returns>
      
        public object GetCountries()
        {
            List<Models.PPCPWebService.DC.CountriesLKP> getCountries = objdal.GetCountries();
            return getCountries;
        }
        /// <summary>
        /// Get States-vinod(30/7/2018)
        /// </summary>
        /// <param name="CountryID"></param>
        /// <returns></returns>

        public object GetStates(string CountryID)
        {

            List<Models.PPCPWebService.DC.StatesLKP> getStates = objdal.GetStates(Convert.ToInt32(CountryID));
            return getStates;
        }
        /// <summary>
        /// GetCities-vinod(30/7/2018)
        /// </summary>
        /// <param name="StateID"></param>
        /// <returns></returns>
        public object GetCities(string StateID)
        {


            List<Models.PPCPWebService.DC.CitiesLKP> BindCities = objdal.GetCities(Convert.ToInt32(StateID));
            return BindCities;

        }
        /// <summary>
        /// Validate UserName-vinod(30/7/2018)
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public object ValidateUserName(string UserName)
        {
            if (!string.IsNullOrEmpty(UserName))
            {

                List<Models.PPCPWebService.DC.Result> ValidateUserName = objdal.ValidateUsername(UserName);

                return ValidateUserName;
            }
            return 0;
        }
        /// <summary>
        /// Get Organization details-vinod(31/7/2018)
        /// </summary>
        /// <returns></returns>
        public object GetPPCPOrganizations()
        {

            List<Models.PPCPWebService.DC.PPCPOrganizations> getOrganizations = objdal.GetOrganizations();
            return getOrganizations;
        }

        /// <summary>
        /// Get Speicific Organization details-anusha
        /// </summary>
        /// <returns></returns>
        public object GetPPCPSpecificOrganization(string OrganizationID)
        {

            List<Models.PPCPWebService.DC.PPCPOrganizations> getOrganizations = objdal.GetPPCPSpecificOrganization(Convert.ToInt32(OrganizationID));
            return getOrganizations;
        }


        /// <summary>
        /// Get Organization Providers details-vinod(31/7/2018)
        /// </summary>
        /// <param name="OrganizationID"></param>
        /// <returns></returns>
        public object GetPPCPOrganizationProviders(string OrganizationID)
        {

            List<Models.PPCPWebService.DC.PPCPOrganizationProviders> getPPCPOrganizationProviders = objdal.GetPPCPOrganizationProviders(Convert.ToInt32(OrganizationID));
            return getPPCPOrganizationProviders;
        }

        public object GetPPCPProviders(string OrganizationID)
        {

            List<Models.PPCPWebService.DC.PPCPOrganizationProviders> GetPPCPProviders = objdal.GetPPCPProviders(Convert.ToInt32(OrganizationID));
            return GetPPCPProviders;
        }


        /// <summary>
        /// Get organization plan details-vinod(31/7/2018)
        /// </summary>
        /// <param name="OrganizationID"></param>
        /// <param name="ProviderID"></param>
        /// <param name="PlanID"></param>
        /// <returns></returns>

        public object GetPPCPOrganizationProviderPlans(string OrganizationID, string ProviderID, string PlanID,string MemberAge,string MemberGender,string PlanType)
        {

            List<Models.PPCPWebService.DC.PlansAndPlansMapping> getPPCPOrganizationProvidersplans = objdal.GetPPCPOrganizationProviderPlans(Convert.ToInt32(OrganizationID), Convert.ToInt32(ProviderID), Convert.ToInt32(PlanID), MemberAge, MemberGender,Convert.ToInt32(PlanType));
            if (getPPCPOrganizationProvidersplans.Count >= 1)
            {
                getPPCPOrganizationProvidersplans.ToList().ForEach(u =>
                {
                    u.OrganizationID = getPPCPOrganizationProvidersplans[0].OrgID;
                    u.OrganizationName = getPPCPOrganizationProvidersplans[0].OrgName;
                });

                //getPPCPOrganizationProvidersplans.ToList().ForEach(u => u.OrganizationID = getPPCPOrganizationProvidersplans[0].OrgID  u.OrganizationName= getPPCPOrganizationProvidersplans[0].OrgName);

                // getPPCPOrganizationProvidersplans.Select(c => { c.OrganizationID = getPPCPOrganizationProvidersplans[0].OrgID; return c; } ).ToList();
                // getPPCPOrganizationProvidersplans[0].OrganizationID = getPPCPOrganizationProvidersplans[0].OrgID;
                // getPPCPOrganizationProvidersplans[0].OrganizationName = getPPCPOrganizationProvidersplans[0].OrgName;
            }
            //string json = "{\"PaymentIntervals\":[{\"No.of Installments\":\"6\",\"InstallmentAmount\":\"10\",\"InstallmentFee\":\"1\",\"Savings \":\"1\"}]}";
            List<PaymentIntervals> IntervalsList = new List<PaymentIntervals>();
            if (Convert.ToInt32(PlanID) == 0)
            {
                return getPPCPOrganizationProvidersplans;
            }


            if (getPPCPOrganizationProvidersplans.Count >= 1)
            {
                string json = "{\"PaymentIntervals\":" + getPPCPOrganizationProvidersplans[0].PaymentIntervals + "}";
                var result = JsonConvert.DeserializeObject<PaymentIntervalsList>(json);

                for (int i = 0; i < result.PaymentIntervals.Count; i++)
                {
                    PaymentIntervals intervals = new PaymentIntervals();
                    intervals.PlanID = getPPCPOrganizationProvidersplans[0].PlanID;
                    intervals.PlanName = getPPCPOrganizationProvidersplans[0].PlanName;
                    intervals.MonthlyFee = getPPCPOrganizationProvidersplans[0].MonthlyFee;
                    intervals.IsActive = getPPCPOrganizationProvidersplans[0].IsActive;
                    intervals.CreatedBy = getPPCPOrganizationProvidersplans[0].CreatedBy;
                    intervals.CreatedDate = getPPCPOrganizationProvidersplans[0].CreatedDate;
                    intervals.ModifiedBy = getPPCPOrganizationProvidersplans[0].ModifiedBy;
                    intervals.ModifiedDate = getPPCPOrganizationProvidersplans[0].ModifiedDate;
                    intervals.TotalAmount = result.PaymentIntervals[i].TotalAmount;
                    intervals.Paymentschedule = result.PaymentIntervals[i].Paymentschedule;
                    intervals.NoofInstallments = result.PaymentIntervals[i].NoofInstallments;
                    intervals.InstallmentFee = result.PaymentIntervals[i].InstallmentFee;
                    intervals.InstallmentAmount = result.PaymentIntervals[i].InstallmentAmount;
                    intervals.Savings = result.PaymentIntervals[0].Savings;
                    intervals.ProviderID = getPPCPOrganizationProvidersplans[0].ProviderID;
                    intervals.ProviderName = getPPCPOrganizationProvidersplans[0].ProviderName;
                    intervals.OrganizationID = getPPCPOrganizationProvidersplans[0].OrgID;
                    intervals.OrganizationName = getPPCPOrganizationProvidersplans[0].OrgName;
                    intervals.AccountID = getPPCPOrganizationProvidersplans[0].AccountID;
                    intervals.CommPPCP = getPPCPOrganizationProvidersplans[0].CommPPCP;
                    intervals.CommPrimaryMember = getPPCPOrganizationProvidersplans[0].CommPrimaryMember;

                    IntervalsList.Add(intervals);
                }

            }

            return IntervalsList;
        }


        public object GetPlanPaymentDetails(string OrganizationID, string ProviderID, string PlanID, string MemberCount,string MemberAge,string MemberGernder,string PlanType)
        {

            List<Models.PPCPWebService.DC.PlansAndPlansMapping> getPPCPOrganizationProvidersplans = objdal.GetPPCPOrganizationProviderPlans(Convert.ToInt32(OrganizationID), Convert.ToInt32(ProviderID), Convert.ToInt32(PlanID), MemberAge,MemberGernder,Convert.ToInt32(PlanType));
            //string json = "{\"PaymentIntervals\":[{\"No.of Installments\":\"6\",\"InstallmentAmount\":\"10\",\"InstallmentFee\":\"1\",\"Savings \":\"1\"}]}";
            List<PaymentIntervals> IntervalsList = new List<PaymentIntervals>();
            if (Convert.ToInt32(PlanID) == 0)
            {
                return getPPCPOrganizationProvidersplans;
            }


            if (getPPCPOrganizationProvidersplans.Count >= 1)
            {
                string json = "{\"PaymentIntervals\":" + getPPCPOrganizationProvidersplans[0].PaymentIntervals + "}";
                var result = JsonConvert.DeserializeObject<PaymentIntervalsList>(json);

                for (int i = 0; i < result.PaymentIntervals.Count; i++)
                {
                    PaymentIntervals intervals = new PaymentIntervals();
                    intervals.PlanID = getPPCPOrganizationProvidersplans[0].PlanID;
                    intervals.PlanName = getPPCPOrganizationProvidersplans[0].PlanName;
                    intervals.MonthlyFee = getPPCPOrganizationProvidersplans[0].MonthlyFee;
                    intervals.IsActive = getPPCPOrganizationProvidersplans[0].IsActive;
                    intervals.CreatedBy = getPPCPOrganizationProvidersplans[0].CreatedBy;
                    intervals.CreatedDate = getPPCPOrganizationProvidersplans[0].CreatedDate;
                    intervals.ModifiedBy = getPPCPOrganizationProvidersplans[0].ModifiedBy;
                    intervals.ModifiedDate = getPPCPOrganizationProvidersplans[0].ModifiedDate;
                    intervals.CommAdditionalMember= getPPCPOrganizationProvidersplans[0].CommAdditionalMember;
                    if(PlanType=="2" && intervals.CommAdditionalMember >0)
                    {
                        if (result.PaymentIntervals[i].TotalAmount > 0)
                        {

                          decimal? dd=  result.PaymentIntervals[i].TotalAmount * Convert.ToInt32(MemberCount) * (intervals.CommAdditionalMember / 100);
                            intervals.TotalAmount = (result.PaymentIntervals[i].TotalAmount * Convert.ToInt32(MemberCount)) - dd;

                        }
                        else
                        {
                            intervals.TotalAmount = result.PaymentIntervals[i].TotalAmount;
                        }

                        decimal? ins = (result.PaymentIntervals[i].InstallmentAmount * Convert.ToInt32(MemberCount)) * (intervals.CommAdditionalMember / 100);
                        intervals.InstallmentAmount = (result.PaymentIntervals[i].InstallmentAmount * Convert.ToInt32(MemberCount))-ins;


                    }
                    else
                    {
                        if (result.PaymentIntervals[i].TotalAmount > 0)
                        {
                            intervals.TotalAmount = result.PaymentIntervals[i].TotalAmount * Convert.ToInt32(MemberCount);
                        }
                        else
                        {
                            intervals.TotalAmount = result.PaymentIntervals[i].TotalAmount;
                        }
                        intervals.InstallmentAmount = result.PaymentIntervals[i].InstallmentAmount;
                    }
                  
                    //if (result.PaymentIntervals[i].InstallmentAmount > 0)
                    //{
                    //    intervals.InstallmentAmount = result.PaymentIntervals[i].InstallmentAmount * Convert.ToInt32(MemberCount);
                    //}
                    //else
                    //{
                    //    intervals.InstallmentAmount = result.PaymentIntervals[i].InstallmentAmount;
                    //}
                    if (result.PaymentIntervals[i].Savings > 0)
                    {
                        intervals.Savings = result.PaymentIntervals[i].Savings * Convert.ToInt32(MemberCount);
                    }
                    else
                    {
                        intervals.Savings = result.PaymentIntervals[i].Savings;
                    }
                    if (result.PaymentIntervals[i].InstallmentFee > 0)
                    {
                        intervals.InstallmentFee = result.PaymentIntervals[i].InstallmentFee * Convert.ToInt32(MemberCount);
                    }
                    else
                    {
                        intervals.InstallmentFee = result.PaymentIntervals[i].InstallmentFee;
                    }
                    // intervals.TotalAmount = result.PaymentIntervals[i].TotalAmount;
                    intervals.Paymentschedule = result.PaymentIntervals[i].Paymentschedule;
                    intervals.NoofInstallments = result.PaymentIntervals[i].NoofInstallments;

                    intervals.ProviderID = getPPCPOrganizationProvidersplans[0].ProviderID;
                    intervals.ProviderName = getPPCPOrganizationProvidersplans[0].ProviderName;
                    intervals.OrganizationID = getPPCPOrganizationProvidersplans[0].OrganizationID;
                    intervals.OrganizationName = getPPCPOrganizationProvidersplans[0].OrganizationName;
                    intervals.AccountID = getPPCPOrganizationProvidersplans[0].AccountID;
                    IntervalsList.Add(intervals);
                }

            }

            return IntervalsList;
        }

        // Member Plans & Payments Details (Report)
        public object GetMemberPlansAndPaymentDetails(string MemberID, string PlanID, string OrganizationID)
        {

            List<Models.PPCPWebService.DC.MemberPlans> lstMemberDetails = objdal.GetMemberPlansAndPaymentDetails(Convert.ToInt32(PlanID), Convert.ToInt32(MemberID), Convert.ToInt32(OrganizationID));
            return lstMemberDetails;
        }
        /// <summary>
        /// Get Plane Details based on planID-vinod(31/7/2018)
        /// </summary>
        /// <param name="PlanID"></param>
        /// <returns></returns>
        public object GetPlanDetails(string PlanID, string MemberID, string OrganizationID, string ProviderID)
        {

            List<Models.PPCPWebService.DC.MemberPlans> getPPCPOrganizationProvidersplans = objdal.GetPlanDetails(Convert.ToInt32(PlanID), Convert.ToInt32(MemberID), Convert.ToInt32(OrganizationID), Convert.ToInt32(ProviderID));
            return getPPCPOrganizationProvidersplans;
        }

        public object ValidateForgotPassword(string UserName, string Type)
        {

            List<Models.PPCPWebService.DC.Credentials> validateForgotPassword = objdal.ValidateForgotPassword(UserName, Convert.ToInt32(Type));
            return validateForgotPassword;
        }

        public object ValidateForgotUserName(string FirstName, string LastName, string CountryCode, string MobileNumber, string Email, string Type)
        {

            List<Models.PPCPWebService.DC.Credentials> validateForgotUserName = objdal.ValidateForgotUserName(FirstName, LastName, CountryCode, MobileNumber, Email, Convert.ToInt32(1));
            return validateForgotUserName;
        }

        public object SendCredentials(string FirstName, string LastName, string CountryCode, string MobileNumber, string Email, string UserName, string Password, string TempID)
        {

            int result = objdal.SendCredentials(FirstName, LastName, CountryCode, MobileNumber, Email, UserName, Password, TempID);
            return result;
        }

        public object validateChangePassword(string UserID, string Password, string TypeID)
        {

            List<Result> changePassword = objdal.validateChangePassword(Convert.ToInt32(UserID), Password, Convert.ToInt32(TypeID));
            return changePassword;
        }
        public object GetPaymentDetails(string MemberID)
        {

            List<Models.PPCPWebService.DC.PaymentDetails> getpaymentdetails = objdal.GetPaymentDetails(Convert.ToInt32(MemberID));
            return getpaymentdetails;
        }

        /// <summary>
        /// Get the Relationship details for bind the dropdown in member profile
        /// </summary>
        /// <returns></returns>
        public object GetRelationShipDetails()
        {

            List<RelationShip> getRelationShipDetails = objdal.GetRelationShipDetails();
            return getRelationShipDetails;
        }

        /// <summary>
        /// Get plans details from Plans table
        /// </summary>
        /// <returns></returns>
        public object GetPlans(string Type)
        {
            List<Plan> getPlans = new List<Plan>();
            getPlans = objdal.GetPlans(Type);
            return getPlans;
        }

        /// <summary>
        /// Add plans to the Plans tables-vinod
        /// </summary>
        /// <param name="XML"></param>
        /// <returns></returns>
        public object AddPlans(string XML)
        {
            List<Result> obj = objdal.AddPlans(XML);
            return obj;
        }
        /// <summary>
        /// Validate plan name form PlanName table-vinod
        /// </summary>
        /// <param name="strPlanName"></param>
        /// <returns></returns>
        public object ValidatePlanName(string strPlanName)
        {
            
            int i = objdal.ValidatePlanName(strPlanName);
            return i;
        }
      
        public object SavePlanMapping(string OrganizationName, string OrganizationID, string ProviderName, string ProviderID, string PlanName, string PlanID)
        {
            List<Result> result = new List<Result>();
            if (!string.IsNullOrEmpty(OrganizationID) && !string.IsNullOrEmpty(ProviderID) && !string.IsNullOrEmpty(PlanID))
            {

                result = objdal.SavePlanMapping(OrganizationName, Convert.ToInt32(OrganizationID), ProviderName, Convert.ToInt32(ProviderID), PlanName, Convert.ToInt32(PlanID));
              
            }
            return result;
        }
        public object WithdrawPlans(string PlanID,string UnsubscribeDate)
        {
            List<Result> obj = objdal.WithdrawPlans(Convert.ToInt32(PlanID),Convert.ToDateTime(UnsubscribeDate));
            return obj;

        }
        public object GetOrganizationPaymentDetails(string OrganizationID, string Todate, string FromDate, string ProviderId, string PlanId)
        {
            DALOrganizationService objdal = new DALOrganizationService();

            List<MemberPlan> GetpaymentDetails = new List<MemberPlan>();

          //  GetpaymentDetails = objdal.GetOrganizationPaymentDetails(Convert.ToInt32(OrganizationID), Convert.ToDateTime(Todate), Convert.ToDateTime(FromDate), Convert.ToInt32(ProviderId), Convert.ToInt32(PlanId));

            return GetpaymentDetails;

        }
        public object GetAdminPaymentDetails(string strToDate, string strFromDate,string strOrganizationID,string strProviderID,string strPlanID)
        {

            List<MemberPlan> GetpaymentDetails = new List<MemberPlan>();         
            GetpaymentDetails = objdal.GetAdminPaymentDetails(Convert.ToDateTime(strToDate), Convert.ToDateTime(strFromDate),Convert.ToInt32(strOrganizationID), Convert.ToInt32(strProviderID), Convert.ToInt32(strPlanID));

            return GetpaymentDetails;

        }

        public object GetOrganizationAutoComplete(string Text)
        {
            object OrganizationDetails = objdal.GetOrganizationAutoComplete(Text);
            return OrganizationDetails;
        }

        public object GetMemberPlansDetails(string strFromDate,string strToDate,string MemberID)
        {
            object MemberPlanDetails = objdal.GetMemberPlansDetails(Convert.ToDateTime(strFromDate), Convert.ToDateTime(strToDate),Convert.ToInt32(MemberID));

            return MemberPlanDetails;
        }

        public object GetOrganizationPlanDetails(string strFromDate, string strToDate, string OrganizationID)
        {
            object OrganizationPlanDetails = objdal.GetOrganizationPlanDetails(Convert.ToDateTime(strFromDate), Convert.ToDateTime(strToDate), Convert.ToInt32(OrganizationID));

            return OrganizationPlanDetails;

        }

        public object GetProviderPlanDetails(string strFromDate, string strToDate, string ProviderID)
        {
            object ProviderPlanDetails = objdal.GetProviderPlanDetails(Convert.ToDateTime(strFromDate), Convert.ToDateTime(strToDate), Convert.ToInt32(ProviderID));

            return ProviderPlanDetails;

        }
        /// <summary>
        /// Insert TermsAndConditions for Member,Organization and Provider.in type=1(Member),2(Organization),3(User)-vinod
        /// </summary>
        /// <param name="TermsandConditionName"></param>
        /// <param name="TempletPath"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public object InsertTermsAndConditions(string TermsandConditionName,string TempletPath, string Type)
        {
           List<Result> objresult= objdal.InsertTermsAndConditions(TermsandConditionName, TempletPath, Convert.ToInt32(Type));
            return objresult;
        }
        /// <summary>
        /// Get TermsAndConditions for Member,OrganizationUser in type=1(Member),2(Organization),3(User)-vinod
        /// </summary>
        /// <param name="strType"></param>
        /// <returns></returns>

        public object GetTermsAndConditions(string strType)
        {
            List<TermsAndCondition> objTermsAndCondition = objdal.GetTermsAndConditions(Convert.ToInt32(strType));
            return objTermsAndCondition;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="OrganizationID"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public object GetMembersAutoComplete(string OrganizationID, string Text)
        {
            object memberDetails = objdal.GetMembersAutoComplete(Convert.ToInt32(OrganizationID), Text);
            return memberDetails;
        }
        /// <summary>
        /// Search Provider Details in Organizaton Module -- By Ragini 
        /// </summary>
        /// <param name="OrganizationID"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public object GetProviderDetailsAutoComplete(string OrganizationID, string Text)
        {
            object providerDetails = objdal.GetProviderDetailAutoComplete(Convert.ToInt32(OrganizationID), Text);
            return providerDetails;
        }
        public object GetDisabledOrganizationDetails(string strOrganizationID)
        {
            List<Organization> getDisabledOrganizationDetails = new List<Organization>();
             getDisabledOrganizationDetails = objdal.GetDisabledOrganizationDetails(Convert.ToInt32(strOrganizationID));
            return getDisabledOrganizationDetails;
        }
        /// <summary>
        /// Get Organization User auto Complete-vinod
        /// </summary>
        /// <param name="OrganizationID"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public object GetUserDetailsAutoComplete(string OrganizationID, string Text)
        {
            object providerDetails = objdal.GetUserDetailsAutoComplete(Convert.ToInt32(OrganizationID), Text);
            return providerDetails;
        }

    }
}