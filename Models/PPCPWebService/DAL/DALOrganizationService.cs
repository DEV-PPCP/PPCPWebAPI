﻿using Newtonsoft.Json;
using PPCPWebApiServices.Models.PPCPWebService.DC;
using PPCPWebApiServices.Models.Service;
using PPCPWebApiServices.ServiceAccess;
using Stripe;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using static PPCPWebApiServices.Models.PPCPWebService.DC.DCProviderService;

namespace PPCPWebApiServices.Models.PPCPWebService.DAL
{
    public class DALOrganizationService
    {
        public List<OrganizationDetails> SaveOrganizationDetails(string xml)
        {
            List<OrganizationDetails> objTemporaryDetails = new List<OrganizationDetails>();
            StripeMethods stripeMethods = new StripeMethods();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(OrganizationDetails));
                StringReader rdr = new StringReader(xml);
                OrganizationDetails objDCOrganizationService = (OrganizationDetails)serializer.Deserialize(rdr);
                string AccountId = "";
                string stripeApiKey = System.Configuration.ConfigurationManager.AppSettings["StripeApiKey"];
                StripeConfiguration.SetApiKey(stripeApiKey);
                Account account = stripeMethods.CreateStripeCustomer(objDCOrganizationService.AccountHolderName,
                                                                                objDCOrganizationService.AccountNumber,
                                                                                 objDCOrganizationService.RoutingNumber,
                                                                                 objDCOrganizationService.MobileNumber,
                                                                                 objDCOrganizationService.FirstName,
                                                                                 objDCOrganizationService.LastName,
                                                                                 objDCOrganizationService.Email,
                                                                                 objDCOrganizationService.OrgZip,
                                                                                objDCOrganizationService.OrgAddress,
                                                                                 "",
                                                                                objDCOrganizationService.OrgStateName,
                                                                                objDCOrganizationService.OrgCityName,
                                                                                  objDCOrganizationService.DOB
                                                                                  ,objDCOrganizationService.TaxID, objDCOrganizationService.UserSSN,
                                                                                  objDCOrganizationService.OrganizationName,
                                                                                  objDCOrganizationService.OrgMobileNumber,
                                                                                  objDCOrganizationService.OrgEmail
                                                                                  );//objDCOrganizationService






                //   var bankAccountOptions = new StripeAccountBankAccountOptions();
                //   bankAccountOptions.AccountHolderName = objDCOrganizationService.AccountHolderName;
                //   bankAccountOptions.AccountNumber = objDCOrganizationService.AccountNumber;
                //   bankAccountOptions.RoutingNumber = objDCOrganizationService.RoutingNumber;
                //   bankAccountOptions.Country = "US";
                //   bankAccountOptions.Currency = "USD";
                //   bankAccountOptions.AccountHolderType = "company";
                //   var accountLegalEntityOptions = new StripeAccountLegalEntityOptions();
                //   //accountLegalEntityOptions.SSNLast4 = xn["SSN"].InnerText;
                //   accountLegalEntityOptions.PhoneNumber = objDCOrganizationService.OrgMobileNumber;
                //   accountLegalEntityOptions.BusinessTaxId = objDCOrganizationService.TaxID;
                //   accountLegalEntityOptions.PersonalAddressLine1 = objDCOrganizationService.Address;
                //   accountLegalEntityOptions.PersonalAddressLine2 = "";
                //   accountLegalEntityOptions.PersonalAddressPostalCode = objDCOrganizationService.Zip;
                //   accountLegalEntityOptions.Type = "company";
                //   accountLegalEntityOptions.AddressLine1 = objDCOrganizationService.OrgAddress;
                //   accountLegalEntityOptions.AddressLine2 = "";
                ////   accountLegalEntityOptions.AddressCity = Convert.ToString(objDCOrganizationService.OrgCityID);
                ////   accountLegalEntityOptions.AddressState = Convert.ToString(objDCOrganizationService.OrgStateID);
                //   accountLegalEntityOptions.AddressPostalCode = Convert.ToString(objDCOrganizationService.OrgZip);
                //   accountLegalEntityOptions.BusinessName = objDCOrganizationService.OrganizationName;
                //   //accountLegalEntityOptions.FirstName = xn["FirstName"].InnerText;
                //   //accountLegalEntityOptions.LastName = xn["LastName"].InnerText;
                //   //DateTime date = Convert.ToDateTime(xn["DateOfBirth"].InnerText);
                //   //accountLegalEntityOptions.BirthDay = date.Day;
                //   //accountLegalEntityOptions.BirthMonth = date.Month;
                //   //accountLegalEntityOptions.BirthYear = date.Year;
                //   // DateTime dd = DateTime.Now;
                //   var accountOptions = new StripeAccountCreateOptions()
                //   {
                //       Email = objDCOrganizationService.OrgEmail,
                //       Type = StripeAccountType.Custom,
                //       Country = "US",
                //       TosAcceptanceDate = DateTime.Now,
                //       TosAcceptanceIp = Iph.Utilities.Common.GetUserIp(),
                //       ExternalBankAccount = bankAccountOptions,
                //       LegalEntity = accountLegalEntityOptions
                //   };
                //var accountService = new StripeAccountService();
                //StripeAccount account = accountService.Create(accountOptions);
                AccountId = account.Id;
                string UserPassword = objDCOrganizationService.Password;
                byte[] bytes = Encoding.UTF8.GetBytes(UserPassword);
                string Encryptpassword = Convert.ToBase64String(bytes);
                using (var context = new DALMemberService())
                {
                    if (xml != "")
                    {
                        SqlParameter XML = new SqlParameter("@XML", xml);
                        SqlParameter EncryptPassword = new SqlParameter("@EncryptPassword", Encryptpassword);
                        SqlParameter AccountID = new SqlParameter("@AccountID", AccountId);
                        objTemporaryDetails = context.Database.SqlQuery<OrganizationDetails>("Pr_InsertOrganizationDetails @XML,@EncryptPassword,@AccountID", XML, EncryptPassword, AccountID).ToList();
                        //if (objTemporaryDetails.Count > 0)
                        //{

                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                OrganizationDetails obj = new OrganizationDetails();
                obj.Result = ex.Message;
                objTemporaryDetails.Add(obj);

            }
            return objTemporaryDetails;
        }


