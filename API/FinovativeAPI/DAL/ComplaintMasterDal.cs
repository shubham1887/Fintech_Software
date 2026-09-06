using Project.Model;
using Project.Processing;
using Project.Requests.Complaints;
using Project.Response.ComplaintResponse;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class ComplaintMasterDal
    {
        private readonly string Key = "_ComplaintMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ComplaintMaster>>();
            cache.Remove(Key);
        }

        public int Insert(ComplaintMaster ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ComplaintMaster.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.ComplaintMasterID;
        }

        public int Update(ComplaintMaster ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ComplaintMaster.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.ComplaintMasterID;
        }

        internal List<ComplaintMaster> SelectAll()
        {
            var _complaintMasterList = new MemoryCacheProcessing<List<ComplaintMaster>>();
            List<ComplaintMaster> complaintMasterList = _complaintMasterList.GetOrCreate(Key, () => SelectWithoutCache());

            return complaintMasterList.OrderByDescending(c => c.DOC).ToList();
        }

        private List<ComplaintMaster> SelectWithoutCache()
        {
            List<ComplaintMaster> list = new List<ComplaintMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.ComplaintMaster.ToList();
            }
            return list;
        }

        internal ComplaintMaster SelectByTransactionMasterID(int TransactionMasterID)
        {
            return SelectAll().Where(C => C.TransactionMasterID == TransactionMasterID).FirstOrDefault();
        }

        internal async Task<int> SelectComplaintCount()
        {
            List<ComplaintMaster> complaints = SelectAll();

            int ComplaintCount = complaints.Where(c => c.ComplaintStatus == StatusType.Process && c.ProviderID > 0).Count();

            await Task.Delay(1);

            return ComplaintCount;
        }

        internal ComplaintMaster SelectByID(int complaintMasterID)
        {
            return SelectAll().FirstOrDefault(C => C.ComplaintMasterID == complaintMasterID);
        }

        internal List<ViewModel> SelectExcelDownload(SelectRequest mainRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<ViewModel> response = null;
            List<Response.Operator.ViewModel> operators = new OperatorMasterDAL().Select();
            List<ComplaintMaster> complaints = SelectAll();
            List<UserMaster> userList = new UserDAL().Select();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            response = (from complaint in complaints
                        where complaint.DOCInt >= From && complaint.DOCInt <= To
                        join op in operators on complaint.OperatorID equals (op.OperatorID)
                        join ul in userList on complaint.UserID equals (ul.UserID)
                        join pl in providerList on complaint.ProviderID equals pl.ProviderID
                       into providerDetails
                        from LProvider in providerDetails.DefaultIfEmpty()
                        orderby complaint.DOC descending
                        select new ViewModel
                        {
                            OperatorID = complaint.OperatorID,
                            Service = op.ServiceName,
                            Operator = op.OperatorName,
                            ProviderName = LProvider?.ProviderName ?? "",
                            UserID = complaint.UserID,
                            FirmName = ul.FirmName.ConcatNameMobileAliasName(ul.MobileNumber, ul.AliasName),
                            ComplaintMasterID = complaint.ComplaintMasterID,
                            ComplaintStatus = complaint.ComplaintStatus,
                            ComplaintType = complaint.ComplaintType ?? ComplaintType.Complaint,
                            Amount = complaint.Amount,
                            ConsumerNumber = complaint.ConsumerNumber,
                            NewConsumerNumber = complaint.NewConsumerNumber ?? "",
                            CustomerRemark = complaint.CustomerRemark,
                            AdminRemark = complaint.AdminRemark,
                            TransactionMasterID = complaint.TransactionMasterID,
                            Logo = op?.Path?.ConcatWebAPIURL(),
                            DOC = complaint.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            TransactionDate = complaint.TransactionDate.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture) ?? "",
                            UpdatedDOC = complaint.UpdatedDOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture)
                        }).ToList();

            if (!mainRequest.Status.EqualsIgnoreCase("All"))
            {
                response = (from item in response
                            where item.ComplaintStatus.ToUpper() == mainRequest.Status.ToUpper()
                            select item).ToList();
            }
            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                response = (from item in response
                            where item.UserID == UserID
                            select item).ToList();
            }
            return response;
        }

        internal int SelectComplaintCount(SelectRequest mainRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<ViewModel> response = null;
            List<Response.Operator.ViewModel> operators = new OperatorMasterDAL().Select();
            List<ComplaintMaster> complaints = SelectAll();
            List<UserMaster> userList = new UserDAL().Select();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            response = (from complaint in complaints
                        where complaint.DOCInt >= From && complaint.DOCInt <= To
                        join op in operators on complaint.OperatorID equals (op.OperatorID)
                        join ul in userList on complaint.UserID equals (ul.UserID)
                        join pl in providerList on complaint.ProviderID equals pl.ProviderID
                        into providerdetails
                        from LProvider in providerdetails.DefaultIfEmpty()
                        orderby complaint.DOC descending
                        select new ViewModel
                        {
                            OperatorID = complaint.OperatorID,
                            Service = op.ServiceName,
                            Operator = op.OperatorName,
                            ProviderName = LProvider?.ProviderName ?? "",
                            UserID = complaint.UserID,
                            FirmName = ul.FirmName.ConcatNameMobileAliasName(ul.MobileNumber, ul.AliasName),
                            ComplaintMasterID = complaint.ComplaintMasterID,
                            ComplaintStatus = complaint.ComplaintStatus,
                            ComplaintType = complaint.ComplaintType ?? ComplaintType.Complaint,
                            Amount = complaint.Amount,
                            ConsumerNumber = complaint.ConsumerNumber,
                            NewConsumerNumber = complaint.NewConsumerNumber ?? "",
                            CustomerRemark = complaint.CustomerRemark,
                            AdminRemark = complaint.AdminRemark,
                            TransactionMasterID = complaint.TransactionMasterID,
                            Logo = op?.Path?.ConcatWebAPIURL(),
                            DOC = complaint.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            TransactionDate = complaint.TransactionDate.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture) ?? "",
                            UpdatedDOC = complaint.UpdatedDOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture)
                        }).ToList();

            if (!mainRequest.Status.EqualsIgnoreCase("All"))
            {
                response = (from item in response
                            where item.ComplaintStatus.ToUpper() == mainRequest.Status.ToUpper()
                            select item).ToList();
            }
            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                response = (from item in response
                            where item.UserID == UserID
                            select item).ToList();
            }
            return response.Count;
        }

        internal List<ViewModel> Select(SelectRequest mainRequest, int UserID)
        {

            List<ViewModel> response = null;
            List<Response.Operator.ViewModel> operators = new OperatorMasterDAL().Select();
            List<ComplaintMaster> complaints = SelectAll();
            List<UserMaster> userList = new UserDAL().Select();
            int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            if (mainRequest.DateType.EqualsIgnoreCase("PAST")){
                From = Convert.ToInt32(DateTime.Now.AddDays(-15).ToString(Messages.DOCIntegerFomrat));
                To = Convert.ToInt32(DateTime.Now.AddDays(-1).ToString(Messages.DOCIntegerFomrat));
            }
         
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

                response = (from complaint in complaints
                            where complaint.DOCInt >= From && complaint.DOCInt <= To
                            join op in operators on complaint.OperatorID equals (op.OperatorID)
                            join ul in userList on complaint.UserID equals (ul.UserID)
                            join pl in providerList on complaint.ProviderID equals pl.ProviderID
                      
                        into providerDetails
                            from LProvider in providerDetails.DefaultIfEmpty()
                            orderby complaint.DOC descending
                            select new ViewModel
                            {
                                OperatorID = complaint.OperatorID,
                                Service = op.ServiceName,
                                Operator = op.OperatorName,
                                ProviderName = LProvider?.ProviderName ?? "",
                                UserID = complaint.UserID,
                                FirmName = ul.FirmName.ConcatNameMobileAliasName(ul.MobileNumber, ul.AliasName),
                                ComplaintMasterID = complaint.ComplaintMasterID,
                                ComplaintStatus = complaint.ComplaintStatus,
                                ComplaintType = complaint.ComplaintType ?? ComplaintType.Complaint,
                                Amount = complaint.Amount,
                                DateType = mainRequest.DateType,
                                ConsumerNumber = complaint.ConsumerNumber,
                                NewConsumerNumber = complaint.NewConsumerNumber ?? "",
                                CustomerRemark = complaint.CustomerRemark,
                                AdminRemark = complaint.AdminRemark,
                                TransactionMasterID = complaint.TransactionMasterID,
                                OldStatus =StaticModel.StatusType.Success,
                                Logo = op?.Path?.ConcatWebAPIURL(),
                                DOC = complaint.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                                TransactionDate = complaint.TransactionDate.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture) ?? "",
                                UpdatedDOC = complaint.UpdatedDOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                                ReferenceNo = complaint?.ReferenceNo ?? "",
                            }).ToList();
            if (!mainRequest.Status.EqualsIgnoreCase("All"))
            {
                response = (from item in response
                            where item.ComplaintStatus.EqualsIgnoreCase(mainRequest.Status)
                            select item).ToList();
            }
            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                response = (from item in response
                            where item.UserID == UserID
                            select item).ToList();
            }

            response = (from item in response

                        select item).Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();

            return response;
        }

        internal ViewModel SelectDetailByID(int complaintMasterID)
        {
            ViewModel response = null;
            List<Response.Operator.ViewModel> operators = new OperatorMasterDAL().Select();
            List<ComplaintMaster> complaints = SelectAll();
            List<UserMaster> userList = new UserDAL().Select();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            response = (from complaint in complaints
                        where complaint.ComplaintMasterID == complaintMasterID
                        join op in operators on complaint.OperatorID equals (op.OperatorID)
                        join ul in userList on complaint.UserID equals (ul.UserID)
                        join Provider in providerList on complaint.ProviderID equals Provider.ProviderID into providerdetail
                        from LP in providerdetail.DefaultIfEmpty()

                        orderby complaint.DOC descending
                        select new ViewModel
                        {
                            Service = op.ServiceName,
                            Operator = op.OperatorName,
                            IsBBPS = op.IsBBPS,
                            UserID = complaint.UserID,
                            FirmName = ul.FirmName.ConcatNameMobileAliasName(ul.MobileNumber, ul.AliasName),
                            ComplaintMasterID = complaint.ComplaintMasterID,
                            ComplaintStatus = complaint.ComplaintStatus,
                            Amount = complaint.Amount,
                            StringAmount = complaint.Amount.TwoDigitDecimal(),
                            ConsumerNumber = complaint.ConsumerNumber,
                            NewConsumerNumber = complaint.NewConsumerNumber ?? "",
                            ComplaintType = complaint.ComplaintType ?? ComplaintType.Complaint,
                            CustomerRemark = complaint.CustomerRemark,
                            AdminRemark = complaint.AdminRemark,
                            TransactionMasterID = complaint.TransactionMasterID,
                            DOC = complaint.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            TransactionDate = complaint.TransactionDate.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture) ?? complaint.TransactionMasterDOCInt.ToString(),
                            UpdatedDOC = complaint.UpdatedDOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            ProviderName = LP?.ProviderName ?? "",
                            SupportNumber = LP?.SupportNumber ?? "",
                            ProviderRequestID = complaint?.ProviderRequestID ?? "",
                        }).FirstOrDefault();

            return response;
        }

        internal async Task<DashboardComplaintViewModel> SelectDashboradComplaint(string Date)
        {
            int dateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            List<ComplaintMaster> complaintMainList = SelectAll();

            var mainList = (from complaint in complaintMainList
                            where complaint.ComplaintStatus.ToUpper() == StatusType.Process.ToUpper()
                            where complaint.DOCInt == dateInt
                            select new
                            {
                                Amount = complaint.Amount,
                                ComplaintStatus = complaint.ComplaintStatus
                            }).ToList();

            var complaintList = (from complaint in mainList
                                 group complaint by new { complaint.ComplaintStatus } into grp
                                 select new
                                 {
                                     Count = grp.Count(),
                                     Amount = grp.Sum(x => x.Amount),
                                 }).FirstOrDefault();

            DashboardComplaintViewModel response = new DashboardComplaintViewModel();

            response.ComplaintAmount = complaintList?.Amount ?? 0;
            response.ComplaintCount = complaintList?.Count ?? 0;
            await Task.Delay(1);
            return response;
        }

        internal decimal SelectTotalAmount(int From, int To, int UserID)
        {
            List<ComplaintMaster> complaintMainList = SelectAll();
            var CompalintMain = (from com in complaintMainList
                                 where com.TransactionMasterDOCInt >= From
                                 && com.TransactionMasterDOCInt <= To
                                 && com.UserID == UserID
                                 && com.ComplaintStatus.ToUpper() == ComplaintStatusType.Accept.ToUpper()
                                 select new
                                 {
                                     Amount = com.Amount,
                                 }).ToList();
            return CompalintMain?.Sum(X => X.Amount) ?? 0;
        }
    }
}