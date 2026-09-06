using Project.BAL;
using Project.DAL;
using Project.Model;
using Project.Response.Tax;
using Project.StaticModel;
using System;

namespace Project.Processing
{
    public class TaxProcessing
    {
        public decimal getCCF(decimal amount)
        {
            decimal CCFPercentage = GetCCFPercentage();
            decimal CCFMinimumAmount = Company.CCFMinimumAmount;
            decimal ccf = amount * CCFPercentage;
            if (ccf <= 12)
            {
                return CCFMinimumAmount;
            }
            return ccf;
        }
        private decimal GetCCFPercentage()
        {
            decimal Percentage = 0;
            decimal CCFInPercentage = Company.CCFInPercentage;

            Percentage = CCFInPercentage / 100;
            return Percentage;
        }

        private decimal GetCashBackValue(int UserID, int Amount, int OperatorID, decimal CCFAmount)
        {
            decimal CashBack = 0;

            Response.Slab.DetailViewModel slabDetailViewModel = new Response.Slab.DetailViewModel();
            slabDetailViewModel = new SlabDetailBAL().GetSetCommission(UserID, Amount, OperatorID);
            if (slabDetailViewModel == null)
                return CashBack;

            CashBack = CalculateCashBack(slabDetailViewModel.RetailerComm, slabDetailViewModel.RetailerCommType, slabDetailViewModel.RetailerCommissionCategory, CCFAmount);

            return CashBack;

        }
        public decimal CalculateCashBack(decimal comm, string commType, string commissionCategory, decimal CCFAmount)
        {
            // comm always +ve
            if (comm == 0)
                return comm;

            if (comm <= 0.000001M)
                commissionCategory = CommissionCategory.Surcharge;

            if (commissionCategory.EqualsIgnoreCase(CommissionCategory.Surcharge))
            {
                comm = Math.Abs(comm) * -1;
            }

            if (commType.EqualsIgnoreCase(CommissionType.Fix))
                return comm;

            if (!commType.EqualsIgnoreCase(CommissionType.Flexi))
                return comm;

            decimal flexi = CCFAmount * comm;

            comm = flexi;

            return comm;
        }

        public decimal GetTDSOnCommission(decimal Commission)
        {
            decimal TDS = 0;

            if (Commission <= 0)
            {
                return TDS;
            }

            bool IsTDSOn = new SettingMasterBal().GetTDSOn();

            if (IsTDSOn == false)
            {
                return TDS;
            }

            TDS = Commission * TaxInfo.TDS;

            return TDS;
        }

        public decimal GetAdminGSTOnCommission(decimal RCommission, decimal DCommission, decimal SDCommission)
        {
            decimal GST = 0;

            // we will test and open
            if (RCommission >= 0)
            {
                return GST;
            }

            bool IsGSTOn = new SettingMasterBal().GetGSTOn();

            if (IsGSTOn == false)
            {
                return GST;
            }

            GST = Math.Abs(RCommission + DCommission + SDCommission) * TaxInfo.GST;

            return GST;
        }

        public decimal GetTDS(decimal Commission, out string TaxDescrption)
        {
            TaxDescrption = "";
            decimal TDS = 0;

            if (Commission <= 0)
                return TDS;
            
            decimal TdsPercentage = SelectPrepaidTdsPercentage();
            if (TdsPercentage <= 0)
                return TDS;
            TDS = Commission * TdsPercentage;

            TaxDescrption = $",TDS:{TDS.TwoDigitDecimal()})";

            return TDS;
        }

        internal decimal GetGSTForDMTForDistAndSuperDist(decimal Commission, out string TaxDescrption)
        {
            decimal GST = 0;
            TaxDescrption = "";
            if (Commission >= 0.00001M)
            {
                TaxDescrption = $"(Commission:{Commission.TwoDigitDecimal()}, GST:{GST.TwoDigitDecimal()}";
                return GST;
            }

            GST = Math.Abs(Commission) * TaxInfo.Inclusion18Percentage;
            TaxDescrption = $"(Commission:{Commission.TwoDigitDecimal()}, GST:{GST.TwoDigitDecimal()}";
            return GST;

        }

        internal decimal GetGSTForDMTForReatiler(decimal splitAmount, decimal RCommission, int ServiceID, out string TaxDescrption)
        {
            decimal GST = 0;
            TaxDescrption = "";
            if (ServiceID == (int)ServiceCode.MoneyTransfer)
            {
                if(splitAmount<=1)
                {
                    decimal GstAmount=splitAmount + Math.Abs(RCommission);
                    GST = GstAmount * TaxInfo.Inclusion18Percentage;
                }

                decimal CCFValue = getCCF(splitAmount);
                GST = CCFValue * TaxInfo.Inclusion18Percentage;
                TaxDescrption = $"(Charge:{CCFValue.TwoDigitDecimal()}, Comm:{RCommission.TwoDigitDecimal()}, GST:{GST.TwoDigitDecimal()}";
                return GST;

            }
            if (RCommission >= 0.00001M)
                return GST;
            GST = Math.Abs(RCommission) * TaxInfo.Inclusion18Percentage;
            TaxDescrption = $"(Commission:{RCommission.TwoDigitDecimal()}, GST:{GST.TwoDigitDecimal()}";

            return GST;
        }

        public GSTAndTDSViewModel GetCalculateGSTandTDS(decimal Amount, decimal Retailercomm, decimal DistributerCommi, decimal SuperDistributerCommi, int ProviderID, int UserID, int DistributerUserID, int SuperDistributerUserID, int ServiceID)
        {
            GSTAndTDSViewModel viewModel = new GSTAndTDSViewModel();
            string retTaxDescrption = "";
            try
            {
                viewModel.RetailerGST = GetGSTForDMTForReatiler(Amount, Retailercomm, ServiceID, out retTaxDescrption);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                viewModel.RetailerGST = 0;
            }
            viewModel.RetTaxDescrption = retTaxDescrption;
            try
            {
                viewModel.RetailerTDS = GetTDS(Retailercomm, out retTaxDescrption);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                viewModel.RetailerTDS = 0;
            }
            string distTaxDescrption = "";
            try
            {
                viewModel.DistGST = GetGSTForDMTForDistAndSuperDist(DistributerCommi, out distTaxDescrption);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                viewModel.DistGST = 0;
            }
            try
            {
               
                viewModel.DistTDS = GetTDS(DistributerCommi, out distTaxDescrption);
               
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                viewModel.DistTDS = 0;
            }

            string superDistTaxDescrption = "";
            try
            {
                viewModel.SuperDistGST = GetGSTForDMTForDistAndSuperDist(SuperDistributerCommi, out superDistTaxDescrption);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                viewModel.SuperDistGST = 0;
            }
            try
            {
                viewModel.SuperDistTDS = GetTDS(SuperDistributerCommi, out superDistTaxDescrption);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                viewModel.SuperDistTDS = 0;
            }
            viewModel.RetTaxDescrption = $"{viewModel.RetTaxDescrption}{retTaxDescrption}";
            viewModel.DistTaxDescrption = distTaxDescrption;
            viewModel.SuperDistTaxDescrption = superDistTaxDescrption;
           
            return viewModel;
        }
        private decimal SelectPrepaidTdsPercentage()
        {
            decimal PrepaidTdsPercentage = new SettingMasterBal().SelectByKeyWithDecimalDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.PrepaidTdsPercentage), 0);

            if (PrepaidTdsPercentage <= 0M)
                return PrepaidTdsPercentage = 0;
            PrepaidTdsPercentage = PrepaidTdsPercentage / 100;
            return PrepaidTdsPercentage;
        }
    }
}