        public List<OrganizationUsers> ValidateOrganization(string Username, string Password, string IpAddress)
        {
            List<OrganizationUsers> ordDetails = new List<OrganizationUsers>();
            try
            {
                using (var context = new DALMemberService())
                {
                    SqlParameter UserName = new SqlParameter("@UserName", Username);
                    SqlParameter PassWord = new SqlParameter("@Password", Password);
                    ordDetails = context.Database.SqlQuery<OrganizationUsers>("Pr_ValidateOrganizationCredentials @UserName, @Password", UserName, PassWord).ToList();

                    if (ordDetails.Count() > 0)
                    {
                        if (ordDetails[0].IsActive != true)
                        {
                            using (var Context = new Dev_PPCPEntities(1))
                            {
                                int UserID = Convert.ToInt32(ordDetails[0].UserID);
                                UserCredential h = Context.UserCredentials.First(m => m.UserID == UserID);
                                h.UserStatus = "A";
                                Context.SaveChanges();

                                int OrganizationID = Convert.ToInt32(ordDetails[0].OrganizationID);
                                Organization Organization = Context.Organizations.First(m => m.OrganizationID == OrganizationID);
                                Organization.IsActive = true;
                                Context.SaveChanges();

                                OrganizationUser OrganizationUser = Context.OrganizationUsers.First(m => m.OrganizationID == OrganizationID);
                                OrganizationUser.IsActive = true;
                                Context.SaveChanges();
                            }

                        }


                        //List<MemberLoginDetails> list = new List<MemberLoginDetails>();
                        //SqlParameter OrganizationID = new SqlParameter("@OrganizationID", listMemberDetails[0].OrganizationID);
                        //SqlParameter UserID = new SqlParameter("@UserID", listMemberDetails[0].UserID);
                        //list = context.Database.SqlQuery<MemberLoginDetails>("Pr_UpdateOrganizationUsers @OrganizationID,@UserID", OrganizationID, UserID).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ordDetails;
        }
        public List<TemporaryUserDetails> AddUserDetails(string xml)
        {
            string MobileNumber = "", CountryCode = "";
            List<TemporaryUserDetails> objTemporaryDetails = new List<TemporaryUserDetails>();
            try
            {
                using (var context = new DALMemberService())
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);
                    XmlNode node = xmlDoc.SelectSingleNode("/OrganizationDetails ");
                    string UserPassword = node["Password"].InnerText;
                    string UserName = node["UserName"].InnerText;
                    MobileNumber = node["MobileNumber"].InnerText;
                    CountryCode = node["CountryCode"].InnerText;
                    byte[] bytes = Encoding.UTF8.GetBytes(UserPassword);
                    string Encryptpassword = Convert.ToBase64String(bytes);
                    SqlParameter XML = new SqlParameter("@XML", xml);
                    SqlParameter EncryptPassword = new SqlParameter("@EncryptPassword", Encryptpassword);
                    objTemporaryDetails = context.Database.SqlQuery<TemporaryUserDetails>("Pr_AddUserDetails @XML,@EncryptPassword", XML, EncryptPassword).ToList();
                    if (objTemporaryDetails.Count > 0)
                    {
                        string message = "Thank you for enrolling with PPCP. Your UserName : " + UserName + " And Password : " + UserPassword;
                        SendMessageByText(message, MobileNumber, CountryCode);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objTemporaryDetails;
        }
        private void SendMessageByText(string message, string MobileNo, string CountryCode)
        {
            List<string> MessageInfo = new List<string>();
            try
            {
                List<Application_Parameter_Config> getApplication_Parameter_Config = GetApplicationParameterConfig();

                string PhoneNumber = "";
                string AccountSid = getApplication_Parameter_Config[0].PARAMETER_VALUE.Trim(); //"ACee0fa31137b3bb48086716289755b6ce"; //getApplication_Parameter_Config[0].PARAMETER_VALUE;// "ACee0fa31137b3bb48086716289755b6ce";
                string AuthToken = getApplication_Parameter_Config[1].PARAMETER_VALUE.Trim(); //"b2647dabc45ab06f9157d696667ee9bb"; //list[1].PARAMETER_VALUE;//"b2647dabc45ab06f9157d696667ee9bb";
                string FromPhone = getApplication_Parameter_Config[2].PARAMETER_VALUE.Trim(); //"+17322534561";//list[2].PARAMETER_VALUE;//"+17322534561";               
                string MessagingServiceSid = getApplication_Parameter_Config[8].PARAMETER_VALUE.Trim();
                if (string.IsNullOrEmpty(CountryCode))
                {
                    CountryCode = getApplication_Parameter_Config[3].PARAMETER_VALUE;// list[3].PARAMETER_VALUE;
                }
                if (MobileNo != "")
                {
                    PhoneNumber = MobileNo;

                }

                string mobile = "+" + CountryCode + PhoneNumber;

                string K = "";

                MessageInfo = SMS.SendMessage(FromPhone, AccountSid, AuthToken, mobile, message, MessagingServiceSid);






                //if (K.Substring(0, 1) == "1")
                //{
                //    // Result = "1";//if want show popupsuecessfully messaage based text sucess we can these things
                //}
                //else
                //{
                //    // Result = "0";//if want show popupsuecessfully messaage based text sucess we can these things
                //}
            }
            catch (Exception ex)
            {

            }
        }
        public List<Application_Parameter_Config> GetApplicationParameterConfig()
        {
            List<Application_Parameter_Config> getApplicationParameterConfig = new List<Application_Parameter_Config>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {

                    getApplicationParameterConfig = Context.Application_Parameter_Config.ToList();

                }
            }
            catch (Exception ex)
            {

            }
            return getApplicationParameterConfig;

        }
        public List<Result> ValidateUsername(string Username)
        {
            List<Result> IsActive;
            using (var context = new DALDefaultService())
            {
                SqlParameter UserName = new SqlParameter("@UserName", Username);
                IsActive = context.Database.SqlQuery<Result>("Pr_ValidateUserName @UserName", UserName).ToList();
            }
            return IsActive;
        }

        public List<OrganizationDetails> GetOrganizationUsersProfile(int OrganizationId)
        {
            List<OrganizationDetails> getOrganizationUsersDetails = new List<OrganizationDetails>();
            try
            {
                using (var context = new DALDefaultService())
                {
                    SqlParameter OrganizationID = new SqlParameter("@OrganizationID", OrganizationId);
                    getOrganizationUsersDetails = context.Database.SqlQuery<OrganizationDetails>("Pr_GetOrganizationUsers @OrganizationID", OrganizationID).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return getOrganizationUsersDetails;
        }

        public List<ValidateOrgForgotCredentials> ValidateOrgForgotCredentials(string Username, string Firstname, string Lastname, string Mobilenumber, string Countrycode, string email, string type)
        {
            List<ValidateOrgForgotCredentials> getOrganizationUsersDetails = new List<ValidateOrgForgotCredentials>();
            try
            {

                using (var context = new DALDefaultService())
                {
                    SqlParameter UserName = new SqlParameter("@UserName", Username);
                    SqlParameter FirstName = new SqlParameter("@FirstName", Firstname);
                    SqlParameter LastName = new SqlParameter("@LastName", Lastname);
                    SqlParameter MobileNumber = new SqlParameter("@MobileNumber", Mobilenumber);
                    SqlParameter CountryCode = new SqlParameter("@CountryCode", Countrycode);
                    SqlParameter Email = new SqlParameter("@Email", email);
                    SqlParameter TempType = new SqlParameter("@TempType", type);
                    getOrganizationUsersDetails = context.Database.SqlQuery<ValidateOrgForgotCredentials>("Pr_ValidateOrgForgotCredentials @UserName,@FirstName,@LastName,@MobileNumber,@CountryCode,@Email,@TempType ", UserName, FirstName, LastName, MobileNumber, CountryCode, Email, TempType).ToList();
                }
                if (getOrganizationUsersDetails.Count > 0)
                {
                    DALDefaultService dal = new DALDefaultService();
                    string OTP = dal.randamNumber();
                    string Message = "Dear " + getOrganizationUsersDetails[0].FirstName + " " + getOrganizationUsersDetails[0].LastName + ", Your one time password is : " + OTP;
                    dal.SendMessageByText(Message, getOrganizationUsersDetails[0].MobileNumber, getOrganizationUsersDetails[0].CountryCode);
                    getOrganizationUsersDetails[0].Otp = OTP;
                }


            }
            catch (Exception ex)
            {

            }
            return getOrganizationUsersDetails;
        }

        public int UpdateOrgCredentials(int userID, string Password)
        {
            int result = 0;
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(Password);
                Password = Convert.ToBase64String(bytes);//Convert the password to Encrypt
                PPCPWebApiServices.UserCredential obj = new PPCPWebApiServices.UserCredential();
                using (var Context = new Dev_PPCPEntities(1))
                {
                    UserCredential h = Context.UserCredentials.First(m => m.UserID == userID);
                    h.Userpassword = Password;
                    result = Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }


        /// <summary>
        /// Update Organization ,Organization User  and UserCredential Details-vinod
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public List<OrganizationDetails> UpdateOrganizationDetails(string xml)
        {
            List<OrganizationDetails> objTemporaryDetails = new List<OrganizationDetails>();

            using (var context = new DALMemberService())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                XmlNode node = xmlDoc.SelectSingleNode("/OrganizationDetails ");
                string UserPassword = node["Password"].InnerText;
                byte[] bytes = Encoding.UTF8.GetBytes(UserPassword);
                string Encryptpassword = Convert.ToBase64String(bytes);
                try
                {
                    if (xml != "")
                    {
                        SqlParameter XML = new SqlParameter("@XML", xml);
                        SqlParameter EncryptPassword = new SqlParameter("@EncryptPassword", Encryptpassword);
                        objTemporaryDetails = context.Database.SqlQuery<OrganizationDetails>("Pr_UpdateOrganizationUsers @XML,@EncryptPassword", XML, EncryptPassword).ToList();

                    }
                }
                catch (Exception ex)
                {
                    OrganizationDetails obj = new OrganizationDetails();
                    obj.Result = ex.Message;
                    objTemporaryDetails.Add(obj);
                }
            }
            return objTemporaryDetails;
        }


        public List<SpecializationLKP> GetSpecializationLKP()
        {

            List<SpecializationLKP> getSpecializationLKP = new List<SpecializationLKP>();
           
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                
                    getSpecializationLKP = Context.SpecializationLKPs.Where(m => m.IsDelete == true).ToList();

                }
            }
            catch (Exception ex)
            {

            }
            return getSpecializationLKP;
        }


        public List<TemporaryUserDetails> AddDoctorDetails(string xml)
        {
            string MobileNumber = "", CountryCode = "";
            List<TemporaryUserDetails> objTemporaryDetails = new List<TemporaryUserDetails>();
            try
            {
                using (var context = new DALMemberService())
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);
                    XmlNode node = xmlDoc.SelectSingleNode("/AddDoctor");
                    string UserPassword = node["Password"].InnerText;
                    string UserName = node["UserName"].InnerText;
                    MobileNumber = node["MobileNumber"].InnerText;
                    CountryCode = node["CountryCode"].InnerText;
                    byte[] bytes = Encoding.UTF8.GetBytes(UserPassword);
                    string Encryptpassword = Convert.ToBase64String(bytes);
                    SqlParameter XML = new SqlParameter("@XML", xml);
                    SqlParameter EncryptPassword = new SqlParameter("@EncryptPassword", Encryptpassword);
                    objTemporaryDetails = context.Database.SqlQuery<TemporaryUserDetails>("Pr_AddDoctors @XML,@EncryptPassword", XML, EncryptPassword).ToList();
                    if (objTemporaryDetails.Count > 0)
                    {
                        string message = "Thank you for enrolling with PPCP. Your UserName : " + UserName + " and Password : " + UserPassword;
                        SendMessageByText(message, MobileNumber, CountryCode);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objTemporaryDetails;
        }


        public List<TemporaryUserDetails> AddMemberDetails(string xml)
        {
            string MobileNumber = "", CountryCode = "";
            List<TemporaryUserDetails> objTemporaryDetails = new List<TemporaryUserDetails>();
            try
            {
                using (var context = new DALMemberService())
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);
                    XmlNode node = xmlDoc.SelectSingleNode("/AddMemberDetails");
                    string UserPassword = node["Password"].InnerText;
                    string UserName = node["UserName"].InnerText;
                    MobileNumber = node["MobileNumber"].InnerText;
                    CountryCode = node["CountryCode"].InnerText;
                    byte[] bytes = Encoding.UTF8.GetBytes(UserPassword);
                    string Encryptpassword = Convert.ToBase64String(bytes);
                    SqlParameter XML = new SqlParameter("@XML", xml);
                    SqlParameter EncryptPassword = new SqlParameter("@EncryptPassword", Encryptpassword);
                    objTemporaryDetails = context.Database.SqlQuery<TemporaryUserDetails>("Pr_AddMember @XML,@EncryptPassword", XML, EncryptPassword).ToList();
                    if (objTemporaryDetails.Count > 0)
                    {
                        string message = "Thank you for enrolling with PPCP. Your UserName : " + UserName + " And Password : " + UserPassword;
                        SendMessageByText(message, MobileNumber, CountryCode);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objTemporaryDetails;
        }
        /// <summary>
        /// GetOrganizationMemberDetails
        /// </summary>
        /// <param name="OrganizationID"></param>
        /// <returns></returns>
        public List<Member> GetOrganizationMemberDetails(int OrganizationID,int MemberID)
        {
            List<Member> getMemberDetails = new List<Member>();

            using (var Context = new Dev_PPCPEntities(1))
            {
                if(MemberID==0 && OrganizationID!=0)
                {
                    getMemberDetails = Context.Members.Where(m => m.ID == OrganizationID && m.Type == 2).ToList();
                }
                else if (MemberID == 0 && OrganizationID == 0)
                {
                    getMemberDetails = Context.Members.ToList();
                }
                else if (MemberID != 0 && OrganizationID == 0)
                {
                    getMemberDetails = Context.Members.Where(m => m.MemberID == MemberID).ToList();
                }
                else
                {
                    getMemberDetails = Context.Members.Where(m => m.ID == OrganizationID && m.Type == 2 && m.MemberID== MemberID).ToList();
                }

               
            }
            return getMemberDetails;
        }
        //veena
        /// <summary>
        /// GetOrganizationPlanDetails
        /// </summary>
        /// <param name="intOrganizationPlanCode"></param>
        /// <returns></returns>
        public List<PPCPWebApiServices.Models.PPCPWebService.DC.OrganizationPlanDetails> GetOrganizationPlanDetails(int intOrganizationPlanCode)
        {
            List<PPCPWebApiServices.Models.PPCPWebService.DC.OrganizationPlanDetails> getOrganizationPlanDetails = new List<PPCPWebApiServices.Models.PPCPWebService.DC.OrganizationPlanDetails>();
            try
            {

                using (var context = new DALMemberService())
                {

                    SqlParameter PlanCode = new SqlParameter("@PlanCode", intOrganizationPlanCode);
                    getOrganizationPlanDetails = context.Database.SqlQuery<PPCPWebApiServices.Models.PPCPWebService.DC.OrganizationPlanDetails>("Pr_GetMemberPlans @PlanCode", PlanCode).ToList();
                }
                if (getOrganizationPlanDetails.Count >= 1)
                {
                    string json = "{\"PaymentIntervalsDetails1\":" + "[" + getOrganizationPlanDetails[0].PaymentInterval + "]" + "}";
                    var result = JsonConvert.DeserializeObject<PaymentIntervalsDetails>(json);

                    for (int i = 0; i < result.PaymentIntervalsDetails1.Count; i++)
                    {
                        PaymentIntervals intervals = new PaymentIntervals();
                        //getMemberPlanDetails[0].TotalAmount = result.PaymentIntervals[i].TotalAmount;
                        getOrganizationPlanDetails[0].InstallmentAmount = result.PaymentIntervalsDetails1[i].InstallmentAmount;
                        getOrganizationPlanDetails[0].Savings = result.PaymentIntervalsDetails1[i].Savings;
                        getOrganizationPlanDetails[0].InstallmentFee = result.PaymentIntervalsDetails1[i].InstallmentFee;
                        getOrganizationPlanDetails[0].Paymentschedule = result.PaymentIntervalsDetails1[i].Paymentschedule;
                        getOrganizationPlanDetails[0].NoofInstallments = result.PaymentIntervalsDetails1[i].NoofInstallments;
                        // IntervalsList.Add(intervals);
                    }
                }

                return getOrganizationPlanDetails;
            }
            catch (Exception ex)
            {

            }
            return getOrganizationPlanDetails;
        }
        public int ValidateProviderUserName(string Username)
        {
            int result = 0;
            using (var Context = new Dev_PPCPEntities(1))
            {
                result = Context.ProviderCredentials.Count(P => P.UserName == Username);

            }
            return result;

        }

        /// <summary>
        /// veena view OrganizationPaymentDetails using OrganizationID
        /// </summary>
        /// <param name="intOrganizationID"></param>
        /// <returns></returns>
        public List<MemberPlan> GetOrganizationPaymentDetails(int intOrganizationID, DateTime Todate, DateTime FromDate,int MemberID,string PaymentStatus,int PlanType)
        {
            List<MemberPlan> getMemberFamilyPlanDetails = new List<MemberPlan>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    string strtemp = "";
                    if (MemberID != 0)
                    {
                        strtemp = "1";
                    }                  
                    if (!String.IsNullOrEmpty(PaymentStatus))
                    {
                        strtemp = strtemp + "2";
                    }
                    if (PlanType != 0)
                    {
                        strtemp = strtemp + "3";
                    }
                    switch (strtemp)
                    {
                        case " ":
                            getMemberFamilyPlanDetails = Context.MemberPlans.Where(m => m.OrganizationID == intOrganizationID && m.CreatedDate <= Todate && m.CreatedDate >= FromDate).ToList();
                            break;
                        case "1":
                            getMemberFamilyPlanDetails = Context.MemberPlans.Where(m => m.OrganizationID == intOrganizationID && m.CreatedDate <= Todate && m.CreatedDate >= FromDate && m.MemberID == MemberID).ToList();
                            break;
                        case "2":
                            getMemberFamilyPlanDetails = Context.MemberPlans.Where(m => m.OrganizationID == intOrganizationID && m.CreatedDate <= Todate && m.CreatedDate >= FromDate && m.Status == PaymentStatus).ToList();

                            break;
                        case "3":
                            getMemberFamilyPlanDetails = Context.MemberPlans.Where(m => m.OrganizationID == intOrganizationID && m.CreatedDate <= Todate && m.CreatedDate >= FromDate && m.PlanType == PlanType).ToList();

                            break;
                        case "12":
                            getMemberFamilyPlanDetails = Context.MemberPlans.Where(m => m.OrganizationID == intOrganizationID && m.CreatedDate <= Todate && m.CreatedDate >= FromDate && m.MemberID == MemberID && m.Status == PaymentStatus).ToList();

                            break;
                        case "13":
                            getMemberFamilyPlanDetails = Context.MemberPlans.Where(m => m.OrganizationID == intOrganizationID && m.CreatedDate <= Todate && m.CreatedDate >= FromDate && m.MemberID == MemberID && m.PlanType == PlanType).ToList();
                            break;
                        case "23":
                            getMemberFamilyPlanDetails = Context.MemberPlans.Where(m => m.OrganizationID == intOrganizationID && m.CreatedDate <= Todate && m.CreatedDate >= FromDate && m.Status == PaymentStatus && m.PlanType == PlanType).ToList();
                            break;
                        case "123":
                            getMemberFamilyPlanDetails = Context.MemberPlans.Where(m => m.OrganizationID == intOrganizationID && m.CreatedDate <= Todate && m.CreatedDate >= FromDate && m.MemberID == MemberID && m.Status == PaymentStatus && m.PlanType == PlanType).ToList();
                            break;
                        default:
                            getMemberFamilyPlanDetails = Context.MemberPlans.Where(m => m.OrganizationID == intOrganizationID && m.CreatedDate <= Todate && m.CreatedDate >= FromDate).ToList();
                            break;
                    }
                    if (getMemberFamilyPlanDetails.Count > 0)
                    {
                        Decimal lengthSum = Convert.ToDecimal(getMemberFamilyPlanDetails.Select(x => x.TotalAmount).Sum());
                        getMemberFamilyPlanDetails[0].GrandTotalAmount = lengthSum;
                        Decimal GrandAmountPaid = Convert.ToDecimal(getMemberFamilyPlanDetails.Select(x => x.AmountPaid).Sum());
                        getMemberFamilyPlanDetails[0].GrandAmountPaid = GrandAmountPaid;

                    }

                }
            }

            catch (Exception ex)
            {

            }
           
            
            return getMemberFamilyPlanDetails;

        }

   
        public List<OrganizationUser> GetOrganizationUsers(int UserID, int OrganizationId)
        {
            List<OrganizationUser> getOrganizationUsers = new List<OrganizationUser>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    if (UserID == 0 && OrganizationId!=0)
                    {
                        getOrganizationUsers = Context.OrganizationUsers.Where(m => m.OrganizationID == OrganizationId).ToList();
                    }
                    else if (UserID == 0 && OrganizationId == 0)
                    {
                        getOrganizationUsers = Context.OrganizationUsers.ToList();
                    }
                    else if (UserID != 0 && OrganizationId == 0)
                    {
                        getOrganizationUsers = Context.OrganizationUsers.Where(m => m.UserID == UserID).ToList();
                    }
                    else
                    {
                        getOrganizationUsers = Context.OrganizationUsers.Where(m => m.OrganizationID == OrganizationId && m.UserID == UserID).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return getOrganizationUsers;
        }
        //Insertion of OrganizationPlan Details :Ragini on 20-09-2019
        public int InsertOrganizationPlan(int intOrgId, int intPlanId, DateTime strStartDate)
        {
            int res = 0;
            List<OrganizationPlan> getOrganizationPlans = new List<OrganizationPlan>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    OrganizationPlan objsys = new OrganizationPlan();

                    objsys.PlanID = intPlanId;
                    objsys.OrganizationID = intOrgId;
                    objsys.PlanstartDate = strStartDate;
                    objsys.IsDelete = false;
                    objsys.CreatedDate = DateTime.Parse(Convert.ToString(DateTime.Now), new CultureInfo("en-US"));
                    Context.OrganizationPlans.Add(objsys);
                    res = Context.SaveChanges();
                }

            }
            catch (Exception ex)
            {

            }
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="intOrganizationID"></param>
        /// <returns></returns>
        
        public object GetOrganizationProviderDetails(int intOrganizationID,int intProviderID)
        {

            List<ProviderDetails> getorganizationProviderDetail = new List<ProviderDetails>();
            try
            {

                using (var context = new Dev_PPCPEntities(1))
                {
                    SqlParameter OrganizationID = new SqlParameter("@OrganizationID", intOrganizationID);
                    SqlParameter ProviderID = new SqlParameter("@ProviderID", intProviderID);
                    getorganizationProviderDetail = context.Database.SqlQuery<ProviderDetails>("Pr_GetOrganizationProviders @OrganizationID,@ProviderID", OrganizationID, ProviderID).ToList();
                    //if (intOrganizationID == 0)
                    //{
                    //    getorganizationProviderDetail = context.Providers.ToList();

                    //}
                    //else if( intProviderID != 0 && intOrganizationID !=0)
                    //{
                    //    getorganizationProviderDetail = context.Providers.Where(a => a.OrganizationID == intOrganizationID && a.ProviderID== intProviderID).ToList();
                    //}
                    //else
                    //{
                    //    getorganizationProviderDetail = context.Providers.Where(a => a.OrganizationID == intOrganizationID).ToList();

                    //}

                }
            }
            catch (Exception ex)
            {

            }
            return getorganizationProviderDetail;

        }
        /// <summary>
        /// Updating View Provider Details in Organization Module By Ragini
        /// </summary>
        /// <param name="OrgProviderXml"></param>
        /// <returns></returns>
        public List<Result> UpdateOrganizationProviderDetails(string OrgProviderXml)
        {
            List<Result> res = new List<Result>();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ProviderDetails));
                StringReader rdr = new StringReader(OrgProviderXml);
                ProviderDetails providerDetails = (ProviderDetails)serializer.Deserialize(rdr);

                using (var Context = new Dev_PPCPEntities(1))
                {
                    if (providerDetails.ProviderID != 0)
                    {
                        Provider objProvider = Context.Providers.First(m => m.ProviderID == providerDetails.ProviderID);
                        objProvider.FirstName = providerDetails.FirstName;
                        objProvider.LastName = providerDetails.LastName;
                        objProvider.DOB = providerDetails.DOB;
                        objProvider.CountryName = providerDetails.CountryName;
                        objProvider.CountryID = providerDetails.CountryID;
                        objProvider.Email = providerDetails.Email;
                        objProvider.MobileNumber = providerDetails.MobileNumber;
                        objProvider.Gender = providerDetails.Gender;
                        objProvider.Salutation = providerDetails.Salutation;
                        objProvider.CountryCode = providerDetails.CountryCode;
                        objProvider.CityName = providerDetails.CityName;
                        objProvider.CityID = providerDetails.CityID;
                        objProvider.StateID = providerDetails.StateID;
                        objProvider.StateName = providerDetails.StateName;
                        objProvider.SpecializationName = providerDetails.SpecializationName;
                        objProvider.SpecializationID = providerDetails.SpecializationID;
                        objProvider.Address = providerDetails.Address;
                        objProvider.Zip = providerDetails.Zip;
                        objProvider.NPI = providerDetails.NPI;
                        objProvider.Address = providerDetails.Address;
                        objProvider.Fax = providerDetails.Fax;
                        objProvider.Degree = providerDetails.Degree;
                        int result = Context.SaveChanges();
                        Result objresult = new Result();
                        objresult.ResultID = result;
                        res.Add(objresult);
                    }
                }

            }
            catch (Exception ex)
            {
                Result objres = new Result();
                objres.ResultName = ex.Message;
                res.Add(objres);
            }
            return res;
        }

