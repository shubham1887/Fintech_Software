using Project.DAL;
using Project.Processing;
using Project.Requests;
using Project.Response.TransactionWiseReport;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class TransactionWiseReportBAL
    {
        internal OpratorWiseDateWiseWrapper SelectOperatorWisesUserWiseReport(Project.Requests.TransactionWiseReport.TransactionWiseReportRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            OpratorWiseDateWiseWrapper Wrapper = new OpratorWiseDateWiseWrapper();
            List<OpratorWiseDateWiseViewModel> list = new TransactionWiseReportDAL().SelectOperatorWisesUserWiseReport(mainRequest, keyDataModel.UserID);

            if (list == null || list.Count < 0)
            {
                failureMessage = Message.TransactionMessage.NotFound;
                return Wrapper;
            }


            OpratorWiseDateWiseViewModel rangewise = new OpratorWiseDateWiseViewModel();
            decimal totalMobileSale = 0;
            decimal totalMoneyTransfer = 0;
            decimal totalDTHSale = 0;
            decimal totalPaymentGatewaySale = 0;
            decimal totalAepsSale = 0;
            decimal totalMAtm = 0;
            decimal totalElectricity = 0;
            decimal totalPostPaid = 0;
            decimal totalCreditCard = 0;
            decimal totalGeneralInsurance = 0;
            decimal totalFasTag = 0;
            decimal totalBroadband = 0;
            decimal totalGasCylinderBooking = 0;
            decimal totalWaterBill = 0;
            decimal totalPipedGas = 0;
            decimal totalPayout = 0;
            decimal totalUPITransfer = 0;
            decimal totalPOS = 0;
            decimal totalCMS = 0;
            decimal TotalService = 0;
            decimal totalExpressMoneyTransfer = 0;
            foreach (OpratorWiseDateWiseViewModel item in list)
            {
                totalMobileSale += item.MobileSale;
                totalMoneyTransfer += item.MoneyTransfer;
                totalDTHSale += item.DTHSale;
                totalPaymentGatewaySale += item.PaymentGatewaySale;
                totalAepsSale += item.AepsSale;
                totalCreditCard += item.CreditCard;
                totalGeneralInsurance += item.GeneralInsurance;
                totalFasTag += item.FasTag;
                totalBroadband += item.Broadband;
                totalGasCylinderBooking += item.GasCylinderBooking;
                totalWaterBill += item.WaterBill;
                totalPipedGas += item.PipedGas;
                totalMAtm += item.MAtm;
                totalElectricity += item.Electricity;
                totalPostPaid += item.PostPaid;
                totalPayout += item.Payout;
                totalUPITransfer += item.UPITransfer;
                totalPOS += item.POS;
                totalCMS += item.CMS;
                totalExpressMoneyTransfer += item.ExpressMoneyTransfer;
                TotalService += item.TotalService;
            }

            rangewise.DOC = "Total";
            rangewise.MobileSaleID = (int)ServiceCode.Mobile;
            rangewise.DTHSaleID = (int)ServiceCode.Dth;
            rangewise.MoneyTransferID = (int)ServiceCode.MoneyTransfer;
            rangewise.PaymentGatewaySaleID = (int)ServiceCode.PaymentGateway;
            rangewise.AepsSaleID = (int)ServiceCode.AEPS;
            rangewise.ElectricityID = (int)ServiceCode.Electricity;
            rangewise.PostPaidID = (int)ServiceCode.PostPaid;
            rangewise.MAtmID = (int)ServiceCode.MATM;
            rangewise.GeneralInsuranceID = (int)ServiceCode.GeneralInsurance;
            rangewise.BroadbandID = (int)ServiceCode.Broadband;
            rangewise.GasCylinderBookingID = (int)ServiceCode.GasCylinderBooking;
            rangewise.PipedGasID = (int)ServiceCode.PipedGas;
            rangewise.PayoutID = (int)ServiceCode.Payout;
            rangewise.UPITransferID = (int)ServiceCode.UPITransfer;
            rangewise.POSID = (int)ServiceCode.POS;
            rangewise.ExpressMoneyTransferID = (int)ServiceCode.ExpressMoneyTransfer;
            rangewise.WaterBillID = (int)ServiceCode.Waterbill;
            rangewise.FasTagID = (int)ServiceCode.Fasttag;
            rangewise.CreditCardID = (int)ServiceCode.CreditCardPayment;
            rangewise.CMSID = (int)ServiceCode.CMS;
            rangewise.MobileSale = totalMobileSale;
            rangewise.MoneyTransfer = totalMoneyTransfer;
            rangewise.DTHSale = totalDTHSale;
            rangewise.PaymentGatewaySale = totalPaymentGatewaySale;
            rangewise.AepsSale = totalAepsSale;
            rangewise.CreditCard = totalCreditCard;
            rangewise.GeneralInsurance = totalGeneralInsurance;
            rangewise.FasTag = totalFasTag;
            rangewise.Broadband = totalBroadband;
            rangewise.GasCylinderBooking = totalGasCylinderBooking;
            rangewise.WaterBill = totalWaterBill;
            rangewise.PipedGas = totalPipedGas;
            rangewise.MAtm = totalMAtm;
            rangewise.Electricity = totalElectricity;
            rangewise.PostPaid = totalPostPaid;
            rangewise.Payout = totalPayout;
            rangewise.UPITransfer = totalUPITransfer;
            rangewise.POS = totalPOS;
            rangewise.CMS = totalCMS;
            rangewise.ExpressMoneyTransfer = totalExpressMoneyTransfer;
            rangewise.TotalService = TotalService;
            list.Add(rangewise);




            if (mainRequest.ExportType == "PDF")
            {
                string FileName = "OpratorWiseUserWise_Report";
                Wrapper.IsDownload = true;
                Wrapper.ReportPath = new ExcelProcessing().GeneratePdf(FileName, list, keyDataModel.UserID);

            }
            if (mainRequest.ExportType == "EXCEL")
            {
                string FileName = "OpratorWiseUserWise_Report";
                Wrapper.IsDownload = true;
                Wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, list, keyDataModel.UserID);
            }
            Wrapper.list = list;
            failureMessage = StaticModel.StatusType.Ok;
            return Wrapper;
        }
        internal UserWiseOpratorWiseWrapper SelectUserWiseOperatorWiseSaleReport(Project.Requests.TransactionWiseReport.TransactionWiseReportRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            UserWiseOpratorWiseWrapper Wrapper = new UserWiseOpratorWiseWrapper();
            List<UserWiseOpratorWiseViewModel> list = new List<UserWiseOpratorWiseViewModel>();

            list = new TransactionWiseReportDAL().SelectUserWiseOperatorWiseSaleReport(mainRequest, keyDataModel.UserID);



            if (list == null || list.Count < 0)
            {
                failureMessage = Message.TransactionMessage.NotFound;
                return Wrapper;
            }
            Wrapper.Count = list.Count();

            if (!Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                list = list.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }
            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            UserWiseOpratorWiseViewModel rangewise = new UserWiseOpratorWiseViewModel();
            decimal totalMobileSale = 0;
            decimal totalMoneyTransfer = 0;
            decimal totalDTHSale = 0;
            decimal totalPaymentGatewaySale = 0;
            decimal totalAepsSale = 0;
            decimal totalMAtm = 0;
            decimal totalElectricity = 0;
            decimal totalPostPaid = 0;
            decimal totalCreditCard = 0;
            decimal totalGeneralInsurance = 0;
            decimal totalFasTag = 0;
            decimal totalBroadband = 0;
            decimal totalPipedGas = 0;
            decimal totalPayout = 0;
            decimal totalUPITransfer = 0;
            decimal totalPOS = 0;
            decimal totalCMS = 0;
            decimal TotalService = 0;
            decimal totalExpressMoneyTransfer = 0;
            decimal totalPastMobileSale = 0;
            decimal totalPastMoneyTransfer = 0;
            decimal totalPastDTHSale = 0;
            decimal totalPastPaymentGatewaySale = 0;
            decimal totalPastAepsSale = 0;
            decimal totalPastMAtm = 0;
            decimal totalPastElectricity = 0;
            decimal totalPastPostPaid = 0;
            decimal totalPastCreditCard = 0;
            decimal totalPastGeneralInsurance = 0;
            decimal totalPastFasTag = 0;
            decimal totalPastBroadband = 0;
            decimal totalPastPipedGas = 0;
            decimal totalPastPayout = 0;
            decimal totalPastUPITransfer = 0;
            decimal totalPastPOS = 0;
            decimal totalPastCMS = 0;
            decimal TotalPastService = 0;
            decimal totalPastExpressMoneyTransfer = 0;
            foreach (UserWiseOpratorWiseViewModel item in list)
            {
                totalMobileSale += item.TodayMobileSale;
                totalMoneyTransfer += item.TodayMoneyTransfer;
                totalDTHSale += item.TodayDTHSale;
                totalPaymentGatewaySale += item.TodayPaymentGatewaySale;
                totalAepsSale += item.TodayAepsSale;
                totalCreditCard += item.TodayCreditCard;
                totalGeneralInsurance += item.TodayGeneralInsurance;
                totalFasTag += item.TodayFasTag;
                totalBroadband += item.TodayBroadband;

                totalPipedGas += item.TodayPipedGas;
                totalMAtm += item.TodayMAtm;
                totalElectricity += item.TodayElectricity;
                totalPostPaid += item.TodayPostPaid;
                totalPayout += item.TodayPayout;
                totalUPITransfer += item.TodayUPITransfer;
                totalPOS += item.TodayPOS;
                totalCMS += item.TodayCMS;
                totalExpressMoneyTransfer += item.TodayExpressMoneyTransfer;
                totalPastMobileSale += item.YesterdayMobileSale;
                totalPastMoneyTransfer += item.YesterdayMoneyTransfer;
                totalPastDTHSale += item.YesterdayDTHSale;
                totalPastPaymentGatewaySale += item.YesterdayPaymentGatewaySale;
                totalPastAepsSale += item.YesterdayAepsSale;
                totalPastCreditCard += item.YesterdayCreditCard;
                totalPastGeneralInsurance += item.YesterdayGeneralInsurance;
                totalPastFasTag += item.YesterdayFasTag;
                totalPastBroadband += item.YesterdayBroadband;

                totalPastPipedGas += item.YesterdayPipedGas;
                totalPastMAtm += item.YesterdayMAtm;
                totalPastElectricity += item.YesterdayElectricity;
                totalPastPostPaid += item.YesterdayPostPaid;
                totalPastPayout += item.YesterdayPayout;
                totalPastUPITransfer += item.YesterdayUPITransfer;
                totalPastPOS += item.YesterdayPOS;
                totalPastCMS += item.YesterdayCMS;
                totalPastExpressMoneyTransfer += item.YesterdayExpressMoneyTransfer;
                TotalPastService += item.PastTotalService;
                TotalService += item.TodayTotalService;
            }

            rangewise.FirmName = "Total";
            rangewise.MobileSaleID = (int)ServiceCode.Mobile;
            rangewise.DTHSaleID = (int)ServiceCode.Dth;
            rangewise.MoneyTransferID = (int)ServiceCode.MoneyTransfer;
            rangewise.PaymentGatewaySaleID = (int)ServiceCode.PaymentGateway;
            rangewise.AepsSaleID = (int)ServiceCode.AEPS;
            rangewise.ElectricityID = (int)ServiceCode.Electricity;
            rangewise.PostPaidID = (int)ServiceCode.PostPaid;
            rangewise.MAtmID = (int)ServiceCode.MATM;
            rangewise.GeneralInsuranceID = (int)ServiceCode.GeneralInsurance;
            rangewise.BroadbandID = (int)ServiceCode.Broadband;
            rangewise.GasCylinderBookingID = (int)ServiceCode.GasCylinderBooking;
            rangewise.PipedGasID = (int)ServiceCode.PipedGas;
            rangewise.PayoutID = (int)ServiceCode.Payout;
            rangewise.UPITransferID = (int)ServiceCode.UPITransfer;
            rangewise.POSID = (int)ServiceCode.POS;
            rangewise.ExpressMoneyTransferID = (int)ServiceCode.ExpressMoneyTransfer;
            rangewise.WaterBillID = (int)ServiceCode.Waterbill;
            rangewise.FasTagID = (int)ServiceCode.Fasttag;
            rangewise.CreditCardID = (int)ServiceCode.CreditCardPayment;
            rangewise.CMSID = (int)ServiceCode.CMS;
            rangewise.TodayMobileSale = totalMobileSale;
            rangewise.TodayMoneyTransfer = totalMoneyTransfer;
            rangewise.TodayDTHSale = totalDTHSale;
            rangewise.TodayPaymentGatewaySale = totalPaymentGatewaySale;
            rangewise.TodayAepsSale = totalAepsSale;
            rangewise.TodayCreditCard = totalCreditCard;
            rangewise.TodayGeneralInsurance = totalGeneralInsurance;
            rangewise.TodayFasTag = totalFasTag;
            rangewise.TodayBroadband = totalBroadband;

            rangewise.TodayPipedGas = totalPipedGas;
            rangewise.TodayMAtm = totalMAtm;
            rangewise.TodayElectricity = totalElectricity;
            rangewise.TodayPostPaid = totalPostPaid;
            rangewise.TodayPayout = totalPayout;
            rangewise.TodayUPITransfer = totalUPITransfer;
            rangewise.TodayPOS = totalPOS;
            rangewise.TodayCMS = totalCMS;
            rangewise.TodayExpressMoneyTransfer = totalExpressMoneyTransfer;
            rangewise.YesterdayMobileSale = totalPastMobileSale;
            rangewise.YesterdayMoneyTransfer = totalPastMoneyTransfer;
            rangewise.YesterdayDTHSale = totalPastDTHSale;
            rangewise.YesterdayPaymentGatewaySale= totalPastPaymentGatewaySale;
            rangewise.YesterdayAepsSale = totalPastAepsSale;
            rangewise.YesterdayCreditCard = totalPastCreditCard;
            rangewise.YesterdayGeneralInsurance = totalPastGeneralInsurance;
            rangewise.YesterdayFasTag = totalPastFasTag;
            rangewise.YesterdayBroadband = totalPastBroadband;

            rangewise.YesterdayPipedGas= totalPastPipedGas;
            rangewise.YesterdayMAtm = totalPastMAtm;
            rangewise.YesterdayElectricity = totalPastElectricity;
            rangewise.YesterdayPostPaid = totalPastPostPaid;
            rangewise.YesterdayPayout = totalPastPayout;
            rangewise.YesterdayUPITransfer = totalPastUPITransfer;
            rangewise.YesterdayPOS = totalPastPOS;
            rangewise.YesterdayCMS = totalPastCMS;
            rangewise.YesterdayExpressMoneyTransfer = totalPastExpressMoneyTransfer;
            rangewise.PastTotalService = TotalPastService;
            rangewise.TodayTotalService = TotalService;
            list.Add(rangewise);



            if (mainRequest.ExportType == "PDF")
            {
                string FileName = "UserWiseServiceWiseSale_Report";
                Wrapper.IsDownload = true;
                Wrapper.ReportPath = new ExcelProcessing().GeneratePdf(FileName, list, keyDataModel.UserID);

            }
            if (mainRequest.ExportType == "EXCEL")
            {
                string FileName = "UserWiseServiceWiseSale_Report";
                Wrapper.IsDownload = true;
                Wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, list, keyDataModel.UserID);
            }
            Wrapper.list = list;
            failureMessage = StaticModel.StatusType.Ok;
            return Wrapper;
        }

    }
}
