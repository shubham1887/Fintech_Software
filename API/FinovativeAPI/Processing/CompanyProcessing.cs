using Project.DAL;
using Project.Model;
using Project.StaticModel;
using System;

namespace Project.Processing
{
    public class CompanyProcessing
    {
        public void SelectCompanyDetails()
        {
            try
            {
                Company.ClientName = GetStringValue("ClientName");

                Company.WebAPIURL = GetStringValue("WebAPIURL");

                Company.UIURL = GetStringValue("UIURL");

                Company.DefaultSlabName = GetStringValue("DefaultSlabName");

                Company.XMPPDomain = GetStringValue("XMPPDomain");

                Company.XMPPWebSiteUserName = GetStringValue("XMPPWebSiteUserName");

                Company.XMPPWebSiteUserPassword = GetStringValue("XMPPWebSiteUserPassword");

                Company.XMPPOtomaxUser = GetStringValue("XMPPOtomaxUser");

                Company.ConsistyLoginToken = GetStringValue("ConsistyLoginToken");

                Company.CommissionOnTransfer = GetStringValueWithDefaultValue("CommissionOnTransfer", "No");

                Company.AdminUserID = GetIntegerValue("AdminUserID");

                Company.DefaultSlabID = GetIntegerValue("DefaultSlabID");

                Company.XMPPProviderID = GetIntegerValue("XMPPProviderID");

                Company.NEFTProviderID = GetIntegerValue("NEFTProviderID");

                Company.RazorPayProviderID = GetIntegerValue("RazorPayProviderID");

                Company.WorldLineProviderID = GetIntegerValue("WorldLineProviderID");

                Company.FireAndForgetRechargeEarly = GetStringValueWithDefaultValue("FireAndForgetRechargeEarly", "No");

                Company.PaymentGatewayKYC = GetStringValueWithDefaultValue("PaymentGatewayKYC", "Yes");

                Company.XMPPResend = GetStringValueWithDefaultValue("XMPPResend", "No");

                Company.PaySwiffProviderID = GetIntegerValue("PaySwiffProviderID");

                Company.BsnlOperatorID = GetIntegerValue("BsnlOperatorID");

                Company.BsnlSpecialOperatorID = GetIntegerValue("BsnlSpecialOperatorID");

                Company.OverDraft = GetStringValueWithDefaultValue("OverDraft", "No");

                Company.IsW2OFundTransfer = GetStringValueWithDefaultValue("IsW2OFundTransfer", "No");

                Company.IsWhiteLabel = GetStringValueWithDefaultValue("IsWhiteLabel", "No");

                Company.NameDisplayFormat = GetStringValueWithDefaultValue("NameDisplayFormat", "");

                Company.GSTHold = GetStringValueWithDefaultValue("GSTHold", "No");

                Company.IsPendingProvider = GetStringValueWithDefaultValue("IsPendingProvider", "No");

                Company.CommissionHold = GetStringValueWithDefaultValue("CommissionHold", "No");

                Company.OutsideRoleKYCForMobileDTH = GetStringValueWithDefaultValue("OutsideRoleKYCForMobileDTH", "Yes");

                Company.PGCommissionWithDownline = GetStringValueWithDefaultValue("PGCommissionWithDownline", "Yes");
                Company.IsUplineActive = GetStringValueWithDefaultValue("IsUplineActive", "No");

              
                Company.YesBankProviderID = GetIntegerValue("YesBankProviderID");

              
                Company.ArchiveDataInDays = GetIntegerValue("ArchiveDataInDays");
               
                Company.PaySwiffProviderID = GetIntegerValue("PaySwiffProviderID");

                Company.AutoBalanceTransfer = GetStringValueWithDefaultValue("AutoBalanceTransfer", "No");
                Company.CallbackStatusChangeAllowed = GetStringValueWithDefaultValue("CallbackStatusChangeAllowed", "No");

                Company.FreeUPIAutoSuccess = GetStringValueWithDefaultValue("FreeUPIAutoSuccess", "No");
                Company.PayoutAccountApprovalRequired = GetStringValueWithDefaultValue("PayoutAccountApprovalRequired", "Yes");
                Company.BlankTransactionIDAllowed = GetStringValueWithDefaultValue("BlankTransactionIDAllowed", "No");
                Company.AutoEnableUser = GetStringValueWithDefaultValue("AutoEnableUser", "Yes");
                Company.BackDateFundTransfer = GetStringValueWithDefaultValue("BackDateFundTransfer", "No");
                Company.IsLastBalanceChecking = GetStringValueWithDefaultValue("IsLastBalanceChecking", "Yes");
                Company.ServicewiseSubscription = GetStringValueWithDefaultValue("ServicewiseSubscription", "No");
                Company.PlanApiForAPIUser = GetStringValueWithDefaultValue("PlanApiForAPIUser", "No");
                Company.IsCreditInstitution = GetStringValueWithDefaultValue("IsCreditInstitution", "No");
                Company.IsFintechLeadGeneration = GetStringValueWithDefaultValue("IsFintechLeadGeneration", "No");
                Company.BillFetchValidation = GetStringValueWithDefaultValue("BillFetchValidation", "No");
                Company.CallbackStatusSuccessToFailOnly = GetStringValueWithDefaultValue("CallbackStatusSuccesstoFailOnly", "No");
                Company.IsPGSettlementHold = GetStringValueWithDefaultValue("IsPGSettlementHold", "No");
                Company.CCFMinimumAmount = GetdecimalValueWithDefaultValue("CCFMinimumAmount", 12);
                Company.CCFInPercentage = GetdecimalValueWithDefaultValue("CCFInPercentage", 1.2M);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            } 
        }

        private string GetStringValue(string key)
        {
            CompanyMaster ObjModel = new CompanyMasterDal().SelectByName(key);

            if (ObjModel == null)
                return "";

            return ObjModel.Value;
        }

        private string GetStringValueWithDefaultValue(string key, string defaultValue)
        {
            CompanyMaster ObjModel = new CompanyMasterDal().SelectByName(key);

            if (ObjModel == null)
                return defaultValue;

            return ObjModel.Value;
        }

        private bool GetBooleanValue(string key)
        {
            CompanyMaster ObjModel = new CompanyMasterDal().SelectByName(key);

            if (ObjModel == null)
                return true;

            if (ObjModel.Value.EqualsIgnoreCase("true"))
                return true;

            if (ObjModel.Value.EqualsIgnoreCase("false"))
                return false;

            return true;
        }

        private int GetIntegerValue(string key)
        {
            int retInt = 0;

            try
            {
                CompanyMaster ObjModel = new CompanyMasterDal().SelectByName(key);

                if (ObjModel == null)
                {
                    return 0;
                }
                retInt = Convert.ToInt32(ObjModel.Value);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return 0;
            }
            return retInt;
        }
        private decimal GetdecimalValueWithDefaultValue(string key, decimal defaultValue)
        {
            decimal retdecimal = 0;
            try
            {
                CompanyMaster ObjModel = new CompanyMasterDal().SelectByName(key);
                if (ObjModel == null)
                {
                    return defaultValue;
                }
                retdecimal = Convert.ToDecimal(ObjModel.Value);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return defaultValue;
            }
            return retdecimal;
        }
    }
}