        public List<Organization> GetOrganizationDetails(int intOrganizationID)
        {
            List<Organization> getOrganizationDetails = new List<Organization>();
            using (var Context = new Dev_PPCPEntities(1))
            {
                if (intOrganizationID == 0)
                {
                    getOrganizationDetails = Context.Organizations.ToList();
                }
                else
                {
                    getOrganizationDetails = Context.Organizations.Where(a => a.OrganizationID == intOrganizationID).ToList();
                }

            }
            return getOrganizationDetails;
        }

        public List<PlansAndPlansMapping> GetOrganizationPlans(int intOrganizationID, int Type)
        {

            List<PlansAndPlansMapping> getOrganizationDetails = new List<PlansAndPlansMapping>();
            try {
                using (var context = new DALDefaultService())
                {
                    SqlParameter OrganizationID = new SqlParameter("@OrganizationID", intOrganizationID);
                    getOrganizationDetails = context.Database.SqlQuery<PlansAndPlansMapping>("Pr_GetOrganizationPlans @OrganizationID", OrganizationID).ToList();

                    if (Type == 1)//1 Subcribed
                    {
                        var result = getOrganizationDetails.OrderByDescending(item => item.PlanID).GroupBy(item => item.PlanID).SelectMany(g => g.Count() > 1 ? g.Where(x => x.OrgID != 0) : g).Where(a => a.OrgID == 1);
                        getOrganizationDetails = result.ToList();
                    }
                    else if (Type == 2)// Unsubcribed
                    {
                        var result = getOrganizationDetails.OrderByDescending(item => item.PlanID).GroupBy(item => item.PlanID).SelectMany(g => g.Count() > 1 ? g.Where(x => x.OrgID != 0) : g).Where(a => a.OrgID == 0);
                        getOrganizationDetails = result.ToList();
                    }
                    else//3 ALL
                    {
                        var result = getOrganizationDetails.OrderByDescending(item => item.PlanID).GroupBy(item => item.PlanID).SelectMany(g => g.Count() > 1 ? g.Where(x => x.OrgID != 0) : g);
                        getOrganizationDetails = result.ToList();

                    }
                }
              
            }
            catch (Exception ex)
            {

            }

            return getOrganizationDetails;
        }
        public List<PlansAndPlansMapping> GetOrganizationProviderPlans(int intOrganizationID, int intProviderID, int intType)
        {
            List<PlansAndPlansMapping> getOrganizationDetails = new List<PlansAndPlansMapping>();

            using (var context = new DALDefaultService())
            {
                SqlParameter OrganizationID = new SqlParameter("@OrganizationID", intOrganizationID);
                SqlParameter ProviderID = new SqlParameter("@ProviderID", intProviderID);
                getOrganizationDetails = context.Database.SqlQuery<PlansAndPlansMapping>("Pr_GetOrganizationProviderPlans @OrganizationID,@ProviderID", OrganizationID, ProviderID).ToList();
                if (intType == 1)
                {
                    var result = getOrganizationDetails.OrderByDescending(item => item.PlanID).GroupBy(item => item.PlanID).SelectMany(g => g.Count() > 1 ? g.Where(x => x.OrgID != 1) : g).Where(a => a.OrgID == 1);
                    getOrganizationDetails = result.ToList();
                }
                else if (intType == 2)
                {
                    var result = getOrganizationDetails.OrderByDescending(item => item.PlanID).GroupBy(item => item.PlanID).SelectMany(g => g.Count() > 1 ? g.Where(x => x.OrgID != 1) : g).Where(a => a.OrgID == 0);
                    getOrganizationDetails = result.ToList();
                }
                else
                {
                    var result = getOrganizationDetails.OrderByDescending(item => item.PlanID).GroupBy(item => item.PlanID).SelectMany(g => g.Count() > 1 ? g.Where(x => x.OrgID != 1) : g);
                    getOrganizationDetails = result.ToList();
                }

            }
            return getOrganizationDetails;
        }

