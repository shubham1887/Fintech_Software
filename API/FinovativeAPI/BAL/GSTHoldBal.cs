using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.GSTHold;
using Project.Response.GSTHold;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class GSTHoldBal
    {
        internal GSTHoldWrapper SelectGSTHold(GSTHoldSearchRequest selectRequest, int LoginUserID, out string failureMessage)
        {
            GSTHoldWrapper wrapper = new GSTHoldWrapper();
            List<ViewModel> response = new List<ViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                response = new GSTHoldDal().SelectGSTHold(selectRequest, LoginUserID);
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

        internal decimal GSTholdAmount(int UserID)
        {
            if (!Company.GSTHold.EqualsIgnoreCase("Yes")) return 0;

            int Year = Convert.ToInt32(DateTime.Now.Year.ToString("0#"));
            int lastMonth = DateTime.Now.AddMonths(-1).Month;

            if (lastMonth == 12)
                Year = Year - 1;

            decimal GSTHoldedAmount = new GSTHoldDal().SelectHoldAmountUserwise(UserID, lastMonth, Year);

            return GSTHoldedAmount;
        }

        public void GSTUpdateMonthly()
        {
            try
            {
                bool isDevelopment = new Util().isDevelopment();
                if (isDevelopment)
                {
                    return;
                }

                if (!Company.GSTHold.EqualsIgnoreCase("Yes"))
                {
                    return;
                }

                DateTime lastMonth = DateTime.Now.AddMonths(-1);

                int Year = lastMonth.Year;
                int Month = lastMonth.Month;

                Requests.TransactionReport.GSTSummaryRequest selectRequest = new Requests.TransactionReport.GSTSummaryRequest
                {
                    Year = Year,
                    Month = Month
                };
                List<GSTViewModel> response = new List<GSTViewModel>();
                try
                {
                    response = new GSTHoldDal().GSTsummary(selectRequest, Messages.GSTMonthly);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (response == null || response.Count <= 0)
                {
                    return;
                }

                foreach (GSTViewModel item in response)
                {
                    GSTHold ObjModel = null;
                    try
                    {
                        ObjModel = new GSTHoldDal().SelectByID(item);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                        continue;
                    }

                    if (ObjModel != null) continue;

                    ObjModel = new GSTHold();
                    ObjModel.UserID = item.UserID;
                    ObjModel.Month = item.Month;
                    ObjModel.Year = item.Year;
                    ObjModel.StateID = item.StateID;
                    ObjModel.IGST = item.IGST;
                    ObjModel.CGST = item.CGST;
                    ObjModel.SGST = item.SGST;
                    ObjModel.HoldAmount = item.HoldAmount;
                    ObjModel.TaxableValue = item.TaxableValue;
                    ObjModel.GSTNo = item.GSTNo;
                    ObjModel.Status = StatusType.Process;

                    try
                    {
                        if (ObjModel.HoldAmount > 0.0001M)
                            new GSTHoldDal().Insert(ObjModel);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        public void GSTUpdateDaily()
        {
            try
            {
                bool isDevelopment = new Util().isDevelopment();
                if (isDevelopment == true)
                {
                    return;
                }

                if (!Company.GSTHold.EqualsIgnoreCase("Yes"))
                {
                    return;
                }

                DateTime lastMonth = DateTime.Now;

                int Year = lastMonth.Year;
                int Month = lastMonth.Month;

                Requests.TransactionReport.GSTSummaryRequest selectRequest = new Requests.TransactionReport.GSTSummaryRequest
                {
                    Year = Year,
                    Month = Month
                };
                List<GSTViewModel> response = new List<GSTViewModel>();
                try
                {
                    response = new GSTHoldDal().GSTsummary(selectRequest, Messages.GSTDaily);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (response == null || response.Count <= 0)
                {
                    return;
                }

                foreach (GSTViewModel item in response)
                {
                    GSTHold ObjModel = null;
                    try
                    {
                        ObjModel = new GSTHoldDal().SelectByID(item);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                        continue;
                    }

                    if (ObjModel != null)
                    {
                        ObjModel.IGST = item.IGST;
                        ObjModel.CGST = item.CGST;
                        ObjModel.SGST = item.SGST;
                        ObjModel.HoldAmount = item.HoldAmount;
                        ObjModel.TaxableValue = item.TaxableValue;
                        ObjModel.Status = StatusType.Process;
                        ObjModel.UpdatedDOC = DateTime.Now;

                        try
                        {
                            if (ObjModel.HoldAmount > 0.0001M)
                                new GSTHoldDal().Update(ObjModel);
                        }
                        catch (Exception ex)
                        {
                            new Logger().Write(ex);
                        }

                        return;
                    }
                    if (ObjModel == null)
                    {
                        ObjModel = new GSTHold();
                        ObjModel.UserID = item.UserID;
                        ObjModel.Month = item.Month;
                        ObjModel.Year = item.Year;
                        ObjModel.StateID = item.StateID;
                        ObjModel.IGST = item.IGST;
                        ObjModel.CGST = item.CGST;
                        ObjModel.SGST = item.SGST;
                        ObjModel.HoldAmount = item.HoldAmount;
                        ObjModel.TaxableValue = item.TaxableValue;
                        ObjModel.GSTNo = item.GSTNo;
                        ObjModel.Status = StatusType.Process;

                        try
                        {
                            if (ObjModel.HoldAmount > 0.0001M)
                                new GSTHoldDal().Insert(ObjModel);
                        }
                        catch (Exception ex)
                        {
                            new Logger().Write(ex);
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal string UpdateStatus(UpdateStatusRequest updateRequest, int UserID)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            int LoginUserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (!Util.isRoleAdminOrSupport(LoginUserTypeID))
                return GSTMessage.InvalidUser;

            GSTHold gstHold = new GSTHoldDal().SelectByGSTHoldID(updateRequest.GSTHoldID);

            if (gstHold == null)
                return GSTMessage.NotUpdateStatus;

            if (new ResponseProcess().checkProcess(gstHold.Status))  // Original status process
            {
                gstHold.Status = updateRequest.Status;
                gstHold.AdminRemark = updateRequest.AdminRemark;
                gstHold.UpdatedDOC = DateTime.Now;

                int rowsAfftected = new GSTHoldDal().Update(gstHold);

                if (rowsAfftected <= 0)
                    return GSTMessage.NotUpdateStatus;
            }

            return StatusType.Ok;
        }

        internal CheckGSTHoldWrapper SelectForGSTHold(Requests.TransactionReport.GSTSummaryRequest selectRequest, out string failureMessage)
        {
            CheckGSTHoldWrapper wrapper = new CheckGSTHoldWrapper();

            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            if (!Company.GSTHold.EqualsIgnoreCase("Yes"))
            {
                return wrapper;
            }
            List<GSTViewModel> responsemonthly = new List<GSTViewModel>();
            try
            {
                responsemonthly = new GSTHoldDal().GSTsummary(selectRequest, Messages.GSTMonthly);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (responsemonthly == null || responsemonthly.Count <= 0)
            {
                return wrapper;
            }

            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.list = responsemonthly;
            wrapper.TotalCount = responsemonthly.Count;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal string BulkUpdateGST(List<GSTBulkUpdateUsersRequest> mainRequest, out string failureMessge)
        {
            string Result = GSTMessage.successupdate;

            failureMessge = GSTMessage.failupdate;
            if (mainRequest == null || mainRequest.Count <= 0)
                return failureMessge;

            if (!Company.GSTHold.EqualsIgnoreCase("Yes"))
            {
                return failureMessge;
            }

            int FailureCount = 0, SuccessCount = 0;

            foreach (GSTBulkUpdateUsersRequest request in mainRequest)
            {
                failureMessge = BulkUpdateUser(request);

                if (!failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    FailureCount++;

                if (failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    SuccessCount++;
            }

            failureMessge = StatusType.Ok;
            if (FailureCount <= 0 && SuccessCount > 0) return Result;

            if (SuccessCount <= 0)
            {
                Result = GSTMessage.failupdate;
                return Result;
            }

            Result = GSTMessage.UserGSTSuccessFail;
            Result = Result.Replace("[FailureCount]", FailureCount.ToString())
                           .Replace("[SuccessCount]", SuccessCount.ToString());
            return Result;
        }

        internal string BulkUpdateUser(GSTBulkUpdateUsersRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            Requests.TransactionReport.GSTSummaryRequest selectRequest = new Requests.TransactionReport.GSTSummaryRequest
            {
                Year = mainRequest.Year,
                Month = mainRequest.Month
            };
            List<GSTViewModel> response = new List<GSTViewModel>();
            try
            {
                response = new GSTHoldDal().GSTsummary(selectRequest, Messages.GSTMonthly);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response == null || response.Count <= 0)
            {
                return failureMessage;
            }

            foreach (GSTViewModel item in response)
            {
                failureMessage = GSTMessage.failupdate;
                if (item.UserID != mainRequest.UserID) continue;

                GSTHold ObjModel = null;
                try
                {
                    ObjModel = new GSTHoldDal().SelectByID(item);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    continue;
                }

                if (ObjModel != null) continue;

                ObjModel = new GSTHold();
                ObjModel.UserID = item.UserID;
                ObjModel.Month = item.Month;
                ObjModel.Year = item.Year;
                ObjModel.StateID = item.StateID;
                ObjModel.IGST = item.IGST;
                ObjModel.CGST = item.CGST;
                ObjModel.SGST = item.SGST;
                ObjModel.HoldAmount = item.HoldAmount;
                ObjModel.TaxableValue = item.TaxableValue;
                ObjModel.GSTNo = item.GSTNo;
                ObjModel.Status = StatusType.Process;

                try
                {
                    if (ObjModel.HoldAmount > 0)
                        new GSTHoldDal().Insert(ObjModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }
    }
}