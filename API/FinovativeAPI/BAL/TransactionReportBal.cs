using Projecdt.Response.TransactionReport;
using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.Transaction;
using Project.Requests.TransactionReport;
using Project.Response.TransactionMaster;
using Project.Response.TransactionReport;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class TransactionReportBAL
    {
        internal AEPSReportWrapper SelectForMicroATMReport(AEPSReport mainRequest, int UserID, out string failureMessage)
        {
            AEPSReportWrapper wrapper = new AEPSReportWrapper();

            List<AEPSViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<MATMExcelViewModel> ExcellistMATM = new List<MATMExcelViewModel>();
            try
            {
                if (mainRequest.IsExcelDownload)
                {
                    ExcellistMATM = new TransactionReportDAL().SelectForMicroATMReportExcelDownload(mainRequest, UserID);
                    wrapper.Count = ExcellistMATM.Count();

                    //wrapper.ExcellistMATM = ExcellistMATM;

                    wrapper.IsExcelDownload = mainRequest.IsExcelDownload;

                    wrapper.ExcellistMATM = ExcellistMATM;

                    failureMessage = StatusType.Ok;
                    return wrapper;


                    /*list = new TransactionReportDAL().SelectForMicroATMReportExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();*/
                }
                else
                {
                    list = new TransactionReportDAL().SelectForMicroATMReport(mainRequest, UserID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().SelectForMicroATMCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0 && mainRequest.IsExcelDownload == false)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().MicroATMHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();
            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            if (UserID != Company.AdminUserID)
            {
                foreach (AEPSViewModel item in list)
                {
                    item.ProviderName = "";
                }
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal AEPSReportWrapper SelectForCMSReport(AEPSReport mainRequest, int UserID, out string failureMessage)
        {
            AEPSReportWrapper wrapper = new AEPSReportWrapper();

            List<AEPSViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if (mainRequest.IsExcelDownload)
                {
                    list = new TransactionReportDAL().SelectForCMSReportExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForCMSReport(mainRequest, UserID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().SelectForCMSCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0 && mainRequest.IsExcelDownload == false)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().CMSHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();
            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            if (UserID != Company.AdminUserID)
            {
                foreach (AEPSViewModel item in list)
                {
                    item.ProviderName = "";
                }
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal AEPSReportWrapper SelectForPOSReport(AEPSReport mainRequest, int UserID, out string failureMessage)
        {
            AEPSReportWrapper wrapper = new AEPSReportWrapper();

            List<AEPSViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if (mainRequest.IsExcelDownload)
                {
                    list = new TransactionReportDAL().SelectForPOSReportExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForPOSReport(mainRequest, UserID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().SelectForPOSCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0 && mainRequest.IsExcelDownload == false)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().POSHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();
            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            if (UserID != Company.AdminUserID)
            {
                foreach (AEPSViewModel item in list)
                {
                    item.ProviderName = "";
                }
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal AEPSReportWrapper SelectForAEPSReport(AEPSReport mainRequest, int UserID, out string failureMessage)
        {
            AEPSReportWrapper wrapper = new AEPSReportWrapper();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID == (int)UserType.DFOS || UserTypeID == (int)UserType.SDFOS)
            {
                int ParentID = new UserDAL().SelectParentID(UserID);
                UserID = ParentID;
            }

            List<AEPSViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            validateSkipAndTakeCount(mainRequest);
            isExcelOrPDF(mainRequest);

            List<AEPSExcelViewModel> Excellist = new List<AEPSExcelViewModel>();
            try
            {
                if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload) || new Validation().IsValidString(mainRequest.CriteriaText))
                {
                    Excellist = new TransactionReportDAL().SelectForAEPSExcelDownloadReport(mainRequest, UserID);
                    wrapper.Count = Excellist.Count();

                    wrapper.Excellist = Excellist;

                    if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
                    {
                        wrapper.IsDownload = true;
                    }
                    wrapper.Excellist = Excellist;

                    failureMessage = StatusType.Ok;
                    return wrapper;
                }
                else
                {
                    list = new TransactionReportDAL().SelectForAEPSReport(mainRequest, UserID);
                    
                }
            }

            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0 && mainRequest.IsExcelDownload == false)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().AEPSHeaderSummary(list, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();
            wrapper.Count = list.Count();
            if (!Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                list = list.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });

            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.Transaction.AEPSReport request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }

            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal CashDepositWrapper SelectForCashDepositReport(AEPSReport mainRequest, int UserID, out string failureMessage)
        {
            CashDepositWrapper wrapper = new CashDepositWrapper();

            List<CashDepositViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if (mainRequest.IsExcelDownload)
                {
                    list = new TransactionReportDAL().SelectForCashDepositReportExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForCashDepositReport(mainRequest, UserID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().SelectForCashDepositCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().CashDepositHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            string ShowCommission = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission));

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (CashDepositViewModel item in list) item.RetailerComm = item.DistributerComm = item.SuperDistributerComm = 0;
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal List<CommissionViewModel> SelectRoleWithCommission(CommissionRequest selectRequest, out string failureMessage)
        {
            List<CommissionViewModel> report = null;
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return report;

            failureMessage = Messages.DataNotFound;
            try
            {
                report = new TransactionDAL().SelectRoleWithCommission(selectRequest.TransactionMasterID, selectRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (report == null || report.Count <= 0) return report;
            foreach (CommissionViewModel item in report)
            {
                if (item.DistributorID == Company.AdminUserID)
                {
                    item.DistributorName = "Admin";
                    item.SuperDistributorName = "Admin";
                    item.UserType = "Admin";
                }
                if (item.SuperDistributorID == Company.AdminUserID)
                {
                    item.UserType = "Admin";
                    item.SuperDistributorName = "Admin";
                }
            }
            failureMessage = StatusType.Ok;
            return report;
        }

        internal PayoutReportWrapper SelectForWalleTopUpReport(PayoutReport mainRequest, int UserID, out string failureMessage)
        {
            PayoutReportWrapper wrapper = new PayoutReportWrapper();

            List<ViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if (mainRequest.IsExcelDownload)
                {
                    list = new TransactionReportDAL().SelectForWalleTopUpExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForWalleTopUpReport(mainRequest, UserID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().SelectForWalleTopUpCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().WalleTopUpHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            string ShowCommission = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission));

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (ViewModel item in list) item.RetailerCommission = item.DistributerCommission = item.SuperDistributerCommission = 0;
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };
            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal PanCardReportWrapper SelectForPanCardReport(PanCardReportRequest mainRequest, int UserID, out string failureMessage)
        {
            PanCardReportWrapper wrapper = new PanCardReportWrapper();

            List<ViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
                {
                    list = new TransactionReportDAL().SelectForPanCardExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForPanCardReport(mainRequest, UserID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().SelectForPanCardCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().PanCardHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            string ShowCommission = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission));

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (ViewModel item in list) item.RetailerCommission = item.DistributerCommission = item.SuperDistributerCommission = 0;
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };
            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });
            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }

            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.TransactionReport.PanCardReportRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal UPITransferReportWrapper SelectForUPIReport(UPITransferReportRequest mainRequest, int UserID, out string failureMessage)
        {
            UPITransferReportWrapper wrapper = new UPITransferReportWrapper();

            List<ViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if ((Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload)) || new Validation().IsValidString(mainRequest.CriteriaText))
                {
                    list = new TransactionReportDAL().SelectForUPITransferReportExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForUPITransferReport(mainRequest, UserID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().SelectForUPITransferCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().UPITransferHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            string ShowCommission = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission));

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (ViewModel item in list) item.RetailerCommission = item.DistributerCommission = item.SuperDistributerCommission = 0;
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };
            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });

            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }


            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.Transaction.UPITransferReportRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }

            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal OTTTransferReportWrapper SelectForOTTReport(OTTTransferReportRequest mainRequest, int UserID, out string failureMessage)
        {
            OTTTransferReportWrapper wrapper = new OTTTransferReportWrapper();

            List<ViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if (mainRequest.IsExcelDownload || new Validation().IsValidString(mainRequest.CriteriaText))
                {
                    list = new TransactionReportDAL().SelectForOTTTransferReportExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForOTTTransferReport(mainRequest, UserID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().SelectForOTTTransferCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().OTTTransferHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            string ShowCommission = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission));

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (ViewModel item in list) item.RetailerCommission = item.DistributerCommission = item.SuperDistributerCommission = 0;
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };
            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal BusBookingTransferReportWrapper SelectForBusBookingReport(BusBookingTransferReportRequest mainRequest, int UserID, out string failureMessage)
        {
            BusBookingTransferReportWrapper wrapper = new BusBookingTransferReportWrapper();

            List<ViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload) || new Validation().IsValidString(mainRequest.CriteriaText))
                {
                    list = new TransactionReportDAL().SelectForBusBookingTransferReportExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForBusBookingTransferReport(mainRequest, UserID);
                    // if (mainRequest.SkipCount <= 0)
                    wrapper.Count = new TransactionReportDAL().SelectForBusBookingTransferCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().BusBookingTransferHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            string ShowCommission = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission));

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (ViewModel item in list) item.RetailerCommission = item.DistributerCommission = item.SuperDistributerCommission = 0;
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };
            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });
            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }

            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.Transaction.BusBookingTransferReportRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal AccountOpeningReportWrapper SelectAccountOpening(AccontOpeningRequest mainRequest, int UserID, out string failureMessage)
        {
            AccountOpeningReportWrapper wrapper = new AccountOpeningReportWrapper();

            List<ViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if (mainRequest.IsExcelDownload)
                {
                    list = new TransactionReportDAL().SelectForAccountOpeningReportExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForAccountOpeningReport(mainRequest, UserID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().SelectForAccountOpeningCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            string ShowCommission = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission));

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (ViewModel item in list) item.RetailerCommission = item.DistributerCommission = item.SuperDistributerCommission = 0;
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };
            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal PayoutReportWrapper SelectForPayoutReport(PayoutReport mainRequest, int UserID, out string failureMessage)
        {
            PayoutReportWrapper wrapper = new PayoutReportWrapper();

            List<ViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if ((Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload)) || new Validation().IsValidString(mainRequest.CriteriaText))
                {
                    list = new TransactionReportDAL().SelectForPayoutReportExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForPayoutReport(mainRequest, UserID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().SelectForPayoutCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().PayoutHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            string ShowCommission = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission));

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (ViewModel item in list) item.RetailerCommission = item.DistributerCommission = item.SuperDistributerCommission = 0;
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };
            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });
            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }


            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.Transaction.PayoutReport request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }

            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal PayoutReportWrapper SelectForVendorPayReport(PayoutReport mainRequest, int UserID, out string failureMessage)
        {
            PayoutReportWrapper wrapper = new PayoutReportWrapper();

            List<ViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if ((Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload)) || new Validation().IsValidString(mainRequest.CriteriaText))
                {
                    list = new TransactionReportDAL().SelectForVendorPayReportExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForVendorPayReport(mainRequest, UserID);
                    //if (mainRequest.SkipCount <= 0)
                    wrapper.Count = new TransactionReportDAL().SelectForVendorPayCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().VendorPayHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            string ShowCommission = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission));

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (ViewModel item in list) item.RetailerCommission = item.DistributerCommission = item.SuperDistributerCommission = 0;
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };
            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });
            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }


            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }


        internal LapuWrapper LapuwiseReport(int UserID, LapuReportRequest lapuReportRequest, out string failureMessage)
        {
            LapuWrapper wrapper = new LapuWrapper();
            failureMessage = Messages.DataNotFound;
            List<LapuViewModel> report = new List<LapuViewModel>();
            failureMessage = lapuReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                report = new TransactionReportDAL().LapuwiseReport(lapuReportRequest);
                if (report == null || report.Count <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return wrapper;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            report = report.AsQueryable().WhereDynamic(lapuReportRequest.CriteriaText).ToList();

            wrapper.Count = report.Count;
            wrapper.list = report;
            if (!lapuReportRequest.IsExcelDownload)
            {
                wrapper.list = report.AsQueryable().SkipAndTakeCount(lapuReportRequest.SkipCount, lapuReportRequest.TakeCount).ToList();
            }
            if (lapuReportRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().GetHeaderSummaryData(lapuReportRequest);
            }
            wrapper.IsExcelDownload = lapuReportRequest.IsExcelDownload;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal List<MobileDashboardViewModel> SelectForRetailerDashborad(SelectByDateRequest mainRequest, int UserID, out string failureMessage)
        {
            List<MobileDashboardViewModel> report = new List<MobileDashboardViewModel>();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return report;

            try
            {
                TransactionReportDAL ObjDal = new TransactionReportDAL();
                report = ObjDal.SelectForRetailerDashborad(mainRequest, UserID);

                if (report == null || report.Count <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return report;
                }

                string ShowCommission = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission), "Yes");

                if (ShowCommission.EqualsIgnoreCase("No"))
                {
                    foreach (MobileDashboardViewModel item in report)
                    {
                        item.RetailerCommission = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return report;
        }

        internal async Task<DashboardViewModel> SelectForAdminDashborad()
        {
            DashboardViewModel report = new DashboardViewModel();
            string Date = DateTime.Now.ToString();
            int WalletID = 0;
            try
            {
                TransactionReportDAL ObjDal = new TransactionReportDAL();
                report = await ObjDal.SelectForAdminDashborad(Date, WalletID);
                if (report == null)
                {
                    return report;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return report;
        }

        internal SaleWrapper ServicewiseSale(SelectRequest selectRequest, int UserID, out string failureMessage)
        {
            SaleWrapper wrapper = new SaleWrapper();

            failureMessage = selectRequest.IsValidModalState();
            isExcelOrPDF(selectRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<SaleViewModel> reportList = new List<SaleViewModel>();
            try
            {
                List<SaleViewModel> report = new TransactionReportDAL().ServicewiseSale(selectRequest, UserID);

                if (report == null || report.Count <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return wrapper;
                }

                int zeroAmountCount = 0;

                foreach (SaleViewModel item in report)
                {
                    if (item.SuccessAmt <= 0)
                    {
                        zeroAmountCount++;
                    }
                    if (zeroAmountCount >= 3)
                    {
                        continue;
                    }

                    if (item.ServiceName.EqualsIgnoreCase("Dth")) item.ServiceName = "DTH";
                    reportList.Add(item);
                }

                decimal SuccessAmt = 0;
                int SuccessCount = 0;
                decimal PendingAmt = 0;
                int PendingCount = 0;
                decimal FailAmt = 0;
                int FailCount = 0;
                decimal RCommission = 0;
                decimal DCommission = 0;
                decimal SCommission = 0;
                decimal ACommission = 0;
                foreach (SaleViewModel item in reportList)
                {
                    SuccessAmt += item.SuccessAmt;
                    SuccessCount += item.SuccessCount;
                    PendingAmt += item.PendingAmount;
                    PendingCount += item.PendingCount;
                    FailAmt += item.FailAmt;
                    FailCount += item.FailCount;
                    RCommission += item.RCommission;
                    DCommission += item.DCommission;
                    SCommission += item.SCommission;
                    ACommission += item.ACommission;
                }

                SaleViewModel totalItem = new SaleViewModel();
                totalItem.ServiceName = "Total";
                totalItem.SuccessAmt = SuccessAmt;
                totalItem.SuccessCount = SuccessCount;
                totalItem.PendingAmount = PendingAmt;
                totalItem.PendingCount = PendingCount;
                totalItem.FailAmt = FailAmt;
                totalItem.FailCount = FailCount;
                totalItem.RCommission = RCommission;
                totalItem.DCommission = DCommission;
                totalItem.SCommission = SCommission;
                totalItem.ACommission = ACommission;
                reportList.Add(totalItem);

                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
                {
                    List<SaleViewModel> list = new List<SaleViewModel>();
                    foreach (SaleViewModel item in reportList)
                    {
                        item.SCommission = 0;
                        item.DCommission = 0;
                        item.ACommission = 0;
                        list.Add(item);
                    }

                    wrapper.list = list;
                    if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
                    {
                        wrapper.IsDownload = true;
                    }
                    failureMessage = StatusType.Ok;
                    return wrapper;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            wrapper.list = reportList;
            if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }
            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.TransactionReport.SelectRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal ProviderFailureWrapper ProviderFailure(ProviderFailureRequest providerFailureRequest, out string failureMessage)
        {
            ProviderFailureWrapper wrapper = new ProviderFailureWrapper();

            failureMessage = Messages.DataNotFound;
            List<ProviderFailureViewModel> report = new List<ProviderFailureViewModel>();
            failureMessage = providerFailureRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return wrapper;

            int TotalCount = 0;
            decimal TotalAmount = 0;
            try
            {
                report = new TransactionReportDAL().ProviderFailure(providerFailureRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (report == null || report.Count <= 0) return wrapper;

            foreach (ProviderFailureViewModel item in report)
            {
                TotalAmount += item.Amount;
                TotalCount += item.Count;
            }

            report.Add(new ProviderFailureViewModel
            {
                ProviderName = "",
                FailuerMessage = "Total",
                Amount = TotalAmount,
                Count = TotalCount
            });
            wrapper.IsExcelDownload = providerFailureRequest.IsExcelDownload;
            wrapper.list = report;
            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal ProviderSaleDetailsWrapper ProviderSale(ProviderSaleRequest selectRequest, out string failureMessage)
        {
            ProviderSaleDetailsWrapper wrapper = new ProviderSaleDetailsWrapper();
            failureMessage = selectRequest.IsValidModalState();
            isExcelOrPDF(selectRequest);
            List<ProviderSaleViewModel> report = new List<ProviderSaleViewModel>();
            try
            {
                report = new TransactionReportDAL().ProviderSale(selectRequest);

                if (report == null || report.Count <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return wrapper;
                }
                decimal TotalPurchase = 0;
                decimal TotalSaleAmount = 0;
                decimal TotalFailAmount = 0;
                decimal TotalRetailerCommission = 0;
                decimal TotalDistributerCommission = 0;
                decimal TotalAdminCommission = 0;
                decimal TotalSuperDistributerCommission = 0;
                decimal TotalOpeningBalance = 0;
                decimal TotalClosingBalance = 0;
                decimal TotalDifference = 0;
                decimal TotalProfit = 0;
                foreach (ProviderSaleViewModel item in report)
                {
                    TotalPurchase += item.Purchase;
                    TotalSaleAmount += item.Sale;
                    TotalFailAmount += item.Fail;
                    TotalRetailerCommission += item.RetailerCommission;
                    TotalDistributerCommission += item.DistributerCommission;
                    TotalAdminCommission += item.AdminCommission;
                    TotalSuperDistributerCommission += item.SuperDistributerCommission;
                    TotalOpeningBalance += item.OpeningBalance;
                    TotalClosingBalance += item.ClosingBalance;
                    TotalDifference += item.Difference;
                    TotalProfit += item.Profit;
                }
                ProviderSaleViewModel totalitem = new ProviderSaleViewModel();
                totalitem.ProvderName = "Total";
                totalitem.Purchase = TotalPurchase;
                totalitem.Sale = TotalSaleAmount;
                totalitem.Fail = TotalFailAmount;
                totalitem.RetailerCommission = TotalRetailerCommission;
                totalitem.DistributerCommission = TotalDistributerCommission;
                totalitem.AdminCommission = TotalAdminCommission;
                totalitem.SuperDistributerCommission = TotalSuperDistributerCommission;
                totalitem.OpeningBalance = TotalOpeningBalance;
                totalitem.ClosingBalance = TotalClosingBalance;
                totalitem.Difference = TotalDifference;
                totalitem.Profit = TotalProfit;
                report.Add(totalitem);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }
            wrapper.list = report;
            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.TransactionReport.ProviderSaleRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal GSTSummaryWrapper GSTsummary(GSTSummaryRequest selectRequest, out string failureMessage)
        {
            GSTSummaryWrapper wrapper = new GSTSummaryWrapper();
            List<GSTSummaryViewModel> response = new List<GSTSummaryViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                response = new TransactionReportDAL().GSTsummary(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response.Count <= 0 || response == null)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.list = response;
            wrapper.TotalCount = response.Count;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal ManualSuccessFailWrapper ManualSuccessFail(ManualSuccessFailRequest mainRequest, out string failureMessage)
        {
            ManualSuccessFailWrapper wrapper = new ManualSuccessFailWrapper();

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<ManualSuccessFailViewModel> list = new List<ManualSuccessFailViewModel>();
            try
            {
                list = new TransactionDAL().SelectManualSuccessFail(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list.Count <= 0) return wrapper;

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;
            wrapper.Count = list.Count;
            return wrapper;
        }

        internal PastSuccessFailWrapper SelectPastSuccessFail(PastSuccessFailRequest mainRequest, int UserID, out string failureMessage)
        {
            PastSuccessFailWrapper wrapper = new PastSuccessFailWrapper();

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<PastSuccessFailViewModel> list = new List<PastSuccessFailViewModel>();
            try
            {
                list = new TransactionDAL().SelectPastSuccessFail(mainRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list.Count <= 0) return wrapper;

            wrapper.list = list;
            wrapper.Count = list.Count;
            return wrapper;
        }

        internal UserSaleWrapper UserwiseSale(UserSaleSelectRequest selectRequest, int UserID, out string failureMessage)
        {
            UserSaleWrapper report = new UserSaleWrapper();
            failureMessage = selectRequest.IsValidModalState();
            isExcelOrPDF(selectRequest);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return report;

            List<UserSaleViewModel> list = new List<UserSaleViewModel>();

            try
            {
                if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
                {
                    list = new TransactionReportDAL().UserwiseSaleAndExcelDownload(selectRequest, UserID);
                    report.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().UserwiseSale(selectRequest, UserID);
                    report.Count = GetTotalTransactionCount(selectRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return report;
            }
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return report;

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                foreach (UserSaleViewModel item in list)
                {
                    item.DistributerCommission = 0;
                    item.SuperDistributerCommission = 0;
                }
            }
            if (selectRequest.SkipCount <= 0)
            {
                report.HeaderSummary = GetHeaderSummaryData(list, selectRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(selectRequest.SearchText).ToList();

            list = list.AsQueryable().OrderByDynamic(selectRequest.SortColumnName, selectRequest.SortDirection).ToList();
            if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {
                report.IsDownload = true;
            }


            if (report == null)
            {
                failureMessage = Messages.DataNotFound;
                return report;
            }

            report.list = list;

            failureMessage = StatusType.Ok;
            return report;
        }
        internal static bool isExcelOrPDF(Requests.TransactionReport.UserSaleSelectRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        private int GetTotalTransactionCount(UserSaleSelectRequest selectRequest, int UserID)
        {
            List<UserMaster> usersList = new UserDAL().Select();
            List<UserMaster> listcount = new List<UserMaster>();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            listcount = (from item in usersList
                         select item).ToList();

            if (selectRequest.UserTypeID != 0 && selectRequest.UserID != 0)
            {
                listcount = new TransactionReportDAL().SelectDownlinelist(listcount, selectRequest.UserTypeID, selectRequest.UserID);
            }
            if (selectRequest.UserTypeID != 0 && selectRequest.UserID == 0)
            {
                listcount = (from user in listcount
                             where user.UserTypeID == selectRequest.UserTypeID
                             select user).ToList();
            }
            if (UserID != Company.AdminUserID)
            {
                listcount = (from item in listcount
                             where item.UserID == UserID
                             select item).ToList();
            }

            return listcount.Count;
        }

        internal HeaderUserSaleViewModel GetHeaderSummaryData(List<UserSaleViewModel> list, UserSaleSelectRequest SelectRequest, int UserID)
        {
            HeaderUserSaleViewModel headerlist = new HeaderUserSaleViewModel();
            if (SelectRequest.UserID == 0)
                return headerlist;

            var Success = (from item in list
                           group item by 1 into grp
                           select new
                           {
                               Sale = grp.Sum(X => X.Sale),
                               RetailerCommission = grp.Sum(X => X.RetailerCommission),
                               DistributerCommission = grp.Sum(X => X.DistributerCommission),
                               SuperDistributerCommission = grp.Sum(X => X.SuperDistributerCommission),
                           }).FirstOrDefault();

            Response.Fund.DashboardFundViewModel Fundreport = new FundTransactionDal().SelectFundUserwiseHeader(SelectRequest);

            headerlist.TotalSale = Success?.Sale ?? 0;
            headerlist.TotalRCommission = Success?.RetailerCommission ?? 0;
            headerlist.TotalDCommission = Success?.DistributerCommission ?? 0;
            headerlist.TotalSCommission = Success?.SuperDistributerCommission ?? 0;
            headerlist.TotalPurchase = Fundreport?.TransferAmount ?? 0;
            headerlist.TotalTransfer = Fundreport?.TransferAmount ?? 0;
            return headerlist;
        }

        internal TopPerformerWrapper SelectTopPerformer(SelectTopPerformerRequest selectRequest, int UserID, out string failureMessage)
        {
            TopPerformerWrapper wrapper = new TopPerformerWrapper();
            List<TopPerformerViewModel> report = new List<TopPerformerViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                report = new TransactionReportDAL().SelectTopPerformer(selectRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }

            failureMessage = Messages.DataNotFound;
            if (report == null || report.Count <= 0) return wrapper;

            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.list = report;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal OperatorSaleWrapper OperatorSale(OperatorSaleRequest selectRequest, int UserID, out string failureMessage)
        {
            OperatorSaleWrapper wrapper = new OperatorSaleWrapper();
            List<OperatorSaleViewModel> report = new List<OperatorSaleViewModel>();
            isExcelOrPDF(selectRequest);
            try
            {
                if (UserID != Company.AdminUserID)
                {
                    failureMessage = Message.TransactionMessage.InvalidUser;
                    return wrapper;
                }

                report = new TransactionReportDAL().OperatorSale(selectRequest, UserID);

                failureMessage = Messages.DataNotFound;

                if (report == null || report.Count <= 0) return wrapper;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            report = report.AsQueryable().WhereDynamic(selectRequest.Search).ToList();
            if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {

                wrapper.IsDownload = true;
            }
            wrapper.list = report;
            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.TransactionReport.OperatorSaleRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal List<ViewModel> SelectForLast10(Last10Request mainRequest, int UserID, out string failureMessage)
        {
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return list;
            try
            {
                list = new TransactionReportDAL().SelectForLast10(mainRequest.ServiceID, UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return list;

            string ShowCommission = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission), "Yes");

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (Response.TransactionMaster.ViewModel item in list)
                {
                    item.RetailerCommission = 0;
                    item.DistributerCommission = 0;
                    item.SuperDistributerCommission = 0;
                }
            }

            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<ViewModel> SelectForLast10ByServiceID(Last10Request mainRequest, int UserID, out string failureMessage)
        {
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return list;
            try
            {
                list = new TransactionReportDAL().SelectForLast10ByServiceID(mainRequest.ServiceID, UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return list;

            string ShowCommission = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission), "Yes");

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (Response.TransactionMaster.ViewModel item in list)
                {
                    item.RetailerCommission = 0;
                    item.DistributerCommission = 0;
                    item.SuperDistributerCommission = 0;
                }
            }

            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<TwoMonthPerDaySaleViewModel> MonthlySummary(KeyDataModel keydata, out string failureMessage)
        {
            List<TwoMonthPerDaySaleViewModel> list = new List<TwoMonthPerDaySaleViewModel>();
            failureMessage = Messages.DataNotFound;

            try
            {
                list = new TransactionReportDAL().MonthlySummary(keydata.UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }

            if (list == null || list.Count <= 0)
                return list;

            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<ProviderFailureGraphViewModel> ProviderFailureGraph(KeyDataModel keydata, out string failureMessage)
        {
            List<ProviderFailureGraphViewModel> list = new List<ProviderFailureGraphViewModel>();
            failureMessage = Messages.DataNotFound;

            try
            {
                list = new TransactionReportDAL().ProviderFailureGraph();
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            failureMessage = StatusType.Ok;
            return list;
        }

        internal OperatorWiseGSTWrapper GSTOperatorWise(GSTSummaryRequest selectRequest, out string failureMessage)
        {
            OperatorWiseGSTWrapper wrapper = new OperatorWiseGSTWrapper();
            List<OperatorWiseGSTViewModel> response = new List<OperatorWiseGSTViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                response = new TransactionReportDAL().GSTOperatorWise(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response.Count <= 0 || response == null)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.list = response;
            wrapper.TotalCount = response.Count;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal PurchaseWrapper PurchaseReport(PurchaseReportRequest selectRequest, out string failureMessage)
        {
            PurchaseWrapper wrapper = new PurchaseWrapper();
            List<PurchaseViewModel> response = new List<PurchaseViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                response = new TransactionReportDAL().PurchaseReport(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response.Count <= 0 || response == null)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }
            response = response.AsQueryable().WhereDynamic(selectRequest.Search).ToList();

            response = response.AsQueryable().OrderByDynamic(selectRequest.SortColumnName, selectRequest.SortDirection).ToList();
            wrapper.TotalCount = response.Count;

            if (!selectRequest.IsExcelDownload)
            {
                response = response.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.list = response;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal IncomeReportWrapper Income(IncomeReportRequest selectRequest, int UserID, out string failureMessage)
        {
            IncomeReportWrapper wrapper = new IncomeReportWrapper();

            List<IncomeReportViewModel> reportList = new List<IncomeReportViewModel>();
            isExcelOrPDF(selectRequest);
            try
            {
                List<IncomeReportViewModel> report = new TransactionReportDAL().IncomeReport(selectRequest, UserID);

                if (report == null || report.Count <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return wrapper;
                }

                int zeroAmountCount = 0;

                foreach (IncomeReportViewModel item in report)
                {
                    if (item.SuccessAmt <= 0)
                    {
                        zeroAmountCount++;
                    }
                    if (zeroAmountCount >= 3)
                    {
                        continue;
                    }
                    reportList.Add(item);
                }

                decimal SuccessAmt = 0;
                int SuccessCount = 0;
                decimal RCommission = 0;
                decimal DCommission = 0;
                decimal SCommission = 0;
                decimal AdminCommission = 0;
                decimal NetProfit = 0;

                foreach (IncomeReportViewModel item in reportList)
                {
                    SuccessAmt += item.SuccessAmt;
                    SuccessCount += item.SuccessCount;
                    RCommission += item.RCommission;
                    DCommission += item.DCommission;
                    SCommission += item.SCommission;
                    AdminCommission += item.AdminCommission;
                    NetProfit += item.NetProfit;
                }

                IncomeReportViewModel totalItem = new IncomeReportViewModel();
                totalItem.ServiceName = "Total";
                totalItem.SuccessAmt = SuccessAmt;
                totalItem.SuccessCount = SuccessCount;
                totalItem.RCommission = RCommission;
                totalItem.DCommission = DCommission;
                totalItem.SCommission = SCommission;
                totalItem.AdminCommission = AdminCommission;
                totalItem.NetProfit = NetProfit;
                reportList.Add(totalItem);

                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
                {
                    List<IncomeReportViewModel> list = new List<IncomeReportViewModel>();
                    foreach (IncomeReportViewModel item in reportList)
                    {
                        item.SCommission = 0;
                        item.DCommission = 0;
                        list.Add(item);
                    }

                    wrapper.list = list;
                    if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
                    {
                        wrapper.IsDownload = true;
                    }
                    failureMessage = StatusType.Ok;
                    return wrapper;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            wrapper.list = reportList;
            if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }
            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.TransactionReport.IncomeReportRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal ReportWrapper SelectForFilterByUserID(ReportRequest txReportRequest, KeyDataModel keydata, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);

            failureMessage = txReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new TransactionReportDAL().SelectForFilterByUserID(txReportRequest, keydata.UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;
            list = list.AsQueryable().WhereDynamic(txReportRequest.Search).ToList();

            wrapper.Count = list.Count;
            string ShowCommission = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission), "Yes");

            if (ShowCommission.EqualsIgnoreCase("No") && keydata.UserID != Company.AdminUserID)
            {
                foreach (Response.TransactionMaster.ViewModel item in list)
                {
                    item.RetailerCommission = 0;
                    item.DistributerCommission = 0;
                    item.SuperDistributerCommission = 0;
                }
            }
            if (!txReportRequest.IsExcelDownload)
            {
                list = list.AsQueryable().SkipAndTakeCount(txReportRequest.SkipCount, txReportRequest.TakeCount).ToList();
            }
            wrapper.IsExcelDownload = txReportRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }



        internal SelectByOperatorReportWrapper SelectForFilterByOperatorID(SelectByOperatorRequest txReportRequest, KeyDataModel keydata, out string failureMessage)
        {
            SelectByOperatorReportWrapper wrapper = new SelectByOperatorReportWrapper();
            List<ViewModel> report = new List<ViewModel>();

            failureMessage = txReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                report = new TransactionDetailReportDal().SelectForFilterByOperatorID(txReportRequest, keydata.UserID);

                failureMessage = Messages.DataNotFound;

                if (report == null || report.Count <= 0) return wrapper;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            wrapper.IsExcelDownload = txReportRequest.IsExcelDownload;
            wrapper.Count = report.Count;
            wrapper.list = report;
            failureMessage = StatusType.Ok;
            return wrapper;
        }



        internal ReportWrapper SelectForROfferRechargeFilter(ReportRequest txReportRequest, KeyDataModel keydata, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);
            if (UserTypeID == (int)UserType.Support)
                keydata.UserID = Company.AdminUserID;

            failureMessage = txReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            try
            {
                if (txReportRequest.IsExcelDownload || new Validation().IsValidString(txReportRequest.CriteriaText))
                {
                    list = new TransactionReportDAL().SelectForROfferRechargeFilterAndExcelDownload(txReportRequest, keydata.UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForROfferRechargeFilter(txReportRequest, keydata.UserID);
                    if (txReportRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().GetTransactionCount(txReportRequest, keydata.UserID);
                }
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;

            if (txReportRequest.SkipCount <= 0 && txReportRequest.IsExcelDownload == false)
            {
                wrapper.HeaderSummary = new TransactionBal().GetRofferHeaderSummaryData(txReportRequest, keydata.UserID);
            }

            list = list.AsQueryable().OrderByDynamic(txReportRequest.SortColumnName, txReportRequest.SortDirection).ToList();

            wrapper.IsExcelDownload = txReportRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        private void validateSkipAndTakeCount(ReportRequest txReportRequest)
        {
            try
            {
                if (txReportRequest.SkipCount <= 0)
                {
                    txReportRequest.SkipCount = 0;
                }
                if (txReportRequest.TakeCount <= 0)
                {
                    txReportRequest.TakeCount = 10;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void validateSkipAndTakeCount(AEPSReport mainRequest)
        {
            try
            {
                if (mainRequest.SkipCount <= 0)
                {
                    mainRequest.SkipCount = 0;
                }
                if (mainRequest.TakeCount <= 0)
                {
                    mainRequest.TakeCount = 10;
                }
            }
            catch (Exception ex)
            {
            }
        }

        internal RechargeReportWrapper SelectForRechargeFilter(ReportRequest txReportRequest, KeyDataModel keydata, out string failureMessage)
        {
            RechargeReportWrapper wrapper = new RechargeReportWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);
            if (UserTypeID == (int)UserType.Support)
                keydata.UserID = Company.AdminUserID;

            if (UserTypeID == (int)UserType.DFOS || UserTypeID == (int)UserType.SDFOS)
            {
                int ParentID = new UserDAL().SelectParentID(keydata.UserID);
                keydata.UserID = ParentID;
            }

            failureMessage = txReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            isExcelOrPDF(txReportRequest);
            validateSkipAndTakeCount(txReportRequest);

            List<Response.TransactionMaster.RechargeReportViewModel> list = new List<Response.TransactionMaster.RechargeReportViewModel>();
            try
            {
                if ((Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload)) || new Validation().IsValidString(txReportRequest.CriteriaText))
                {
                    list = new TransactionReportDAL().SelectForRechargeFilterAndExcelDownload(txReportRequest, keydata.UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForRechargeFilter(txReportRequest, keydata.UserID);
                    if (txReportRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().GetTransactionCount(txReportRequest, keydata.UserID);
                }
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;

            if (txReportRequest.SkipCount <= 0 && txReportRequest.IsExcelDownload == false)
            {
                wrapper.HeaderSummary = new TransactionBal().GetHeaderSummaryData(txReportRequest, keydata.UserID, "Recharge");
            }

            list = list.AsQueryable().WhereDynamic(txReportRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(txReportRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(txReportRequest.SortColumnName, txReportRequest.SortDirection).ToList();

            if (txReportRequest.IsExcelDownload || txReportRequest.IsPDFDownload)
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(list, parallelOptions, res =>
                {
                    res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                    res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
                    res.FirmName = new UserDAL().SelectFirmNameByUserID(res.UserID);

                    if (res.Status.EqualsIgnoreCase(StatusType.Failure))
                    {
                        res.RetailerCommission = 0;
                        res.DistributerCommission = 0;
                        res.SuperDistributerCommission = 0;
                    }
                });
            }
            if (!Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload))
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(list, parallelOptions, res =>
                {
                    res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                    res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
                    if (res.Status.EqualsIgnoreCase(StatusType.Failure))
                    {
                        res.RetailerCommission = 0;
                        res.DistributerCommission = 0;
                        res.SuperDistributerCommission = 0;
                    }

                    getCommissionInfo(UserTypeID, res);

                    if (res.ComplaintStatus.EqualsIgnoreCase(ComplaintStatusType.Accept))
                        res.ComplaintStatus = ComplaintStatusType.Refund;

                    res.BBPSLogo = (res.IsBBPS == true) ? "app-docs/BBPS/BBPSMnemonic.png".ConcatWebAPIURL() : "";
                });
            }

            if (keydata.UserID != Company.AdminUserID)
            {
                foreach (Response.TransactionMaster.RechargeReportViewModel item in list)
                {
                    item.ProviderID = 0;
                    item.ProviderName = "";
                    item.ROfferCommission = 0;
                    item.AdminCommission = 0;
                }
            }

            if (Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal RechargeReportWrapper SelectForBBPSFilter(ReportRequest txReportRequest, KeyDataModel keydata, out string failureMessage)
        {
            RechargeReportWrapper wrapper = new RechargeReportWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);
            if (UserTypeID == (int)UserType.Support)
                keydata.UserID = Company.AdminUserID;

            if (UserTypeID == (int)UserType.DFOS || UserTypeID == (int)UserType.SDFOS)
            {
                int ParentID = new UserDAL().SelectParentID(keydata.UserID);
                keydata.UserID = ParentID;
            }

            failureMessage = txReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            isExcelOrPDF(txReportRequest);
            validateSkipAndTakeCount(txReportRequest);

            List<Response.TransactionMaster.RechargeReportViewModel> list = new List<Response.TransactionMaster.RechargeReportViewModel>();
            try
            {
                if ((Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload)) || new Validation().IsValidString(txReportRequest.CriteriaText))
                {
                    list = new TransactionReportDAL().SelectBBPSForRechargeFilterAndExcelDownload(txReportRequest, keydata.UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForBBPSFilter(txReportRequest, keydata.UserID);
                    if (txReportRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().GetBBPSTransactionCount(txReportRequest, keydata.UserID);
                }
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;

            if (txReportRequest.SkipCount <= 0 && txReportRequest.IsExcelDownload == false)
            {
                wrapper.HeaderSummary = new TransactionBal().GetHeaderSummaryData(txReportRequest, keydata.UserID, "Recharge");
            }

            list = list.AsQueryable().WhereDynamic(txReportRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(txReportRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(txReportRequest.SortColumnName, txReportRequest.SortDirection).ToList();

            if (txReportRequest.IsExcelDownload || txReportRequest.IsPDFDownload)
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(list, parallelOptions, res =>
                {
                    res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                    res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
                    res.FirmName = new UserDAL().SelectFirmNameByUserID(res.UserID);

                    if (res.Status.EqualsIgnoreCase(StatusType.Failure))
                    {
                        res.RetailerCommission = 0;
                        res.DistributerCommission = 0;
                        res.SuperDistributerCommission = 0;
                    }
                });
            }
            if (!Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload))
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(list, parallelOptions, res =>
                {
                    res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                    res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
                    if (res.Status.EqualsIgnoreCase(StatusType.Failure))
                    {
                        res.RetailerCommission = 0;
                        res.DistributerCommission = 0;
                        res.SuperDistributerCommission = 0;
                    }

                    getCommissionInfo(UserTypeID, res);

                    if (res.ComplaintStatus.EqualsIgnoreCase(ComplaintStatusType.Accept))
                        res.ComplaintStatus = ComplaintStatusType.Refund;

                    res.BBPSLogo = (res.IsBBPS == true) ? "app-docs/BBPS/BBPSMnemonic.png".ConcatWebAPIURL() : "";
                });
            }

            if (keydata.UserID != Company.AdminUserID)
            {
                foreach (Response.TransactionMaster.RechargeReportViewModel item in list)
                {
                    item.ProviderID = 0;
                    item.ProviderName = "";
                    item.ROfferCommission = 0;
                    item.AdminCommission = 0;
                }
            }

            if (Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }


        internal static bool isExcelOrPDF(Requests.Transaction.ReportRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }

            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }

        internal ReportWrapper SelectForDMTFilter(ReportRequest txReportRequest, KeyDataModel keydata, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);
            if (UserTypeID == (int)UserType.Support)
                keydata.UserID = Company.AdminUserID;


            if (UserTypeID == (int)UserType.DFOS || UserTypeID == (int)UserType.SDFOS)
            {
                int ParentID = new UserDAL().SelectParentID(keydata.UserID);
                keydata.UserID = ParentID;
            }

            failureMessage = txReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            validateSkipAndTakeCount(txReportRequest);
            isExcelOrPDF(txReportRequest);

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            try
            {
                if (Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload) || new Validation().IsValidString(txReportRequest.CriteriaText))
                {
                    list = new TransactionReportDAL().SelectForDMTFilterAndExcelDownload(txReportRequest, keydata.UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForDMTFilter(txReportRequest, keydata.UserID);
                    if (txReportRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().GetDMTTransactionCount(txReportRequest, keydata.UserID);
                }
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;

            if (txReportRequest.SkipCount <= 0 && txReportRequest.IsPDFDownload == false && txReportRequest.IsExcelDownload == false && (!new Validation().IsValidString(txReportRequest.CriteriaText)))
                wrapper.HeaderSummary = new TransactionBal().GetHeaderSummaryData(txReportRequest, keydata.UserID, "DMT");

            list = list.AsQueryable().WhereDynamic(txReportRequest.Search).ToList();

            if (!txReportRequest.Criteria.EqualsIgnoreCase("AmountRange"))
                list = list.AsQueryable().WhereDynamic(txReportRequest.CriteriaText).ToList();

            if (new Validation().IsValidString(txReportRequest.CriteriaText))
                wrapper.HeaderSummary = new TransactionBal().GetHeaderSummaryDataByList(list, keydata.UserID);

            list = list.AsQueryable().OrderByDynamic(txReportRequest.SortColumnName, txReportRequest.SortDirection).ToList();

            if (txReportRequest.IsExcelDownload || txReportRequest.IsPDFDownload)
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(list, parallelOptions, res =>
                {
                    res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                    res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
                    res.FirmName = new UserDAL().SelectFirmNameByUserID(res.UserID);

                    if (res.Status.EqualsIgnoreCase(StatusType.Failure))
                    {
                        res.RetailerCommission = 0;
                        res.DistributerCommission = 0;
                        res.SuperDistributerCommission = 0;
                    }
                });
            }

            if (!Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload))
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };
                Parallel.ForEach(list, parallelOptions, res =>
                {
                    res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                    res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);

                    getCommissionInfoDMT(UserTypeID, res);

                    if (res.ComplaintStatus.EqualsIgnoreCase(ComplaintStatusType.Accept))
                        res.ComplaintStatus = ComplaintStatusType.Refund;
                });
            }
            if (keydata.UserID != Company.AdminUserID)
            {
                foreach (Response.TransactionMaster.ViewModel item in list)
                {
                    item.ProviderID = 0;
                    item.ProviderName = "";
                    item.AdminCommission = 0;
                }
            }

            //list = list.Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();

            if (Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload))
            {
                wrapper.IsExcelDownload = true;
            }
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        private void getCommissionInfo(int UserTypeID, Response.TransactionMaster.RechargeReportViewModel res)
        {
            if (Util.isRoleAdminOrSupport(UserTypeID))
                res.CommissionInfo = $"MDS:{res.SuperDistributerCommission.TwoDigitDecimal()}{Environment.NewLine}DS:{res.DistributerCommission.TwoDigitDecimal()}";

            if (UserTypeID == (int)UserType.SuperDistributer)
                res.CommissionInfo = $"RT:{res.RetailerCommission.TwoDigitDecimal()}, MDS:{res.SuperDistributerCommission.TwoDigitDecimal()}{Environment.NewLine}DS:{res.DistributerCommission.TwoDigitDecimal()}";

            if (UserTypeID == (int)UserType.Distributer)
                res.CommissionInfo = $"RT:{res.RetailerCommission.TwoDigitDecimal()}, DT:{res.DistributerCommission.TwoDigitDecimal()}";
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
                res.CommissionInfo = $"RT:{res.RetailerCommission.TwoDigitDecimal()}";
        }

        private void getCommissionInfoDMT(int UserTypeID, Response.TransactionMaster.ViewModel res)
        {
            if (Util.isRoleAdminOrSupport(UserTypeID))
                res.CommissionInfo = $"MDS:{res.SuperDistributerCommission.TwoDigitDecimal()}{Environment.NewLine}DS:{res.DistributerCommission.TwoDigitDecimal()}";

            if (UserTypeID == (int)UserType.SuperDistributer)
                res.CommissionInfo = $"RT:{res.RetailerCommission.TwoDigitDecimal()}, MDS:{res.SuperDistributerCommission.TwoDigitDecimal()}{Environment.NewLine}DS:{res.DistributerCommission.TwoDigitDecimal()}";

            if (UserTypeID == (int)UserType.Distributer)
                res.CommissionInfo = $"RT:{res.RetailerCommission.TwoDigitDecimal()}, DS:{res.DistributerCommission.TwoDigitDecimal()}";
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
                res.CommissionInfo = $"RT:{res.RetailerCommission.TwoDigitDecimal()}";
        }

        internal List<SuccessPayoutTransactionModel> SelectSuccessPayoutTransaction(KeyDataModel keydata, out string failureMessage)
        {
            List<SuccessPayoutTransactionModel> list = new List<SuccessPayoutTransactionModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);
            failureMessage = StatusType.Ok;
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return list;

            try
            {
                list = new TransactionReportDAL().SelectSuccessPayoutTransaction(keydata.UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }

            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return list;

            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<SuccessUPITransferTransactionModel> SelectSuccessUPITransferTransaction(KeyDataModel keydata, out string failureMessage)
        {
            List<SuccessUPITransferTransactionModel> list = new List<SuccessUPITransferTransactionModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);
            failureMessage = StatusType.Ok;
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return list;

            try
            {
                list = new TransactionReportDAL().SelectSuccessUPITransaction(keydata.UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }

            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return list;

            failureMessage = StatusType.Ok;
            return list;
        }

        internal ReportWrapper SelectForCreditCardFilter(ReportRequest txReportRequest, KeyDataModel keydata, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);
            failureMessage = txReportRequest.IsValidModalState();
            isExcelOrPDF(txReportRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            try
            {
                if ((Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload)) || new Validation().IsValidString(txReportRequest.CriteriaText))
                {
                    list = new TransactionReportDAL().SelectForCreditCardFilterAndExcelDownload(txReportRequest, keydata.UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForCreditCardFilter(txReportRequest, keydata.UserID);
                    if (txReportRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().GetCreditCardTransactionCount(txReportRequest, keydata.UserID);
                }
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;

            if (txReportRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().CreditCardHeaderSummary(txReportRequest, keydata.UserID);
            }

            list = list.AsQueryable().WhereDynamic(txReportRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(txReportRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(txReportRequest.SortColumnName, txReportRequest.SortDirection).ToList();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);

                if (Util.isRoleAdminOrSupport(UserTypeID))
                    res.CommissionInfo = $"R :{res.RetailerCommission.TwoDigitDecimal()}{Environment.NewLine} D :{res.DistributerCommission.TwoDigitDecimal()}{Environment.NewLine} S :{res.SuperDistributerCommission.TwoDigitDecimal()}";

                if (UserTypeID == (int)UserType.SuperDistributer)
                    res.CommissionInfo = $"R :{res.RetailerCommission.TwoDigitDecimal()}{Environment.NewLine} D :{res.DistributerCommission.TwoDigitDecimal()}{Environment.NewLine} S :{res.SuperDistributerCommission.TwoDigitDecimal()}";

                if (UserTypeID == (int)UserType.Distributer)
                    res.CommissionInfo = $"R :{res.RetailerCommission.TwoDigitDecimal()}{Environment.NewLine} D :{res.DistributerCommission.TwoDigitDecimal()}";
                if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
                    res.CommissionInfo = $"R :{res.RetailerCommission.TwoDigitDecimal()} ";
            });

            string ShowCommission = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission), "Yes");

            if (ShowCommission.EqualsIgnoreCase("No") && keydata.UserID != Company.AdminUserID)
            {
                foreach (Response.TransactionMaster.ViewModel item in list)
                {
                    item.RetailerCommission = 0;
                    item.DistributerCommission = 0;
                    item.SuperDistributerCommission = 0;
                }
            }
            if (keydata.UserID != Company.AdminUserID)
            {
                foreach (Response.TransactionMaster.ViewModel item in list)
                {
                    item.ProviderID = 0;
                    item.ProviderName = "";
                }
            }
            if (Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }

            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal RechargeReportWrapper SelectForRechargeCompactFilter(ReportRequest txReportRequest, KeyDataModel keydata, out string failureMessage)
        {
            RechargeReportWrapper wrapper = new RechargeReportWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);




            failureMessage = txReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<Response.TransactionMaster.RechargeReportViewModel> list = new List<Response.TransactionMaster.RechargeReportViewModel>();
            try
            {
                if (txReportRequest.IsExcelDownload || new Validation().IsValidString(txReportRequest.CriteriaText))
                {
                    list = new TransactionReportDAL().SelectForRechargeFilterAndExcelDownload(txReportRequest, keydata.UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForRechargeFilter(txReportRequest, keydata.UserID);
                    if (txReportRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().GetTransactionCount(txReportRequest, keydata.UserID);
                }
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;

            if (txReportRequest.SkipCount <= 0 && txReportRequest.IsExcelDownload == false)
            {
                wrapper.HeaderSummary = new TransactionBal().GetHeaderSummaryData(txReportRequest, keydata.UserID, "Recharge");
            }

            list = list.AsQueryable().WhereDynamic(txReportRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(txReportRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(txReportRequest.SortColumnName, txReportRequest.SortDirection).ToList();

            if (txReportRequest.IsExcelDownload)
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(list, parallelOptions, res =>
                {
                    res.Name = $"{res.FirmName}{Environment.NewLine}{res.UserMobile}{Environment.NewLine}{res.Through}";
                    res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                    res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
                    if (res.Status.EqualsIgnoreCase(StatusType.Failure))
                    {
                        res.RetailerCommission = 0;
                        res.DistributerCommission = 0;
                        res.SuperDistributerCommission = 0;
                    }
                });
            }

            if (!txReportRequest.IsExcelDownload)
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(list, parallelOptions, res =>
                {
                    res.Date = $"Rech: {res.DOC}{Environment.NewLine}Res: {res.UpdatedDOC}{Environment.NewLine}Time: {res.ProcTime}";
                    res.Name = $"{res.FirmName}{Environment.NewLine}{res.UserMobile}{Environment.NewLine}{res.Through}";
                    res.Operator = $"{res.OperatorName}{Environment.NewLine}{res.ConsumerNumber}";
                    res.AmountInfo = $"AMT: {res.Amount.TwoDigitDecimal()}{Environment.NewLine}Debit: {res.DebitAmount.TwoDigitDecimal()}{Environment.NewLine}GST: {res.GST.TwoDigitDecimal()}";
                    res.BalanceInfo = $"Op.Bal: {res.OpeningBalance.TwoDigitDecimal()}{Environment.NewLine}Cl.Bal: {res.ClosingBalance.TwoDigitDecimal()}{Environment.NewLine}TDS: {res.TDS.TwoDigitDecimal()}";
                    res.ProviderInfo = $"{res.ProviderName}{Environment.NewLine}{res.Circle} / {res.IPAddress}{Environment.NewLine}{res.CashBack.TwoDigitDecimal()}";
                    res.APIBalInfo = $"Com: {res.ProviderCommission.TwoDigitDecimal()}{Environment.NewLine}Cl.Bal: {res.ProviderClosingBalance.TwoDigitDecimal()}{Environment.NewLine}V.ID: {res.APIUserRequestID}";

                    res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                    res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
                    if (res.Status.EqualsIgnoreCase(StatusType.Failure))
                    {
                        res.RetailerCommission = 0;
                        res.DistributerCommission = 0;
                        res.SuperDistributerCommission = 0;
                    }
                    if (Util.isRoleAdminOrSupport(UserTypeID))
                        res.CommissionInfo = $"R: {res.RetailerCommission.TwoDigitDecimal()}{Environment.NewLine}D: {res.DistributerCommission.TwoDigitDecimal()}{Environment.NewLine}S: {res.SuperDistributerCommission.TwoDigitDecimal()}{Environment.NewLine}A: {res.AdminProfit.TwoDigitDecimal()}";

                    if (UserTypeID == (int)UserType.SuperDistributer)
                        res.CommissionInfo = $"R: {res.RetailerCommission.TwoDigitDecimal()}{Environment.NewLine}D: {res.DistributerCommission.TwoDigitDecimal()}{Environment.NewLine}S: {res.SuperDistributerCommission.TwoDigitDecimal()}";

                    if (UserTypeID == (int)UserType.Distributer)
                        res.CommissionInfo = $"R: {res.RetailerCommission.TwoDigitDecimal()}{Environment.NewLine}D: {res.DistributerCommission.TwoDigitDecimal()}";


                    if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
                        res.CommissionInfo = $"R: {res.RetailerCommission.TwoDigitDecimal()} ";

                    if (res.ComplaintStatus.EqualsIgnoreCase(ComplaintStatusType.Accept))
                        res.ComplaintStatus = ComplaintStatusType.Refund;

                    res.TxnID = $"TxnID: {res.ReferenceNo}{Environment.NewLine}Comp: {res.ComplaintStatus}";

                    res.BBPSLogo = (res.IsBBPS == true) ? "app-docs/BBPS/BBPSMnemonic.png".ConcatWebAPIURL() : "";
                });
            }

            if (keydata.UserID != Company.AdminUserID)
            {
                foreach (Response.TransactionMaster.RechargeReportViewModel item in list)
                {
                    item.ProviderID = 0;
                    item.ProviderName = "";
                    item.ROfferCommission = 0;
                    item.AdminCommission = 0;
                }
            }

            wrapper.IsExcelDownload = txReportRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal AmountWiseInsightsWrapper AmountWiseInsightsSelect(AmountWiseInsightsSelectRequest selectRequest, out string failureMessage)
        {
            AmountWiseInsightsWrapper wrapper = new AmountWiseInsightsWrapper();

            List<AmountWiseInsightsViewModel> report = new List<AmountWiseInsightsViewModel>();

            failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                report = new TransactionReportDAL().AmountWiseInsightsSelect(selectRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }
            wrapper.TotalCount = report.Count;

            if (!selectRequest.IsExcelDownload)
            {
                report = report.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();
            }

            wrapper.list = report;
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        public void SendBulkRemainderMessage()
        {
            List<Response.RemainderOperatorAmountWise.ViewModel> viewModel = new RemainderOperatorAmountWiseDal().Select();
            if (viewModel == null)
                return;

            foreach (Response.RemainderOperatorAmountWise.ViewModel item in viewModel)
            {
            }
        }

        internal LeadGenerationReportWrapper SelectForLeadGenerationReport(LeadGenerationReportRequest mainRequest, int UserID, out string failureMessage)
        {
            LeadGenerationReportWrapper wrapper = new LeadGenerationReportWrapper();

            List<ViewModel> list = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                if (mainRequest.IsExcelDownload)
                {
                    list = new TransactionReportDAL().SelectForLeadGenerationExcelDownload(mainRequest, UserID);
                    wrapper.Count = list.Count();
                }
                else
                {
                    list = new TransactionReportDAL().SelectForLeadGenerationReport(mainRequest, UserID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new TransactionReportDAL().SelectForLeadGenerationCount(mainRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0)
                return wrapper;

            if (mainRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = new TransactionReportDAL().LeadGenerationHeaderSummary(mainRequest, UserID);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            string ShowCommission = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission));

            if (ShowCommission.EqualsIgnoreCase("No") && UserID != Company.AdminUserID)
            {
                foreach (ViewModel item in list) item.RetailerCommission = item.DistributerCommission = item.SuperDistributerCommission = 0;
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };
            Parallel.ForEach(list, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal List<Response.SDK.ViewModel> SelectForLast10ByAgentCode(string AgentCode, out string failureMessage)
        {
            List<Response.SDK.ViewModel> list = new List<Response.SDK.ViewModel>();
            try
            {
                list = new TransactionReportDAL().SelectForLast10ByAgentCode(AgentCode);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return list;

            failureMessage = StatusType.Ok;
            return list;
        }
    }
}