        public List<Result> InsertOrganizationProviderPlans(int intOrganizationID, int intPlanID, DateTime strStartDate, int intProviderID, string strOrganizationName, string strPlanName, string strProviderName)
        {
            List<Result> objResult = new List<Result>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    PlansMapping objPlanMapping = new PlansMapping();

                    objPlanMapping.PlanID = intPlanID;
                    objPlanMapping.PlanName = strPlanName;
                    objPlanMapping.OrganizationID = intOrganizationID;
                    objPlanMapping.Organizationname = strOrganizationName;
                    objPlanMapping.PlanStartDate = strStartDate;
                    objPlanMapping.ProviderID = intProviderID;
                    objPlanMapping.ProviderName = strProviderName;
                    objPlanMapping.IsActive = false;
                    Context.PlansMappings.Add(objPlanMapping);
                    int res = Context.SaveChanges();
                    Result obj = new Result();
                    obj.ResultID = res;
                    objResult.Add(obj);
                }
            }
            catch (Exception ex)
            {
                Result obj = new Result();
                obj.ResultName = ex.Message;
                objResult.Add(obj);
            }
            return objResult;
        }

        public List<Result> UpdateMemberDetails(string xml)
        {
            //List<TemporaryMemberDetails> objTemporaryDetails = new List<TemporaryMemberDetails>();
            List<Result> objResult = new List<Result>();
            XmlSerializer serializer = new XmlSerializer(typeof(AddMemberDetails));
            StringReader rdr = new StringReader(xml);
            AddMemberDetails objMemberDetails = (AddMemberDetails)serializer.Deserialize(rdr);
            Member MemberDetails = new Member();
            

            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    //MemberDetails MemberDetails = new MemberDetails();

                    Member objMemberLoginDetails = Context.Members.First(m => m.MemberID == objMemberDetails.MemberID);
                    objMemberLoginDetails.MemberID = Convert.ToInt32(objMemberDetails.MemberID);
                    objMemberLoginDetails.FirstName = objMemberDetails.FirstName;
                    objMemberLoginDetails.LastName = objMemberDetails.LastName;
                    objMemberLoginDetails.DOB = objMemberDetails.DOB;
                    objMemberLoginDetails.Gender = Convert.ToString(objMemberDetails.Gender);
                    objMemberLoginDetails.Salutation = objMemberDetails.Salutation;
                    objMemberLoginDetails.Email = objMemberDetails.Email;
                    objMemberLoginDetails.CountryCode = objMemberDetails.CountryCode;
                    objMemberDetails.MobileNumber = objMemberDetails.MobileNumber;
                    objMemberLoginDetails.CountryID = objMemberDetails.CountryID;
                    objMemberLoginDetails.CountryName = objMemberDetails.CountryName;
                    objMemberLoginDetails.StateID = objMemberDetails.StateID;
                    objMemberLoginDetails.StateName = objMemberDetails.StateName;
                    objMemberLoginDetails.CityID = objMemberDetails.CityID;
                    objMemberLoginDetails.CityName = objMemberDetails.CityName;
                    objMemberLoginDetails.Zip = objMemberDetails.Zip;
                    objMemberLoginDetails.IsTwofactorAuthentication = Convert.ToBoolean(objMemberDetails.IsTwofactorAuthentication);
                    objMemberLoginDetails.TwoFactorType = Convert.ToInt32(objMemberDetails.TwoFactorType);
                    //Result = Context.SaveChanges();
                    int res = Context.SaveChanges();
                    Result obj = new Result();
                    obj.ResultID = res;
                    objResult.Add(obj);
                }
            }
            catch (Exception ex)
            {
                Result obj = new Result();
                obj.ResultName = ex.Message;
                objResult.Add(obj);
            }
            return objResult;
        }
        public List<Result> UpdateUserDetails(string xml)
        {
            //List<TemporaryMemberDetails> objTemporaryDetails = new List<TemporaryMemberDetails>();
            List<Result> objResult = new List<Result>();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(OrganizationDetails));
                StringReader rdr = new StringReader(xml);
                OrganizationDetails objUserDetails = (OrganizationDetails)serializer.Deserialize(rdr);

                using (var Context = new Dev_PPCPEntities(1))
                {
                    OrganizationUser objUserLoginDetails = Context.OrganizationUsers.First(m => m.UserID == objUserDetails.UserID);
                    objUserLoginDetails.UserID = Convert.ToInt32(objUserDetails.UserID);
                    objUserLoginDetails.FirstName = objUserDetails.FirstName;
                    objUserLoginDetails.LastName = objUserDetails.LastName;
                    objUserLoginDetails.DOB = objUserDetails.DOB;
                    objUserLoginDetails.Gender = Convert.ToString(objUserDetails.Gender);
                    objUserLoginDetails.Salutation = objUserDetails.Salutation;
                    objUserLoginDetails.Email = objUserDetails.Email;
                    objUserLoginDetails.CountryCode = objUserDetails.CountryCode;
                    objUserLoginDetails.MobileNumber = objUserDetails.MobileNumber;
                    objUserLoginDetails.CountryID = Convert.ToInt32(objUserDetails.CountryID);
                    objUserLoginDetails.CountryName = objUserDetails.CountryName;
                    objUserLoginDetails.StateID = objUserDetails.StateID;
                    objUserLoginDetails.StateName = objUserDetails.StateName;
                    objUserLoginDetails.CityID = objUserDetails.CityID;
                    objUserLoginDetails.CityName = objUserDetails.CityName;
                    objUserLoginDetails.Zip = objUserDetails.Zip;
                    objUserLoginDetails.IsTwofactorAuthentication = Convert.ToBoolean(objUserDetails.IsTwofactorAuthentication);
                    objUserLoginDetails.TwoFactorType = Convert.ToInt32(objUserDetails.TwoFactorType);
                    //Result = Context.SaveChanges();
                    int res = Context.SaveChanges();
                    Result obj = new Result();
                    obj.ResultID = res;
                    objResult.Add(obj);
                }
            }
            catch (Exception ex)
            {
                Result obj = new Result();
                obj.ResultName = ex.Message;
                objResult.Add(obj);
            }
            return objResult;
        }
        //       

        public object GetOrganizationMembersAutoComplete(int OrganizationID,string Text)
        {
            using (var Context = new Dev_PPCPEntities(1))
            {
                var dataset = Context.Members
               .Where(x => x.ID == OrganizationID && x.FirstName.Contains(Text) || x.LastName.Contains(Text) || x.MobileNumber.Contains(Text))
               .Select(x => new
               {
                   MemberName = x.FirstName + " " + x.LastName + "," + x.MobileNumber,
                   MemberID = x.MemberID,

               }).ToList();
                return dataset;               
            }


        }
        /// <summary>
        /// Returning Oject of the searched text values to ViewProviders in Organization Module -- Ragini
        /// </summary>
        public object GetProviderDetailAutoComplete(int intOrganizationID,string Text)
        {

            using (var Context = new Dev_PPCPEntities(1))
            {
                var dataset = Context.Providers
               .Where(x => x.OrganizationID == intOrganizationID) 
               .Where(x=>x.FirstName.Contains(Text) || x.LastName.Contains(Text) || x.MobileNumber.Contains(Text))
               .Select(x => new
               {
                   ProviderName = x.FirstName + " " + x.LastName + "," + x.MobileNumber,
                   ProviderID = x.ProviderID,

               }).ToList();
                return dataset;
            }
 
        }

        public List<Result> UpdateTermsandConditions(int OrgaziationID,int UserID, int OrgType, int OrgUserType)
        {
            List<Result> obj = new List<Result>();
            Result res = new Result();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    if(OrgType != 0){

                        Organization Organization = Context.Organizations.First(m => m.OrganizationID == OrgaziationID);
                        Organization.TandCAcceptedDate = DateTime.Parse(Convert.ToString(DateTime.Now), new CultureInfo("en-US"));
                        int Result = Context.SaveChanges();

                        res.ResultID = Result;
                        obj.Add(res);
                    }
                    if (OrgUserType != 0)
                    {

                        OrganizationUser Organization = Context.OrganizationUsers.First(m => m.UserID == UserID);
                        Organization.TandCAcceptedDate = DateTime.Parse(Convert.ToString(DateTime.Now), new CultureInfo("en-US"));
                        int Result = Context.SaveChanges();

                        res.ResultID = Result;
                        obj.Add(res);
                    }
                }
            }
            catch (Exception ex)
            {
                res.ResultName = ex.Message;
                obj.Add(res);
            }

            return obj;
        }


        public List<Member> GetMemberPendingEnrollment(int OrganizationID)
        {
            List<Member> objMember = new List<Member>();
            using (var Context = new Dev_PPCPEntities(1))
            {
                try
                {
                    objMember = Context.Members.SqlQuery("select * from Member where MemberID not in  (select distinct  m.MemberID from Member m inner join MemberPlans mp on m.MemberID=mp.MemberID where m.ID=@id) and ID=@id ", new SqlParameter("@id", OrganizationID)).ToList();
                }catch(Exception ex)
                {

                }

                return objMember;
            }
        }


        public List<Result> UnSubscribedProviderPlan(int OrgaziationID, int ProviderID,int PlanMapID)
        {
            List<Result> obj = new List<Result>();
            Result res = new Result();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    if (ProviderID!=0)
                    {

                        PlansMapping PlansMappings = Context.PlansMappings.First(m => m.PlanMapID == PlanMapID );
                        PlansMappings.IsDelete = true;
                        int Result = Context.SaveChanges();

                        res.ResultID = Result;
                        obj.Add(res);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                res.ResultName = ex.Message;
                obj.Add(res);
            }

            return obj;
        }


        public List<Result> UnSubscribePlanDetails(int OrgaziationID, int PlanID, int PlanMapID)
        {
            List<Result> obj = new List<Result>();
            Result res = new Result();
            List<PlansMapping> PlansMappingList = new List<PlansMapping>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    if (OrgaziationID != 0 && PlanID!=0 && PlanMapID!=0)
                    {

                        OrganizationPlan OrganizationPlans = Context.OrganizationPlans.First(m => m.OrganizationPlanID == PlanMapID);
                        OrganizationPlans.IsDelete = true;
                        int Result = Context.SaveChanges();

                        PlansMappingList = Context.PlansMappings.Where(m => m.OrganizationID == OrgaziationID && m.PlanID == PlanID).ToList();
                        for(int i=0;i< PlansMappingList.Count; i++)
                        {
                            int Planmapid = (int)PlansMappingList[i].PlanMapID;
                            PlansMapping PlansMappings = Context.PlansMappings.First(m => m.PlanMapID == Planmapid);
                            PlansMappings.IsDelete = true;
                            int Result1 = Context.SaveChanges();
                        }
                        res.ResultID = Result;
                        obj.Add(res);
                    }

                }
            }
            catch (Exception ex)
            {
                res.ResultName = ex.Message;
                obj.Add(res);
            }

            return obj;
        }
        //        using (var ctx = new SchoolDBEntities())
        //{
        //    var student = ctx.Students
        //                    .SqlQuery("Select * from Students where StudentId=@id", new SqlParameter("@id", 1))
        //                    .FirstOrDefault();
        //}

        public object GetPPCPReports(DateTime FromDate, DateTime ToDate, int ProviderID, string PaymentStatus, string PlanType,
            int OrganziationID,int Type)
        {
            DateTime todate1=DateTime.Now;
            if(ToDate !=null)

            {
                todate1 = ToDate.AddDays(1).AddMinutes(-1);
            }
            List<PPCPReports> getorganizationProviderDetail = new List<PPCPReports>();
            try
            {

                using (var context = new Dev_PPCPEntities(1))
                {
                    SqlParameter fromdate = new SqlParameter("@FromDate", FromDate);
                    SqlParameter todate = new SqlParameter("@ToDate", todate1);                   
                    SqlParameter Providerid = new SqlParameter("@ProviderID", ProviderID);
                    SqlParameter status = new SqlParameter("@PaymentStatus", PaymentStatus);
                    SqlParameter plantype = new SqlParameter("@PlanType", PlanType);
                    SqlParameter orgid = new SqlParameter("@OrganziationID", OrganziationID);
                    SqlParameter type = new SqlParameter("@Type", Type);

                    
                    getorganizationProviderDetail = context.Database.SqlQuery<PPCPReports>("Pr_GetPPCPReports @FromDate,@ToDate,@ProviderID,@PaymentStatus,@PlanType,@OrganziationID,@Type", fromdate,todate, Providerid, status,plantype, orgid,type).ToList();
                  

                }
            }
            catch (Exception ex)
            {

            }
            return getorganizationProviderDetail;

        }

        /// <summary>
        /// veena view OrganizationPaymentDetails using OrganizationID
        /// </summary>
        /// <param name="intOrganizationID"></param>
        /// <returns></returns>
        public List<TransactionstoPractice> GetTransactionsToPractice(int intOrganizationID, DateTime Todate, DateTime FromDate)
        {
            List<TransactionstoPractice> getTransactionstoPractice = new List<TransactionstoPractice>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    getTransactionstoPractice = Context.TransactionstoPractices.Where(m => m.OrganizationID == intOrganizationID && m.PaymentDate <= Todate && m.PaymentDate >= FromDate).ToList();
                                       
                }
            }

            catch (Exception ex)
            {

            }

            return getTransactionstoPractice;

        }



        public object GetMembersList(int intOrganizationID, int intMemberID)
        {

            List<MembersList> getorganizationProviderDetail = new List<MembersList>();
            try
            {
                using (var context = new Dev_PPCPEntities(1))
                {                    
                    SqlParameter memberID = new SqlParameter("@MemberID", intMemberID);
                    SqlParameter OrganizationID = new SqlParameter("@OrganizationID", intOrganizationID);
                    getorganizationProviderDetail = context.Database.SqlQuery<MembersList>("Pr_GetMembersList @MemberID,@OrganizationID", memberID, OrganizationID ).ToList();
                    
                }
            }
            catch (Exception ex)
            {

            }
            return getorganizationProviderDetail;

        }


    }
}
