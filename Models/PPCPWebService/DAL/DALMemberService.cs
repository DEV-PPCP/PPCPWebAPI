﻿using PPCPWebApiServices.Models.PPCPWebService.DC;
using PPCPWebApiServices.Models.Service;
using Stripe;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using PPCPWebApiServices.ServiceAccess;
using PPCPWebApiServices;

namespace PPCPWebApiServices.Models.PPCPWebService.DAL
{
    public class DALMemberService : DbContext
    {
        static string StripeApiKey = System.Configuration.ConfigurationManager.AppSettings["StripeApiKey"];
        static string StripeAccountID = System.Configuration.ConfigurationManager.AppSettings["IphStripeAccountId"];
       

        public List<TemporaryMemberDetails> SaveMemberSignUP(string xml)
        {
            StripeMethods stripeMethod = new StripeMethods();
            TemporaryMemberDetails res = new TemporaryMemberDetails();
            List<TemporaryMemberDetails> objTemporaryDetails = new List<TemporaryMemberDetails>();
            XmlSerializer serializer = new XmlSerializer(typeof(MemberDetails));
            StringReader rdr = new StringReader(xml);
            MemberDetails objMemberDetails = (MemberDetails)serializer.Deserialize(rdr);
            string MemberPlanInstallmentxml = "";
            try
            {
                int TotalTransferAmount = 0;
                              string UserPassword = objMemberDetails.Password;
                byte[] bytes = Encoding.UTF8.GetBytes(UserPassword);
                string Encryptpassword = Convert.ToBase64String(bytes);
                Intervals intervals = new Intervals();
                intervals.InstallmentAmount = Convert.ToDecimal(objMemberDetails.InstallmentAmount);
                intervals.NoofInstallments = Convert.ToInt32(objMemberDetails.NoofInstallments);
                intervals.InstallmentFee = Convert.ToDecimal(objMemberDetails.InstallmentFee);
                intervals.Savings = Convert.ToInt32(objMemberDetails.Savings);
                intervals.Paymentschedule = objMemberDetails.Paymentschedule;
                intervals.EnrollFee = objMemberDetails.EnrollFee;

                var newtotalamount = ((intervals.InstallmentAmount + intervals.InstallmentFee) * intervals.NoofInstallments) + intervals.EnrollFee;


                var json = JsonConvert.SerializeObject(intervals);
                string Paymentinterval = json;
                decimal Transactionfee = 0;
                string TransactioId = "", Netamount = "0.0", StripeCustomerId = ""; ;
                decimal Totalamount = Convert.ToDecimal(objMemberDetails.AmountPaid);
                var doctortransferamount = Convert.ToDouble(objMemberDetails.InstallmentAmount);

                //added by akhil
                var CommPPCP = Convert.ToDouble(objMemberDetails.CommPPCP);
                var CommPrimaryMember = Convert.ToDouble(objMemberDetails.CommPrimaryMember);
                //end
                if (!string.IsNullOrEmpty(objMemberDetails.CVV) || !string.IsNullOrEmpty(objMemberDetails.CardID))
                {
                    //Create Token if entered new card details
                    var token = "";
                    Token objtoken = new Token();
                    if (String.IsNullOrEmpty(objMemberDetails.CardID))
                    {
                        try
                        {
                            objtoken = stripeMethod.StripeTokenCreation(objMemberDetails.CardNumber, Convert.ToInt32(objMemberDetails.MM),
                                Convert.ToInt32(objMemberDetails.YY), objMemberDetails.CVV, objMemberDetails.NameOnCard);
                            token = objtoken.Id;
                        }
                        catch (Exception ex)
                        {
                            res.result = ex.Message;
                            objTemporaryDetails.Add(res);
                            return objTemporaryDetails;
                        }
                    }
                    //    check the card details exsists are not
                    if (String.IsNullOrEmpty(objMemberDetails.CardID) && !string.IsNullOrEmpty(objMemberDetails.StripeCustomerID))//validat the new card and exsisting card-CustomerID != ""
                    {
                        try
                        {
                            int intValidateCard = stripeMethod.ValidateExistingCardDetails(objMemberDetails.StripeCustomerID, objtoken);
                            if (intValidateCard == 1)
                            {
                                res.result = "Card already exists";
                                objTemporaryDetails.Add(res);
                                return objTemporaryDetails;
                            }
                        }
                        catch (Exception ex)
                        {
                            res.result = ex.Message;
                            objTemporaryDetails.Add(res);
                            return objTemporaryDetails;
                        }

                    }
                    //create the stripe customer id
                    if (string.IsNullOrEmpty(objMemberDetails.StripeCustomerID))
                    {
                        Stripe.Customer current = stripeMethod.GetCustomer(objMemberDetails.Email, token);
                        objMemberDetails.StripeCustomerID = current.Id;
                        StripeCustomerId = current.Id;//StripeCustomerId-if new CustomerID save into StripeCustomerId variable for sp we dont need to update every time
                    }
                    Stripe.Card card = new Stripe.Card();
                    var mycharge = new Stripe.ChargeCreateOptions();
                    if (!string.IsNullOrEmpty(objMemberDetails.StripeCustomerID) && StripeCustomerId == "" && objMemberDetails.CardID == "")
                    {
                        try
                        {
                            card = stripeMethod.AddCardToCustomet(token, objMemberDetails.StripeCustomerID);
                            mycharge.Source = card.Id;
                        }
                        catch (Exception ex)
                        {
                            res.result = ex.Message;
                            objTemporaryDetails.Add(res);
                            return objTemporaryDetails;
                        }
                    }
                    if (!string.IsNullOrEmpty(objMemberDetails.StripeCustomerID) && !string.IsNullOrEmpty(objMemberDetails.CardID))
                    {
                        mycharge.Source = objMemberDetails.CardID;
                    }
                    var dblCurrency = Convert.ToDouble(objMemberDetails.AmountPaid);
                    var rounded = Math.Round(dblCurrency, 2);
                    var cents = rounded * 100;
                    int chargetotal = Convert.ToInt32(cents);
                    //added by akhil to calculate transfer amount
                    var TransferAmount = 0.00;
                    if (CommPrimaryMember == 0)
                    {
                        TransferAmount = doctortransferamount;
                    }
                    else {
                        TransferAmount = Convert.ToDouble((CommPrimaryMember / 100) * doctortransferamount);
                    }
                    var TransferAmountround = Math.Round(TransferAmount, 2);
                    var TransferAmountroundcents = TransferAmountround * 100;
                     TotalTransferAmount = Convert.ToInt32(TransferAmountroundcents);
                    //end
                    var stripeFeePercentage = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["LabCommissionPercentage"]);
                    var stripeFee = (stripeFeePercentage * rounded) / 100;
                    var roundedstripeFee = Math.Round(stripeFee, 2);
                    int stripefeeCents = Convert.ToInt32(roundedstripeFee * 100);
                    int stripeExtraFee = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LabStripeExtraFee"]);
                    mycharge.Currency = "USD";
                    mycharge.Customer = objMemberDetails.StripeCustomerID;
                    mycharge.Amount = chargetotal;
                    var TransferDataOptions = new ChargeTransferDataOptions()
                    {
                        Destination = (!string.IsNullOrEmpty(objMemberDetails.StripeAccountID) ? objMemberDetails.StripeAccountID.Trim() : StripeAccountID.Trim()),
                        //Amount = chargetotal - stripefeeCents - stripeExtraFee
                        Amount = TotalTransferAmount - stripefeeCents - stripeExtraFee
                    };
                    mycharge.TransferData = TransferDataOptions;
                    mycharge.OnBehalfOf = objMemberDetails.StripeAccountID.Trim();// StripeAccountID.Trim();
                                                                                  // mycharge.Destination = StripeAccountID;
                                                                                  // mycharge.DestinationAmount = chargetotal - stripefeeCents - stripeExtraFee;
                    mycharge.Description = "Plans";
                    mycharge.StatementDescriptorSuffix = "Plans";
                    decimal netAmount = Convert.ToDecimal(chargetotal - stripefeeCents - stripeExtraFee) / Convert.ToDecimal(100);
                    decimal transactionFee = Convert.ToDecimal(stripefeeCents + stripeExtraFee) / Convert.ToDecimal(100);
                    //var chargeservice = new StripeChargeService(StripeApiKey);
                    //StripeCharge currentcharge = chargeservice.Create(mycharge);
                    var ChargeService = new ChargeService();
                    Charge currentcharge = ChargeService.Create(mycharge);
                    if (!string.IsNullOrEmpty(currentcharge.Id))
                    {
                        TransactioId = currentcharge.BalanceTransactionId;
                        Transactionfee = transactionFee;
                        Netamount = Convert.ToString(netAmount);
                    }

                    //StripeConfiguration.ApiKey = "sk_test_4eC39HqLyjWDarjtT1zdp7dc";

                    ////var options = new ChargeCreateOptions
                    ////{
                    ////    Amount = 1500,
                    ////    Currency = "usd",
                    ////    Customer = StripeCustomerId,
                    ////};
                    ////var service = new ChargeService();
                    ////var charge = service.Create(options);








                    //var items = new List<SubscriptionItemOptions> {
                    //new SubscriptionItemOptions {
                    //    Price = "price_CBb6IXqvTLXp3f"}
                    //      };
                    //var options = new SubscriptionCreateOptions
                    //{
                    //    Customer = StripeCustomerId,// "cus_4fdAW5ftNQow1a",
                    //    Items = items,
                    //    BillingCycleAnchor = DateTimeOffset.FromUnixTimeSeconds(1591690889).UtcDateTime,
                    //};
                    //var service = new SubscriptionService();
                    //Subscription subscription = service.Create(options);



                    //var options1 = new Stripe.PriceCreateOptions
                    //{
                    //    Nickname = "Standard Quarterly",
                    //    Product = "{{CAR_WASH_PRODUCT_ID}}",
                    //    UnitAmount = 5700,
                    //    Currency = "usd",
                    //    Recurring = new PriceRecurringOptions
                    //    {
                    //        Interval = "month",
                    //        IntervalCount = 3,
                    //        UsageType = "licensed",
                    //    },
                    //};

                
                    //var price = service.Create(options);






                    MemberPlanInstallmentxml = MemberPlanInstallment(objMemberDetails.NoofInstallments, objMemberDetails.Paymentschedule, objMemberDetails.PlanStartDate, objMemberDetails.InstallmentAmount, objMemberDetails.InstallmentFee);
                }
                else
                {
                    Netamount = Convert.ToString(objMemberDetails.AmountPaid);
                    Transactionfee = 0;
                    newtotalamount = 0;

                }
                using (var context = new DALMemberService())
                {

                    SqlParameter XML = new SqlParameter("@XML", xml);
                    SqlParameter EncryptPassword = new SqlParameter("@EncryptPassword", Encryptpassword);
                    SqlParameter TotalAmount = new SqlParameter("@TotalAmount", Totalamount);
                    SqlParameter NetAmount = new SqlParameter("@NetAmount", Netamount);
                    SqlParameter TransactionFee = new SqlParameter("@TransactionFee", Transactionfee);//Netamount send but the value is not saved
                    SqlParameter MemberName = new SqlParameter("@MemberName", objMemberDetails.LastName + " " + objMemberDetails.FirstName);
                    SqlParameter PaymentInterval = new SqlParameter("@PaymentInterval", Paymentinterval);
                    SqlParameter TransactioID = new SqlParameter("@TransactioID", TransactioId);
                    SqlParameter StripeCustomerID = new SqlParameter("@StripeCustomerID", StripeCustomerId);
                    SqlParameter MemberPlanInstallmentXML = new SqlParameter("@MemberPlanInstallmentXML", MemberPlanInstallmentxml);
                    SqlParameter TotalEnrollAmount = new SqlParameter("@TotalEnrollAmount", newtotalamount);
                    SqlParameter Transferamount = new SqlParameter("@TransferAmount", (TotalTransferAmount/100));
                    objTemporaryDetails = context.Database.SqlQuery<TemporaryMemberDetails>("Pr_SaveMemberDetails @XML,@EncryptPassword,@TotalAmount,@NetAmount,@TransactionFee,@MemberName,@PaymentInterval,@TransactioID,@StripeCustomerID,@MemberPlanInstallmentXML,@TotalEnrollAmount,@TransferAmount", XML, EncryptPassword, TotalAmount, NetAmount, TransactionFee, MemberName, PaymentInterval, TransactioID, StripeCustomerID, MemberPlanInstallmentXML, TotalEnrollAmount, Transferamount).ToList();

                }
                if (objTemporaryDetails.Count > 0)
                {
                    objTemporaryDetails[0].TransactionID = TransactioId;
                }
                if (objTemporaryDetails.Count > 0 && objMemberDetails.MobileNumber != "")
                {
                    DALDefaultService dal = new DALDefaultService();
                    string message = "Thank you for enrolling with PPCP. Your UserName : " + objMemberDetails.UserName + " and Password : " + objMemberDetails.Password;
                    dal.SendMessageByText(message, objMemberDetails.MobileNumber, objMemberDetails.CountryCode);
                }
                if (Convert.ToDecimal(Netamount) > 0)
                {
                    var Membername = objMemberDetails.LastName + " " + objMemberDetails.FirstName;
                    var Email = objMemberDetails.Email;
                    PaymentreceiptEmail(Membername, Email, TransactioId, Convert.ToString(Netamount), objMemberDetails.OrganizationName);
                }

            }
            catch (Exception ex)
            {
                res.result = ex.Message;
                objTemporaryDetails.Add(res);
            }
            return objTemporaryDetails;
        }


