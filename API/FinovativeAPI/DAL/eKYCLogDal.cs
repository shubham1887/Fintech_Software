using Project.Model;
using Project.Processing;
using Project.Requests.eKYCLog;
using Project.Response.eKYCLog;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class eKYCLogDal
    {
        internal int Insert(eKYCLog ObjModel)
        {
            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.eKYCLog.Add(ObjModel);
                context.SaveChanges();
                Result = ObjModel.eKYCLogID;
            }
            return Result;
        }

        internal ViewModel SelectByUserID(string DocumentType, int UserID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return (from item in context.eKYCLog
                        where item.UserID == UserID
                        && item.DocumentType == DocumentType
                        orderby item.eKYCLogID descending
                        select new ViewModel
                        {
                            UserID = item.UserID,
                            DocumentType = item.DocumentType,
                            Response = item.Response
                        }).FirstOrDefault();
            }
        }

        internal bool IsCheckPanVerification(int UserID)
        {
            eKYCLog obj = new eKYCLog();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                obj = (from item in context.eKYCLog
                       where item.DocumentType == eKYCDocType.Pan
                                && item.UserID == UserID
                       select item).FirstOrDefault();
            }
            if (obj == null)
                return true;
            return false;
        }

        internal bool IsCheckGSTVerification(int UserID)
        {
            eKYCLog obj = new eKYCLog();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                obj = (from item in context.eKYCLog
                       where item.DocumentType == eKYCDocType.GST
                                && item.UserID == UserID
                       select item).FirstOrDefault();
            }
            if (obj == null)
                return true;
            return false;
        }

        internal List<Response.eKYC.ViewModel> Select(SelectRequest selectRequest, int UserID)
        {
            List<Response.eKYC.ViewModel> list = null;
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = (from item in context.eKYCLog
                        join User in context.UserMaster on item.UserID equals User.UserID
                        where item.DOCInt >= FromDate
                             && item.DOCInt <= ToDate
                         && (!selectRequest.DocumentType.EqualsIgnoreCase("All") ? item.DocumentType == selectRequest.DocumentType : 2 > 1)
                        orderby item.eKYCLogID descending
                        select new Response.eKYC.ViewModel
                        {
                            UserID = item.UserID,
                            FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                            DOC = item.DOC.LongFormatDate(),
                            DocumentType = item.DocumentType,
                            DocumentNo = item.DocumentNo,
                            Response = item.Response,
                            MobileNumber = User.MobileNumber,
                            EmailID = User.EmailID,
                            PANNo = User.PANNo,
                            ProfileImage = String.Concat(Company.WebAPIURL, User.ProfileImagePath),
                        }).ToList();
            }
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                list = (from item in list
                        where item.UserID == UserID
                        select item).ToList();
                return list;
            }

            if (selectRequest.UserID > 0)
            {
                list = (from item in list
                        where item.UserID == selectRequest.UserID
                        select item).ToList();
                return list;
            }

            return list;
        }

        internal void Delete(int userID)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.eKYCLog.Where(U => U.UserID == userID);
                context.eKYCLog.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}