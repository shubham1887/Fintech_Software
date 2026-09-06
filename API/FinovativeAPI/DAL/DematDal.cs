using Project.Model;
using Project.Processing;
using Project.Requests.Demat;
using Project.Response.Demat;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class DematDal
    {
        internal List<ViewModel> Select(SelectRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.AEPS).ToList();

            List<ViewModel> list = new List<ViewModel>();
            List<TransactionMaster> transactionMaster = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                transactionMaster = (from Tran in context.TransactionMaster
                                     where Tran.DOCInt >= FromDate
                                     && Tran.DOCInt <= ToDate
                                     && Tran.ServiceID == (int)ServiceCode.Demat
                                     && (!txReportRequest.Status.EqualsIgnoreCase("All") ? Tran.Status == txReportRequest.Status : 2 > 1)
                                     && (txReportRequest.UserID == 0 ? 2 > 1 : Tran.UserID == txReportRequest.UserID)
                                     orderby Tran.TransactionMasterID descending
                                     select Tran).ToList();
                list = (from Tran in transactionMaster
                        join User in usersList on Tran.UserID equals User.UserID
                        select new ViewModel
                        {
                            UserID = Tran.UserID,
                            TransactionMasterID = Tran.TransactionMasterID,
                            FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                            DOC = Tran.DOC.LongFormatDate(),
                            RequestID = Tran.ProviderRequestID,
                            PersonName = Tran.DMT_SenderName,
                            MobileNumber = Tran.DMT_SenderNumber,
                            EmailID = Tran.DMT_BeneficiaryName,
                            PanNumber = Tran.ConsumerNumber,
                            UCC = Tran.ReferenceNo,
                            Status = Tran.Status,
                            OpeningBalance = Tran.OpeningBalance,
                            Amount = Tran.Amount,
                            RetailerComm = Tran.RetailerCommission,
                            ClosingBalance = Tran.ClosingBalance,
                        }).ToList();
                if (!Util.isRoleAdminOrSupport(UserTypeID))
                {
                    list = (from item in list
                            where item.UserID == UserID
                            select item).ToList();
                }
            }

            return list;
        }
    }
}