        public static string ReadFile(string fileName)
        {
            try
            {
                String filename = HttpContext.Current.Server.MapPath(fileName);
                StreamReader objStreamReader = System.IO.File.OpenText(filename);
                String contents = objStreamReader.ReadToEnd();
                return contents;
            }
            catch (Exception)
            {
            }
            return "";
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
        private void SendEmail(string message, string ProviderEmail, string body, string CcEmail)
        {
            try
            {
                List<Application_Parameter_Config> list = GetApplicationParameterConfig();
                string FromMail = list[4].PARAMETER_VALUE; ;
                string Password = list[5].PARAMETER_VALUE; ;
                Service.MailHelper _objMail = new Service.MailHelper();
                _objMail.SmtpMail(ProviderEmail, CcEmail, message, body, FromMail, Password);
            }
            catch (Exception ex)
            {

            }
        }
        private void PaymentreceiptEmail(string FirstName, string Email, string TransactionID, string PaidAmount, string OrganizationName)
        {
            try
            {
                string body = ReadFile(VirtualPathUtility.MakeRelative(VirtualPathUtility.ToAppRelative(HttpContext.Current.Request.CurrentExecutionFilePath), "~/Resource/EmailTemplates/PaymentReceipt.htm"));

                //string sub = message + " " + "Payment Receipt";
                string sub = "Payment Receipt";
                body = body.Replace("##Date##", DateTime.Now.ToShortDateString());
                body = body.Replace("##PaidAmount##", PaidAmount);
                body = body.Replace("##PaymentMode##", "Card");
                body = body.Replace("##TransactionID##", TransactionID);
                body = body.Replace("##FullDate##", DateTime.Now.ToShortDateString());
                body = body.Replace("##FirstName##", FirstName);
                body = body.Replace("##OrganizationName##", OrganizationName);
                SendEmail(sub, Email, body, "");
            }
            catch (Exception ex)
            {
            }
        }

        public List<MemberLoginDetails> ValidateUser(string Username, string Password, string IpAddress)
        {
            List<MemberLoginDetails> listMemberDetails = new List<MemberLoginDetails>();
            try
            {
                using (var context = new DALMemberService())
                {
                    SqlParameter UserName = new SqlParameter("@UserName", Username);
                    SqlParameter PassWord = new SqlParameter("@Password", Password);
                    listMemberDetails = context.Database.SqlQuery<MemberLoginDetails>("Pr_ValidateMemberCredentials @UserName, @Password", UserName, PassWord).ToList();
                }
                if (listMemberDetails.Count >= 1)
                {
                    if (listMemberDetails[0].IsTwofactorAuthentication == true)
                    {
                        DALDefaultService dal = new DALDefaultService();

                        string OTP = dal.randamNumber();
                        string Message = "MyPhysicianPlan: DO NOT share this Sign In Code.  We will Never call you or text you for it.  Code " + OTP;
                        if (listMemberDetails[0].TwoFactorType == 1)
                        {
                            dal.SendMessageByText(Message, listMemberDetails[0].MobileNumber, listMemberDetails[0].CountryCode);
                            listMemberDetails[0].Otp = OTP;
                        }
                        else
                        {
                            if (listMemberDetails[0].PreferredIP != IpAddress)
                            {
                                dal.SendMessageByText(Message, listMemberDetails[0].MobileNumber, listMemberDetails[0].CountryCode);
                                listMemberDetails[0].Otp = OTP;
                            }
                            else
                            {
                                // No Action Required
                            }
                        }
                    }
                    //Member Terms and Conditions accepted method              
                    try
                    {
                        DALDefaultService objdal = new DALDefaultService();
                        List<TermsAndCondition> objTermsAndConditions = objdal.GetTermsAndConditions(1);//intType=1-Member
                        if (objTermsAndConditions.Count >= 1)
                        {
                            int value = DateTime.Compare(Convert.ToDateTime(listMemberDetails[0].TandCAcceptedDate), Convert.ToDateTime(objTermsAndConditions[0].CreatedDate));
                            if (value > 0)
                                listMemberDetails[0].TermsAndConditionsFlag = 0;
                            else if (value < 0)
                                listMemberDetails[0].TermsAndConditionsFlag = 1;

                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }
            catch (Exception ex)
            {

            }
            return listMemberDetails;
        }


        public string MemberPlanInstallment(int NoofInstalments, string PaymentSchedule, DateTime PlanStartDate, decimal InstallmentAmount, decimal InstallmentFee)
        {
            List<DCMemberPlanInstallmentMapping> objList = new List<DCMemberPlanInstallmentMapping>();
            int AddMonth = 0;
            if (PaymentSchedule == "Quaterly" && AddMonth == 0)
            {
                AddMonth = 3;
            }
            else if (PaymentSchedule == "Half Yearly" && AddMonth == 0)
            {
                AddMonth = 6;
            }
            else if (PaymentSchedule == "Monthly" && AddMonth == 0)
            {
                AddMonth = 1;
            }
            for (int i = 0; i < NoofInstalments; i++)
            {
                DCMemberPlanInstallmentMapping obj = new DCMemberPlanInstallmentMapping();
                if (i == 0)
                {
                    obj.PaymentDueDate = PlanStartDate.AddMonths(0);
                }
                else
                {
                    obj.PaymentDueDate = Convert.ToDateTime(objList[i - 1].PaymentDueDate).AddMonths(AddMonth);
                }
                obj.InstallmentAmount = InstallmentFee;
                obj.PaymentAmount = InstallmentAmount;
                objList.Add(obj);



            }
            string xml = GetXMLFromObject(objList);
            string returnData = xml.Replace("\"", "\'");
            return returnData;


        }


        public static string GetXMLFromObject(object o)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter tw = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(o.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, o);
            }
            catch (Exception ex)
            {
                //Handle Exception Code
            }
            finally
            {
                sw.Close();
                if (tw != null)
                {
                    tw.Close();
                }
            }
            return sw.ToString();
        }

        public List<TemporaryMemberDetails> EnrollPlanDetails(string xml)
        {
            StripeMethods stripeMethods = new StripeMethods();
            TemporaryMemberDetails res = new TemporaryMemberDetails();
            List<TemporaryMemberDetails> objTemporaryDetails = new List<TemporaryMemberDetails>();



            //  XmlSerializer serializer = new XmlSerializer(typeof(MemberDetails));
            XmlSerializer serializer = new XmlSerializer(typeof(List<MemberDetails>));
            StringReader rdr = new StringReader(xml);
            List<MemberDetails> objMemberDetails = (List<MemberDetails>)serializer.Deserialize(rdr);

            string Paymentinterval = "", StripeCustomerId = "", OrgstripeAccountID = "";
            decimal Amount;
            using (var context = new DALMemberService())
            {
                try
                {
                    //Create Paymentinterval json
                    Intervals intervals = new Intervals();
                    intervals.InstallmentAmount = Convert.ToDecimal(objMemberDetails[0].InstallmentAmount);
                    intervals.NoofInstallments = Convert.ToInt32(objMemberDetails[0].NoofInstallments);
                    intervals.InstallmentFee = Convert.ToDecimal(objMemberDetails[0].InstallmentFee);
                    intervals.Savings = Convert.ToInt32(objMemberDetails[0].Savings);
                    intervals.Paymentschedule = objMemberDetails[0].Paymentschedule;
                    intervals.EnrollFee = objMemberDetails[0].EnrollFee;
                    var newtotalamount = ((intervals.InstallmentAmount + intervals.InstallmentFee) * intervals.NoofInstallments) + intervals.EnrollFee;

                    var json = JsonConvert.SerializeObject(intervals);

                    Paymentinterval = json;
                    //Added by akhil
                    var CommPPCP = Convert.ToDouble(objMemberDetails[0].CommPPCP);
                    var CommPrimaryMember = Convert.ToDouble(objMemberDetails[0].CommPrimaryMember);
                    var varTotalAmount = Convert.ToDouble(objMemberDetails[0].AmountPaid);
                  var doctortransferamount = Convert.ToDouble(objMemberDetails[0].InstallmentAmount);

                    //End
                    Token objtoken = new Token();
                    if (!String.IsNullOrEmpty(objMemberDetails[0].CVV) || !String.IsNullOrEmpty(objMemberDetails[0].CardID))
                    {
                        //Create Token if entered new card details
                        var token = "";
                        if (String.IsNullOrEmpty(objMemberDetails[0].CardID))
                        {
                            try
                            {
                                objtoken = stripeMethods.StripeTokenCreation(objMemberDetails[0].CardNumber, Convert.ToInt32(objMemberDetails[0].MM), Convert.ToInt32(objMemberDetails[0].YY), objMemberDetails[0].CVV, objMemberDetails[0].NameOnCard);
                                token = objtoken.Id;
                            }
                            catch (Exception ex)
                            {
                                res.result = ex.Message;
                                objTemporaryDetails.Add(res);
                                return objTemporaryDetails;
                            }
                        }
                        //check the card details exsists are not
                        if (String.IsNullOrEmpty(objMemberDetails[0].CardID) && !string.IsNullOrEmpty(objMemberDetails[0].StripeCustomerID))//validat the new card and exsisting card-CustomerID != ""
                        {
                            try
                            {
                                int intValidateCard = stripeMethods.ValidateExistingCardDetails(objMemberDetails[0].StripeCustomerID, objtoken);
                                if (intValidateCard == 1)
                                {
                                    res.result = "Card already exists";
                                    objTemporaryDetails.Add(res);
                                    return objTemporaryDetails;
                                }
                            }
                            catch (Exception ex)
                            {
                                res.result = ex.Message;
                                objTemporaryDetails.Add(res);
                                return objTemporaryDetails;
                            }
                        }

                        //create the stripe customer id
                        if (string.IsNullOrEmpty(objMemberDetails[0].StripeCustomerID))
                        {
                            try
                            {
                                Stripe.Customer current = stripeMethods.GetCustomer(objMemberDetails[0].Email, token);
                                objMemberDetails[0].StripeCustomerID = current.Id;
                                StripeCustomerId = current.Id;//StripeCustomerId-if new CustomerID save into StripeCustomerId variable for sp we dont need to update every time
                            }
                            catch (Exception ex)
                            {
                                res.result = ex.Message;
                                objTemporaryDetails.Add(res);
                                return objTemporaryDetails;
                            }
                        }
                        Stripe.Card card = new Stripe.Card();
                        var mycharge = new Stripe.ChargeCreateOptions();
                        if (!string.IsNullOrEmpty(objMemberDetails[0].StripeCustomerID) && StripeCustomerId == "" && objMemberDetails[0].CardID == "")
                        {
                            try
                            {
                                card = stripeMethods.AddCardToCustomet(token, objMemberDetails[0].StripeCustomerID);
                                mycharge.Source = card.Id;
                            }
                            catch (Exception ex)
                            {
                                res.result = ex.Message;
                                objTemporaryDetails.Add(res);
                                return objTemporaryDetails;
                            }
                        }
                        if (!string.IsNullOrEmpty(objMemberDetails[0].StripeCustomerID) && !string.IsNullOrEmpty(objMemberDetails[0].CardID))
                        {
                            mycharge.Source = objMemberDetails[0].CardID;
                        }
                        var dblCurrency = Convert.ToDouble(objMemberDetails[0].AmountPaid);//objMemberDetails[0].InstallmentAmount
                        var rounded = Math.Round(dblCurrency, 2);
                        var cents = rounded * 100;
                        int chargetotal = Convert.ToInt32(cents);
                        //added by akhil to calculate transfer amount
                        var TransferAmount = Convert.ToDouble((CommPrimaryMember / 100) * doctortransferamount);//Convert.ToDouble((CommPrimaryMember / 100) * varTotalAmount);
                        var TransferAmountround = Math.Round(TransferAmount, 2);
                        var TransferAmountroundcents = TransferAmountround * 100;
                        int TotalTransferAmount = Convert.ToInt32(TransferAmountroundcents);
                        //end
                        var stripeFeePercentage = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["LabCommissionPercentage"]);
                        var stripeFee = (stripeFeePercentage * rounded) / 100;
                        var roundedstripeFee = Math.Round(stripeFee, 2);
                        int stripefeeCents = Convert.ToInt32(roundedstripeFee * 100);
                        int stripeExtraFee = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LabStripeExtraFee"]);
                        mycharge.Currency = "USD";
                        mycharge.Customer = objMemberDetails[0].StripeCustomerID;
                        mycharge.Amount = chargetotal;
                        var TransferDataOptions = new ChargeTransferDataOptions()
                        {
                            Destination = objMemberDetails[0].StripeAccountID.Trim(), //!string.IsNullOrEmpty(OrgstripeAccountID)? OrgstripeAccountID.Trim(): StripeAccountID.Trim(),// OrgstripeAccountID,//StripeAccountID.Trim(),
                            // Amount = chargetotal - stripefeeCents - stripeExtraFee
                            Amount = TotalTransferAmount - stripefeeCents - stripeExtraFee
                        };
                        mycharge.TransferData = TransferDataOptions;
                        mycharge.OnBehalfOf = objMemberDetails[0].StripeAccountID.Trim();//OrgstripeAccountID.Trim();//StripeAccountID.Trim();
                        mycharge.StatementDescriptorSuffix = "Plans";
                        //mycharge.Destination = StripeAccountID;
                        //mycharge.DestinationAmount = chargetotal - stripefeeCents - stripeExtraFee;
                        mycharge.Description = "Plans";
                        decimal netAmount = Convert.ToDecimal(chargetotal - stripefeeCents - stripeExtraFee) / Convert.ToDecimal(100);
                        decimal transactionFee = Convert.ToDecimal(stripefeeCents + stripeExtraFee) / Convert.ToDecimal(100);
                        var ChargeService = new ChargeService();
                        Charge currentcharge = ChargeService.Create(mycharge);
                        if (!string.IsNullOrEmpty(currentcharge.Id))
                        {
                            string MemberPlanInstallmentxml = MemberPlanInstallment(objMemberDetails[0].NoofInstallments, objMemberDetails[0].Paymentschedule, objMemberDetails[0].PlanStartDate, objMemberDetails[0].InstallmentAmount, objMemberDetails[0].InstallmentFee);
                            string TransactioId = currentcharge.BalanceTransactionId;
                            decimal Totalamount = Convert.ToDecimal(objMemberDetails[0].AmountPaid);
                            decimal Netamount = Convert.ToDecimal(objMemberDetails[0].InstallmentAmount);
                            SqlParameter XML = new SqlParameter("@XML", xml);
                            SqlParameter TotalAmount = new SqlParameter("@TotalAmount", Totalamount);
                            SqlParameter NetAmount = new SqlParameter("@NetAmount", Netamount);
                            SqlParameter TransactionFee = new SqlParameter("@TransactionFee", transactionFee);
                            SqlParameter MemberName = new SqlParameter("@MemberName", objMemberDetails[0].MemberName);
                            SqlParameter PaymentInterval = new SqlParameter("@PaymentInterval", Paymentinterval);
                            SqlParameter TransactioID = new SqlParameter("@TransactioID", TransactioId);
                            SqlParameter StripeCustomerID = new SqlParameter("@StripeCustomerID", StripeCustomerId);
                            SqlParameter MemberPlanInstallmentXML = new SqlParameter("@MemberPlanInstallmentXML", MemberPlanInstallmentxml);
                            SqlParameter TotalEnrollAmount = new SqlParameter("@TotalEnrollAmount", newtotalamount);
                            SqlParameter Transferamount = new SqlParameter("@TransferAmount", (TotalTransferAmount/100));
                            objTemporaryDetails = context.Database.SqlQuery<TemporaryMemberDetails>("Pr_EnrollplaneDetails @XML,@TotalAmount,@NetAmount,@TransactionFee,@MemberName,@PaymentInterval,@TransactioID,@StripeCustomerID,@MemberPlanInstallmentXML,@TotalEnrollAmount,@TransferAmount", XML, TotalAmount, NetAmount, TransactionFee, MemberName, PaymentInterval, TransactioID, StripeCustomerID, MemberPlanInstallmentXML, TotalEnrollAmount, Transferamount).ToList();
                            if (objTemporaryDetails.Count > 0)
                            {
                                objTemporaryDetails[0].TransactionID = TransactioId;
                                objTemporaryDetails[0].StripeCustomerID = StripeCustomerId;
                            }
                            if (Convert.ToDecimal(Netamount) > 0)
                            {
                                var Membername = objMemberDetails[0].MemberName;
                                var Email = objMemberDetails[0].Email;
                                PaymentreceiptEmail(Membername, Email, TransactioId, Convert.ToString(Netamount), objMemberDetails[0].OrganizationName);
                            }
                        }
                    }
                    else
                    {
                        string TransactioId = "";
                        decimal Totalamount = Convert.ToDecimal(objMemberDetails[0].AmountPaid);
                        decimal Netamount = Convert.ToDecimal(objMemberDetails[0].InstallmentAmount);
                        SqlParameter XML = new SqlParameter("@XML", xml);
                        SqlParameter TotalAmount = new SqlParameter("@TotalAmount", Totalamount);
                        SqlParameter NetAmount = new SqlParameter("@NetAmount", Netamount);
                        SqlParameter TransactionFee = new SqlParameter("@TransactionFee", Netamount);//Netamount send but the value is not saved
                        SqlParameter MemberName = new SqlParameter("@MemberName", objMemberDetails[0].MemberName);
                        SqlParameter PaymentInterval = new SqlParameter("@PaymentInterval", Paymentinterval);
                        objTemporaryDetails = context.Database.SqlQuery<TemporaryMemberDetails>("Pr_EnrollplaneDetails @XML,@TotalAmount,@NetAmount,@TransactionFee,@MemberName,@PaymentInterval", XML, TotalAmount, NetAmount, TransactionFee, MemberName, PaymentInterval).ToList();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
                    string str = trace.GetFrame(0).GetMethod().ReflectedType.FullName;

                    if (objTemporaryDetails.Count > 0)
                    {
                        objTemporaryDetails[0].result = ex.Message;
                    }
                    else
                    {

                        res.result = ex.Message;
                        objTemporaryDetails.Add(res);
                    }
                    return objTemporaryDetails;
                }
            }
            return objTemporaryDetails;

        }
        /// <summary>
        /// This web service is used for Add family member details
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>

        public List<TemporaryMemberDetails> AddFamilyMemberDetails(string xml)
        {
            List<TemporaryMemberDetails> objTemporaryDetails = new List<TemporaryMemberDetails>();
            string MobileNumber = "", CountryCode = "", Encryptpassword = "", UserPassword = "", UserName = "";
            using (var context = new DALMemberService())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                XmlNode node = xmlDoc.SelectSingleNode("/MemberDetails ");
                XmlNode node2 = xmlDoc.SelectSingleNode("/MemberDetails/Password");
                XmlNode node3 = xmlDoc.SelectSingleNode("/MemberDetails/UserName");
                if (node2 != null && node3 != null)
                {
                    UserName = node["UserName"].InnerText;
                    UserPassword = node["Password"].InnerText;
                    byte[] bytes = Encoding.UTF8.GetBytes(UserPassword);
                    Encryptpassword = Convert.ToBase64String(bytes);
                }
                MobileNumber = node["MobileNumber"].InnerText;
                CountryCode = node["CountryCode"].InnerText;
                if (Encryptpassword != "")
                {
                    SqlParameter XML = new SqlParameter("@XML", xml);
                    SqlParameter EncryptPassword = new SqlParameter("@EncryptPassword", Encryptpassword);
                    objTemporaryDetails = context.Database.SqlQuery<TemporaryMemberDetails>("Pr_AddFamilyMemberDetails @XML,@EncryptPassword", XML, EncryptPassword).ToList();
                    if (objTemporaryDetails.Count > 0 && MobileNumber != "")
                    {
                        string message = "Thank you for enrolling with PPCP. Your UserName : " + UserName + " and Password : " + UserPassword;
                        SendMessageByText(message, MobileNumber, CountryCode);
                    }
                }
                else
                {
                    SqlParameter XML = new SqlParameter("@XML", xml);
                    objTemporaryDetails = context.Database.SqlQuery<TemporaryMemberDetails>("Pr_AddFamilyMemberDetails @XML", XML).ToList();
                }


                return objTemporaryDetails;
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


            }
            catch (Exception ex)
            {

            }
        }

        public StripePaymentDetails StriepPayment(string token, string Amount, string Membername,
            string CustomerID, string StripeCustomerId, string CardID, string OrgStripeAccountID, decimal? CommPPCP, decimal? CommPrimaryMember, string InstallmentAmount)
        {
            StripeMethods stripeMethods = new StripeMethods();
            StripePaymentDetails PaymentDetails = new StripePaymentDetails();

            try
            {
                Stripe.Card card = new Stripe.Card();
                var mycharge = new Stripe.ChargeCreateOptions();
                if (!string.IsNullOrEmpty(CustomerID) && string.IsNullOrEmpty(CardID) && StripeCustomerId == "")
                {
                    card = stripeMethods.AddCardToCustomet(token, CustomerID);
                    mycharge.Source = card.Id;
                }
                if (!string.IsNullOrEmpty(CustomerID) && !string.IsNullOrEmpty(CardID))
                {
                    mycharge.Source = CardID;
                }
                var CommPPCPAmount = Convert.ToDouble(CommPPCP);
                var CommPrimaryMemberAmount = Convert.ToDouble(CommPrimaryMember);
                var doctortransferamount = Convert.ToDouble(InstallmentAmount);
                var dblCurrency = Convert.ToDouble(Amount);
                var rounded = Math.Round(dblCurrency, 2);
                var cents = rounded * 100;
                int chargetotal = Convert.ToInt32(cents);
                //added by akhil to calculate transfer amount
                var TransferAmount = Convert.ToDouble((CommPrimaryMemberAmount / 100) * doctortransferamount);
                var TransferAmountround = Math.Round(TransferAmount, 2);
                var TransferAmountroundcents = TransferAmountround * 100;
                int TotalTransferAmount = Convert.ToInt32(TransferAmountroundcents);
                //end
                var stripeFeePercentage = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["LabCommissionPercentage"]);
                var stripeFee = (stripeFeePercentage * rounded) / 100;
                var roundedstripeFee = Math.Round(stripeFee, 2);
                int stripefeeCents = Convert.ToInt32(roundedstripeFee * 100);
                int stripeExtraFee = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LabStripeExtraFee"]);
                mycharge.Currency = "USD";
                mycharge.Customer = CustomerID;
                mycharge.Amount = chargetotal;
                var TransferDataOptions = new ChargeTransferDataOptions()
                {
                    Destination = OrgStripeAccountID,
                    Amount = TotalTransferAmount - stripefeeCents - stripeExtraFee
                };
                mycharge.TransferData = TransferDataOptions;
                mycharge.Description = "Plans";
                mycharge.StatementDescriptorSuffix = "Plans";
                mycharge.OnBehalfOf = OrgStripeAccountID.Trim();// StripeAccountID.Trim();
                decimal netAmount = Convert.ToDecimal(chargetotal - stripefeeCents - stripeExtraFee) / Convert.ToDecimal(100);
                decimal transactionFee = Convert.ToDecimal(stripefeeCents + stripeExtraFee) / Convert.ToDecimal(100);
                var ChargeService = new ChargeService();
                Charge currentcharge = ChargeService.Create(mycharge);
                if (!string.IsNullOrEmpty(currentcharge.Id))
                {
                    PaymentDetails.TransactionID = currentcharge.BalanceTransactionId;
                    PaymentDetails.TransactionFee = transactionFee;
                    PaymentDetails.NetAmount = netAmount;
                    PaymentDetails.TransferAmount = TotalTransferAmount;
                }
                return PaymentDetails;
            }
            catch (Exception ex)
            {
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
                string str = trace.GetFrame(0).GetMethod().ReflectedType.FullName;
                PaymentDetails.Result = ex.Message;
                return PaymentDetails;

            }
        }
        public List<TemporaryMemberDetails> UpdateMemberPlanPayments(string xml)
        {
            TemporaryMemberDetails res = new TemporaryMemberDetails();
            List<TemporaryMemberDetails> objTemporaryDetails = new List<TemporaryMemberDetails>();
            StripeMethods stripeMethod = new StripeMethods();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MakePayment));
                StringReader rdr = new StringReader(xml);
                MakePayment objMakePaymentDetails = (MakePayment)serializer.Deserialize(rdr);
                string StripeCustomerId = "";
                var token = "";
                if (objMakePaymentDetails.CVV != 0 || !string.IsNullOrEmpty(objMakePaymentDetails.CardID))
                {
                    //Create Token if entered new card details
                    Token objtoken = new Token();
                    if (String.IsNullOrEmpty(objMakePaymentDetails.CardID))
                    {
                        try
                        {
                            objtoken = stripeMethod.StripeTokenCreation(objMakePaymentDetails.CardNumber, Convert.ToInt32(objMakePaymentDetails.MM), Convert.ToInt32(objMakePaymentDetails.YY), Convert.ToString(objMakePaymentDetails.CVV), objMakePaymentDetails.NameOnCard);
                            token = objtoken.Id;
                        }
                        catch (Exception ex)
                        {
                            res.result = ex.Message;
                            objTemporaryDetails.Add(res);
                            return objTemporaryDetails;
                        }
                    }
                    //check the card details exsists are not 
                    if (String.IsNullOrEmpty(objMakePaymentDetails.CardID) && !string.IsNullOrEmpty(objMakePaymentDetails.StripeCustomerID))//validat the new card and exsisting card-CustomerID != ""
                    {
                        try
                        {
                            int intValidateCard = stripeMethod.ValidateExistingCardDetails(objMakePaymentDetails.StripeCustomerID, objtoken);
                            if (intValidateCard == 1)
                            {
                                res.result = "Card already exists";
                                objTemporaryDetails.Add(res);
                                return objTemporaryDetails;
                            }
                        }
                        catch (Exception ex)
                        {
                            res.result = ex.Message;
                            objTemporaryDetails.Add(res);
                            return objTemporaryDetails;
                        }
                    }
                    //create the stripe customer id
                    if (string.IsNullOrEmpty(objMakePaymentDetails.StripeCustomerID))
                    {
                        try
                        {
                            Stripe.Customer current = stripeMethod.GetCustomer(objMakePaymentDetails.Email, token);
                            objMakePaymentDetails.StripeCustomerID = current.Id;
                            StripeCustomerId = current.Id;//StripeCustomerId-if new CustomerID save into StripeCustomerId variable for sp we dont need to update every time
                        }
                        catch (Exception ex)
                        {
                            res.result = ex.Message;
                            objTemporaryDetails.Add(res);
                            return objTemporaryDetails;
                        }
                    }
                }
                string OrgStripeAccountID = !string.IsNullOrEmpty(objMakePaymentDetails.StripeAccountID) ? objMakePaymentDetails.StripeAccountID.Trim() : StripeCustomerId;
                StripePaymentDetails stripePaymentDetails = StriepPayment(token, Convert.ToString(objMakePaymentDetails.AmountPaid),
                    objMakePaymentDetails.MemberName, objMakePaymentDetails.StripeCustomerID, StripeCustomerId, objMakePaymentDetails.CardID, 
                    OrgStripeAccountID, objMakePaymentDetails.CommPPCP, objMakePaymentDetails.CommPrimaryMember, Convert.ToString(objMakePaymentDetails.InstallmentAmount));
                if (!string.IsNullOrEmpty(stripePaymentDetails.TransactionID))
                {
                    string TransactioId = stripePaymentDetails.TransactionID;
                    decimal Totalamount = Convert.ToDecimal(objMakePaymentDetails.TotalAmount);
                    decimal TotalAmountpaid = Convert.ToDecimal(objMakePaymentDetails.AmountPaid) + Convert.ToDecimal(objMakePaymentDetails.Amount);
                    decimal Netamount = Convert.ToDecimal(stripePaymentDetails.NetAmount);
                    decimal Dueamount = Totalamount - TotalAmountpaid;
                    // decimal temAmountPaid = Convert.ToDecimal(objMakePaymentDetails.AmountPaid);
                    string PaymentStatus = "paid";
                    if (Dueamount == 0)
                    {
                        PaymentStatus = "Paid";
                    }
                    else
                    {
                        PaymentStatus = "Partially Paid";
                    }
                    using (var context = new DALMemberService())
                    {
                        List<DCMemberPlanInstallmentMapping> objList = JsonConvert.DeserializeObject<List<DCMemberPlanInstallmentMapping>>(objMakePaymentDetails.MemberPlanInstallmentMapping);
                        string tempMemberPlanInstallmentxml = GetXMLFromObject(objList);
                        string MemberPlanInstallmentxml = tempMemberPlanInstallmentxml.Replace("\"", "\'");
                        SqlParameter XML = new SqlParameter("@XML", xml);
                        SqlParameter NetAmount = new SqlParameter("@NetAmount", Netamount);
                        SqlParameter TotalAmountPaid = new SqlParameter("@TotalAmountPaid", TotalAmountpaid);
                        SqlParameter AmountPaid = new SqlParameter("@AmountPaid", Convert.ToDecimal(objMakePaymentDetails.AmountPaid));
                        SqlParameter DueAmount = new SqlParameter("@DueAmount", Dueamount);
                        SqlParameter Status = new SqlParameter("@Status", PaymentStatus);
                        SqlParameter TransactionFee = new SqlParameter("@TransactionFee", stripePaymentDetails.TransactionFee);
                        SqlParameter TransactioID = new SqlParameter("@TransactioID", TransactioId);
                        SqlParameter StripeCustomerID = new SqlParameter("@StripeCustomerID", StripeCustomerId);
                        SqlParameter MemberPlanInstallmentXML = new SqlParameter("@MemberPlanInstallmentXML", MemberPlanInstallmentxml);
                        SqlParameter Transferamount = new SqlParameter("@TransferAmount", (stripePaymentDetails.TransferAmount/100));
                        objTemporaryDetails = context.Database.SqlQuery<TemporaryMemberDetails>("Pr_UpdateMemberPlanPaymentDetails @XML,@NetAmount,@TotalAmountPaid,@AmountPaid,@DueAmount,@Status,@TransactionFee,@TransactioID,@StripeCustomerID,@MemberPlanInstallmentXML,@TransferAmount", XML, NetAmount, TotalAmountPaid, AmountPaid, DueAmount, Status, TransactionFee, TransactioID, StripeCustomerID, MemberPlanInstallmentXML, Transferamount).ToList();

                    }
                    if (objTemporaryDetails.Count > 0)
                    {
                        objTemporaryDetails[0].TransactionID = TransactioId;
                        objTemporaryDetails[0].StripeCustomerID = StripeCustomerId;

                    }
                    if (objTemporaryDetails.Count > 0 && objMakePaymentDetails.MobileNumber != "")
                    {
                        DALDefaultService dal = new DALDefaultService();
                        string message = "Your payment is successful. Your transactionID is " + objTemporaryDetails[0].TransactionID;
                        dal.SendMessageByText(message, objMakePaymentDetails.MobileNumber, Convert.ToString(objMakePaymentDetails.CountryCode));
                    }

                    if (Convert.ToDecimal(Netamount) > 0)
                    {
                        var Membername = objMakePaymentDetails.MemberName;
                        var Email = objMakePaymentDetails.Email;
                        PaymentreceiptEmail(Membername, Email, objTemporaryDetails[0].TransactionID, Convert.ToString(Netamount), objMakePaymentDetails.OrganizationName);
                    }
                }
                else
                {
                    res.result = stripePaymentDetails.Result;
                    objTemporaryDetails.Add(res);
                }
            }
            catch (Exception ex)
            {
                if (objTemporaryDetails.Count > 0)
                {
                    objTemporaryDetails[0].result = ex.Message;

                }
                else
                {
                    res.result = ex.Message;
                    objTemporaryDetails.Add(res);
                }
            }
            return objTemporaryDetails;
        }



        //string CardName = "", CardNumber = "", Cvv = "", YY = "", MM = "", Membername = "", MemberId = "", Amount = "", MobileNumber = "", CountryCode = "", totalamount = "", amountpaid = "";
        //using (var context = new DALMemberService())
        //{

        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml(xml);
        //    XmlNode node = xmlDoc.SelectSingleNode("/MemberDetails ");
        //    Cvv = node["CVV"].InnerText;
        //    if (Cvv != "")
        //    {
        //        CardName = node["NameOnCard"].InnerText;
        //        CardNumber = node["CardNumber"].InnerText;
        //        Cvv = node["CVV"].InnerText;
        //        YY = node["YY"].InnerText;
        //        MM = node["MM"].InnerText;
        //        totalamount = node["TotalAmount"].InnerText;
        //        amountpaid = node["AmountPaid"].InnerText;
        //        MemberId = node["MemberID"].InnerText;
        //        Amount = node["Amount"].InnerText;
        //        Membername = node["MemberName"].InnerText;
        //        MobileNumber = node["MobileNumber"].InnerText;
        //        CountryCode = node["CountryCode"].InnerText;
        //    }
        //    try
        //    {
        //        if (Cvv != "")
        //        {
        //            var token = "";
        //            var stripeTokenCreateOptions = new StripeTokenCreateOptions
        //            {
        //                Card = new StripeCreditCardOptions
        //                {
        //                    Number = CardNumber,
        //                    ExpirationMonth = Convert.ToInt32(MM),
        //                    ExpirationYear = Convert.ToInt32(YY),
        //                    Cvc = Cvv,
        //                    Name = CardName
        //                }
        //            };

        //            var tokenService = new StripeTokenService();
        //            var stripeToken = tokenService.Create(stripeTokenCreateOptions);
        //            token = stripeToken.Id;



        //            //Call the StriepPayment method and get the TransactionID
        //            StripePaymentDetails stripePaymentDetails = StriepPayment(token, Amount, Membername, "", "", "");

        //            else if (!string.IsNullOrEmpty(stripePaymentDetails.Result))
        //            {
        //                TemporaryMemberDetails res = new TemporaryMemberDetails();
        //                res.result = stripePaymentDetails.Result;
        //                objTemporaryDetails.Add(res);
        //                return objTemporaryDetails;
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
        //        string str = trace.GetFrame(0).GetMethod().ReflectedType.FullName;

        //        if (objTemporaryDetails.Count > 0)
        //        {
        //            objTemporaryDetails[0].result = ex.Message;
        //        }
        //        else
        //        {
        //            TemporaryMemberDetails res = new TemporaryMemberDetails();
        //            res.result = ex.Message;
        //            objTemporaryDetails.Add(res);
        //        }
        //        return objTemporaryDetails;
        //    }


        public List<Member> GetMemberDetails(int intMemberID)
        {
            List<Member> objMember = new List<Member>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    objMember = Context.Members.Where(m => m.MemberID == intMemberID).ToList();

                }
            }
            catch (Exception ex)
            {

            }
            return objMember;
        }

        public List<PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlansDetails> GetMemberPlanDetails(int intMemberPlanCode)
        {
            List<PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlansDetails> getMemberPlanDetails = new List<PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlansDetails>();
            try
            {

                using (var context = new DALMemberService())
                {

                    SqlParameter PlanCode = new SqlParameter("@PlanCode", intMemberPlanCode);

                    getMemberPlanDetails = context.Database.SqlQuery<PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlansDetails>("Pr_GetMemberPlans @PlanCode", PlanCode).ToList();
                }
                if (getMemberPlanDetails.Count >= 1)
                {
                    string json = "{\"PaymentIntervalsDetails1\":" + "[" + getMemberPlanDetails[0].PaymentInterval + "]" + "}";
                    var result = JsonConvert.DeserializeObject<PaymentIntervalsDetails>(json);

                    for (int i = 0; i < result.PaymentIntervalsDetails1.Count; i++)
                    {
                        PaymentIntervals intervals = new PaymentIntervals();
                        //getMemberPlanDetails[0].TotalAmount = result.PaymentIntervals[i].TotalAmount;
                        getMemberPlanDetails[0].InstallmentAmount = result.PaymentIntervalsDetails1[i].InstallmentAmount;
                        getMemberPlanDetails[0].Savings = result.PaymentIntervalsDetails1[i].Savings;
                        getMemberPlanDetails[0].InstallmentFee = result.PaymentIntervalsDetails1[i].InstallmentFee;
                        getMemberPlanDetails[0].Paymentschedule = result.PaymentIntervalsDetails1[i].Paymentschedule;
                        getMemberPlanDetails[0].NoofInstallments = result.PaymentIntervalsDetails1[i].NoofInstallments;
                        // IntervalsList.Add(intervals);
                    }
                }

                return getMemberPlanDetails;
            }
            catch (Exception ex)
            {

            }
            return getMemberPlanDetails;
        }

        public List<Member> GetFamilyDetails(int intMemberParentID)
        {

            List<Member> getFamilyDetails = new List<Member>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    getFamilyDetails = Context.Members.Where(m => m.MemberParentID == intMemberParentID).ToList();

                }
            }
            catch (Exception ex)
            {

            }
            return getFamilyDetails;

        }
        public List<PaymentDetail> GetPaymentDetails(int intMemberParentID)
        {

            List<PaymentDetail> getPaymentDetail = new List<PaymentDetail>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    getPaymentDetail = Context.PaymentDetails.Where(m => m.MemberParentID == intMemberParentID).ToList();

                }
            }
            catch (Exception ex)
            {

            }
            return getPaymentDetail;

        }

        public List<MemberPlan> GetMemberFamilyPlanDetails(int intMemberParentID, int PlanType)
        {

            List<MemberPlan> getMemberFamilyPlanDetails = new List<MemberPlan>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    getMemberFamilyPlanDetails = Context.MemberPlans.Where(m => (m.MemberParentID == intMemberParentID) && (m.PlanType == PlanType)).ToList();

                }
            }
            catch (Exception ex)
            {

            }
            return getMemberFamilyPlanDetails;

        }


        public List<MemberPlanMapping> GetFamilyPlanMemberDetails(int intMemberPlanID)
        {

            List<MemberPlanMapping> getMemberFamilyPlanDetails = new List<MemberPlanMapping>();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    getMemberFamilyPlanDetails = Context.MemberPlanMappings.Where(m => (m.MemberPlanID == intMemberPlanID)).ToList();

                }
            }
            catch (Exception ex)
            {

            }
            return getMemberFamilyPlanDetails;

        }

        public int CheckMemberExists(string FirstName, string LastName, string Gender, DateTime DOB, string MobileNumber)
        {
            

            int result = 0;
            try
            {
                List<Member> objMember = new List<Member>();
                using (var Context = new Dev_PPCPEntities(1))
                {
                    
                    objMember = Context.Members.Where(o => (o.FirstName == FirstName) &&
                           (o.LastName == LastName) &&
                           (o.Gender == Gender) &&
                           (o.DOB == DOB) &&
                           (o.MobileNumber == MobileNumber)).ToList();

                    result = objMember[0].MemberID;
                }
            }
            catch (Exception ex)
            {
                result = 0;
                return result;
            }
            return result;

        }



        public List<TemporaryMemberDetails> AddDoctorDetails(string xml)
        {
            List<TemporaryMemberDetails> objTemporaryDetails = new List<TemporaryMemberDetails>();
            string MobileNumber = "", CountryCode = "", Encryptpassword = "", UserPassword = "", UserName = "";
            using (var context = new DALMemberService())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                XmlNode node = xmlDoc.SelectSingleNode("/AddDoctor ");
                XmlNode node2 = xmlDoc.SelectSingleNode("/AddDoctor/Password");
                XmlNode node3 = xmlDoc.SelectSingleNode("/AddDoctor/UserName");
                if (node2 != null && node3 != null)
                {
                    UserName = node["UserName"].InnerText;
                    UserPassword = node["Password"].InnerText;
                    byte[] bytes = Encoding.UTF8.GetBytes(UserPassword);
                    Encryptpassword = Convert.ToBase64String(bytes);
                }
                MobileNumber = node["MobileNumber"].InnerText;
                CountryCode = node["CountryCode"].InnerText;
                if (Encryptpassword != "")
                {
                    SqlParameter XML = new SqlParameter("@XML", xml);
                    SqlParameter EncryptPassword = new SqlParameter("@EncryptPassword", Encryptpassword);
                    objTemporaryDetails = context.Database.SqlQuery<TemporaryMemberDetails>("Pr_AddDoctors @XML,@EncryptPassword", XML, EncryptPassword).ToList();
                    if (objTemporaryDetails.Count > 0 && MobileNumber != "")
                    {
                        string message = "Thank you for enrolling with PPCP. Your UserName : " + UserName + " and Password : " + UserPassword;
                        SendMessageByText(message, MobileNumber, CountryCode);
                    }
                }
                //else
                //{
                //    SqlParameter XML = new SqlParameter("@XML", xml);
                //    objTemporaryDetails = context.Database.SqlQuery<TemporaryMemberDetails>("Pr_AddDoctors @XML", XML).ToList();
                //}


                return objTemporaryDetails;
            }
            return objTemporaryDetails;
        }
        /// <summary>
        /// This web service is used for get the Stripe card details based on customerID
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <returns></returns>
        public List<stripeCardDetails> GetStripCardDetails(string CustomerID)
        {
            StripeMethods stripeMethods = new StripeMethods();
            List<stripeCardDetails> stripeCardDetails = new List<stripeCardDetails>();
            try
            {
                var cardDeails = stripeMethods.GetCardDetails(CustomerID);
                for (int i = 0; i < cardDeails.Count; i++)
                {
                    stripeCardDetails obj = new stripeCardDetails();
                    obj.Id = cardDeails[i].Id.ToString();
                    obj.Last4 = cardDeails[i].Last4.ToString();
                    obj.ExpirationMonth = cardDeails[i].ExpMonth.ToString();
                    obj.ExpirationYear = cardDeails[i].ExpYear.ToString();
                    obj.Name = cardDeails[i].Name.ToString();
                    obj.Brand = cardDeails[i].Brand.ToString();
                    obj.Fingerprint = cardDeails[i].Fingerprint.ToString();
                    stripeCardDetails.Add(obj);
                }
            }
            catch (Exception ex)
            {

                //System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
                //string str = trace.GetFrame(0).GetMethod().ReflectedType.FullName;
                //Err.ErrorsEntry(ex.ToString(), ex.Message, ex.GetHashCode(), str, trace.GetFrame(0).GetMethod().Name);
                return stripeCardDetails;
            }
            return stripeCardDetails;
        }


        public object GetMemberPlanAndPaymentsDetails(int intPlanCode)
        {
            using (var Context = new Dev_PPCPEntities(1))
            {
                var entryPoint = (from MP in Context.MemberPlans
                                  join PD in Context.PaymentDetails on MP.Plan_Code equals PD.Plan_Code
                                  join P in Context.Plans on MP.PlanID equals P.PlanID
                                  where MP.Plan_Code == intPlanCode
                                  select new
                                  {
                                      ProviderName = MP.ProviderName,
                                      PlanName = P.PlanName,
                                      TotalAmount = MP.TotalAmount,
                                      AmountPaid = PD.PaidAmount,
                                      DueAmount = PD.DueAmount,
                                      EnrollFee = P.EnrollFee,
                                      PaymentSchedule = MP.PaymentInterval,
                                      Duration = MP.Duration,
                                      PlanStatus = P.PlanStatus,
                                      PaymentDate = PD.CreatedDate,
                                      TransactionID = PD.TransactionID,
                                      ProviderID = MP.ProviderID,
                                      PlanStartDate = MP.PlanStartDate,
                                      PlanEndDate = MP.PlanEndDate,
                                      PlanCode = P.PlanCode,
                                      OrganizationID = MP.OrganizationID

                                  }).ToList();

                if (entryPoint.Count == 0)
                {
                    var entryPoin = (from MP in Context.MemberPlans
                                     join P in Context.Plans on MP.PlanID equals P.PlanID
                                     where MP.Plan_Code == intPlanCode
                                     select new
                                     {
                                         ProviderName = MP.ProviderName,
                                         PlanName = P.PlanName,
                                         TotalAmount = MP.TotalAmount,
                                         EnrollFee = P.EnrollFee,
                                         PaymentSchedule = MP.PaymentInterval,
                                         Duration = MP.Duration,
                                         PlanStatus = P.PlanStatus,
                                         AmountPaid = "0",
                                         PaymentDate = "",
                                         TransactionID = "0",
                                         DueAmount = "0",
                                         ProviderID = MP.ProviderID,
                                         PlanStartDate = MP.PlanStartDate,
                                         PlanEndDate = MP.PlanEndDate,
                                         PlanCode = P.PlanCode,
                                         OrganizationID=MP.OrganizationID
                                     }).ToList();
                    return entryPoin;
                }

                return entryPoint;
            }

        }

        public List<Result> UpdateTermsandConditions(int MemberID)
        {
            List<Result> obj = new List<Result>();
            Result res = new Result();
            try
            {
                using (var Context = new Dev_PPCPEntities(1))
                {
                    Member member = Context.Members.FirstOrDefault(m => m.MemberID == MemberID);
                    member.TandCAcceptedDate = DateTime.Parse(Convert.ToString(DateTime.Now), new CultureInfo("en-US"));
                    int Result = Context.SaveChanges();

                    res.ResultID = Result;
                    obj.Add(res);
                }
            }
            catch (Exception ex)
            {
                res.ResultName = ex.Message;
                obj.Add(res);
            }

            return obj;
        }

        public List<Result> UpdateFamilyMemberDetails(string xml)
        {

            List<Result> objTemporaryDetails = new List<Result>();
            Result res = new Result();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MemberDetails));
                StringReader rdr = new StringReader(xml);
                MemberDetails objMemberDetails = (MemberDetails)serializer.Deserialize(rdr);
                using (var context = new Dev_PPCPEntities(1))
                {
                    Member Member = context.Members.FirstOrDefault(m => m.MemberID == objMemberDetails.MemberID);
                    Member.RelationshipID = objMemberDetails.RelationshipID;
                    Member.RelationshipName = objMemberDetails.RelationshipName;
                    Member.FirstName = objMemberDetails.FirstName;
                    Member.LastName = objMemberDetails.LastName;
                    Member.Salutation = objMemberDetails.Salutation;
                    Member.DOB = DateTime.Parse(Convert.ToString(objMemberDetails.DOB), new CultureInfo("en-US"));
                    Member.Gender = Convert.ToString(objMemberDetails.Gender);
                    Member.CountryCode = objMemberDetails.CountryCode;
                    Member.MobileNumber = objMemberDetails.MobileNumber;
                    Member.Email = objMemberDetails.Email;
                    Member.CountryID = objMemberDetails.CountryID;
                    Member.CountryName = objMemberDetails.CountryName;
                    Member.StateID = objMemberDetails.StateID;
                    Member.StateName = objMemberDetails.StateName;
                    Member.CityID = objMemberDetails.CityID;
                    Member.CityName = objMemberDetails.CityName;
                    Member.Zip = objMemberDetails.Zip;
                    Member.ModifiedDate = DateTime.Parse(Convert.ToString(DateTime.Now), new CultureInfo("en-US"));
                    Member.ModifiedBy = objMemberDetails.LoginMemberID;
                    int Result = context.SaveChanges();
                    res.ResultID = Result;
                    objTemporaryDetails.Add(res);
                }
            }
            catch (Exception ex)
            {
                res.Exception = ex.Message;
                objTemporaryDetails.Add(res);
            }
            return objTemporaryDetails;
        }

        public List<PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlanInstallmentMapping> GetMemberPlanInstallments(int intMemberPlanID)
        {
            List<PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlanInstallmentMapping> getMemberPlanInstallments = new List<PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlanInstallmentMapping>();
            List<PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlanInstallmentMapping> getMemberPlanInstallments1 = new List<PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlanInstallmentMapping>();
            try
            {

                using (var context = new DALMemberService())
                {

                    SqlParameter PlanID = new SqlParameter("@MemberPlanID", intMemberPlanID);

                    getMemberPlanInstallments = context.Database.SqlQuery<PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlanInstallmentMapping>("Pr_GetMemberPlanInstallmentMapping @MemberPlanID", PlanID).ToList();
                }

                foreach (PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlanInstallmentMapping abc in getMemberPlanInstallments)
                {
                    PPCPWebApiServices.Models.PPCPWebService.DC.MemberPlanInstallmentMapping model = new DC.MemberPlanInstallmentMapping();
                    model.MemberPlanID = abc.MemberPlanID;
                    if (abc.PaymentDate == null)
                        model.PaymentDate1 = "";
                    else
                        model.PaymentDate1 = abc.PaymentDate1;
                    model.PaymentDetailsID = abc.PaymentDetailsID;
                    model.PaymentStatus = abc.PaymentStatus;
                    model.PaymentDueDate = abc.PaymentDueDate;
                    model.MemberPlanInstallmentID = abc.MemberPlanInstallmentID;
                    model.PaidAmount = abc.PaidAmount;
                    model.InstallmentAmount = abc.InstallmentAmount;
                    model.PaymentAmount = abc.PaymentAmount;
                    getMemberPlanInstallments1.Add(model);

                }
                //using (var Context = new Dev_PPCPEntities(1))
                //{
                //    getMemberPlanInstallments = Context.MemberPlanInstallmentMappings.Where(m => (m.MemberPlanID == intMemberPlanID)).ToList();

                //}

                //(x.UpdateDate.Value.Date.CompareTo(todayPlusConfigDays)==0)
                //       getMemberPlanInstallments.Where(usr=>usr.Age > 30)
                //.Select(usr => { usr.Name = usr.Name + "dedalyAlive"; return usr; })
                //Convert.ToBoolean(DateTime.Compare(Convert.ToDateTime(usr.PaymentDueDate), DateTime.Today)
                //.ToList();


                //uncomment to do installment amount =0 for future installemnt
                //     getMemberPlanInstallments.Where(usr => (usr.PaymentDueDate.Value.Date.CompareTo(DateTime.Today) == 1) && usr.PaymentStatus == "Pending")
                //.Select(usr => { usr.InstallmentAmount = 0; return usr; })
                //.ToList();

            }
            catch (Exception ex)
            {

            }
            return getMemberPlanInstallments1;
        }
    }
}