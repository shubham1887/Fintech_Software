using Project.Model;
using Project.Processing;
using Project.Response.KYC;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class UserDocumentMasterDal
    {
        public static readonly string Key = "_UserDocumentMaster";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UserDocumentMaster>>();
            cache.Remove(Key);
        }

        internal int Insert(UserDocumentMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserDocumentMaster.Add(objModel);
            context.SaveChanges();
            return objModel.UserDocumentID;
        }

        internal int Update(UserDocumentMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserDocumentMaster.Update(objModel);
            context.SaveChanges();
            return objModel.UserDocumentID;
        }

        internal List<UserDocumentMaster> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<UserDocumentMaster>>();

            List<UserDocumentMaster> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<UserDocumentMaster> SelectWithoutCache()
        {
            List<UserDocumentMaster> list;
            using (var context = new DatabaseContext())
            {
                list = (from item in context.UserDocumentMaster
                        orderby item.UserDocumentID descending
                        select item).ToList();
            }
            return list;
        }

        internal bool SelectByUserID(int userID, string documentType)
        {
            List<UserDocumentMaster> listAll = SelectAll();

            UserDocumentMaster list = (from documents in listAll
                                       where documents.UserID == userID
                                       && documents.DocmentType == documentType
                                       && documents.Status == StatusType.Approve
                                       select documents).FirstOrDefault();

            if (list == null) return false;

            return true;
        }

        internal UserDocumentMaster SelectByUserAndDocType(int userID, string documentType)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return context.UserDocumentMaster.FirstOrDefault(U => U.UserID == userID && U.DocmentType == documentType);
            }
        }

        internal List<ViewModel> Select(Project.Requests.KYC.SelectRequest mainRequest)
        {
            List<ViewModel> list = new List<ViewModel>();

            List<UserDocumentMaster> UserIDList = SelectAll().Distinct().ToList();

            UserIDList = UserIDList.GroupBy(x => x.UserID).Select(x => x.FirstOrDefault()).ToList();

            List<UserMaster> userList = new UserDAL().Select();
            List<UserMaster> userList1 = new UserDAL().Select();
            List<UserMaster> userList2 = new UserDAL().Select();
            List<Response.KYCApprove.ViewModel> KYClist = new KYCApprovedDal().Select();

            var Mailist = (from u in userList
                           join p in userList1 on u.ParentID equals p.UserID
                           join d in UserIDList on u.UserID equals d.UserID
                           join KYCApprove in KYClist on u.UserID equals KYCApprove.ApproveUserID
                           into KYCDetail
                           from LFKYC in KYCDetail.DefaultIfEmpty()
                           select new
                           {
                               PersonName = u.PersonName,
                               Name = u.FirmName.ConcatNameMobileAliasName(u.MobileNumber, u.AliasName),
                               Parent = p.FirmName.ConcatNameMobileAliasName(p.MobileNumber, p.AliasName),
                               UserID = u.UserID,
                               ISApproved = u.IsKYCApproved,
                               RequestDate = d.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                               ApproveDate = u.IsKYCApproved ? d.UpdatedDOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture) : "",
                               Status = d.Status,
                               Remark = d.Remark,
                               ApprovedImageURL = String.Concat(Company.WebAPIURL, d.ImageURL),
                               ApprovedUserID = LFKYC?.UserID ?? 0

                           }).Distinct().ToList();

            list = (from u in Mailist
                    join User in userList2 on u.ApprovedUserID equals User.UserID
                    into UserDetail
                    from LFUser in UserDetail.DefaultIfEmpty()
                    select new ViewModel
                    {
                        PersonName = u.PersonName,
                        Name = u.Name,
                        Parent = u.Parent,
                        UserID = u.UserID,
                        ISApproved = u.ISApproved,
                        RequestDate = u.RequestDate,
                        ApproveDate = u.ApproveDate,
                        Status = u.Status,
                        Remark = u.Remark,
                        KYCApproveName = LFUser?.FirmName.ConcatNameMobileAliasName(LFUser.MobileNumber, LFUser.AliasName) ?? "",
                        ApprovedImageURL = u.ApprovedImageURL,
                    }).Distinct().ToList();


            if (mainRequest.Status.EqualsIgnoreCase("Pending"))
            {
                list = (from item in list
                        where item.Status == StatusType.Process
                        select item).ToList();

                return list;
            }

            if (mainRequest.Status.EqualsIgnoreCase("Rejected"))
            {
                list = (from item in list
                        where item.Status == StatusType.Reject
                        select item).ToList();

                return list;
            }

            if (mainRequest.Status.EqualsIgnoreCase("Approved"))
            {
                bool IsApproved = false;
                IsApproved = mainRequest.Status.EqualsIgnoreCase("Approved");

                list = (from item in list
                        where item.ISApproved == IsApproved
                        select item).ToList();
            }

            list = list.OrderByDescending(item =>
                    DateTime.TryParseExact(item.RequestDate, Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date) ? date : DateTime.MinValue
                ).ToList();

            return list;
        }

        internal async Task<int> SelectPendingKYCCount()
        {
            List<UserDocumentMaster> listAll = SelectAll();

            List<UserMaster> listUser = new UserDAL().Select();

            List<int> KYCUploadedUserIDList = listAll.Where(u => u.Status == StatusType.Process).Select(u => u.UserID).ToList();

            List<int> KYCNotApprovedUserIDList = listUser.Where(u => u.IsKYCApproved == false && u.IsActive == true).Select(u => u.UserID).ToList();

            KYCUploadedUserIDList.Distinct();
            IEnumerable<int> res = KYCNotApprovedUserIDList.AsQueryable().Intersect(KYCUploadedUserIDList);
            int PendingKYCCount = res.Count();

            await Task.Delay(1);
            return PendingKYCCount;
        }

        internal List<ViewModel> SelectDocumentsByUserID(int userID)
        {
            List<ViewModel> list = new List<ViewModel>();
            List<UserDocumentMaster> listDocuments = SelectAll().Where(u => u.UserID == userID).ToList();
            List<Project.Response.KycDocumentMaster.ViewModel> listKycDocument = new KycDocumentMasterDal().Select();
            List<UserMaster> usersList = new UserDAL().Select().Where(u => u.UserID == userID).ToList();

            list = (from documents in listDocuments
                    join user in usersList on documents.UserID equals user.UserID
                    join kyc in listKycDocument on documents.DocmentType equals kyc.DocumentType
                    where documents.UserID == userID
                    select new ViewModel
                    {
                        UserID = documents.UserID,
                        Name = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                        WhatsAppNumber = user.WhatsAppNumber,
                        Status = documents.Status,
                        DocumentNo = documents.DocumentNo,
                        DocumentType = documents.DocmentType,
                        UserDocumentID = documents.UserDocumentID,
                        Remark = documents.Remark,
                        Document = String.Concat(Company.WebAPIURL, documents.ImageURL),
                        ISApproved = user.IsKYCApproved,
                        SampleImageURL = kyc.SampleImageURL.ConcatWebAPIURL(),
                        ApprovedImageURL = String.Concat(Company.WebAPIURL, documents.ImageURL),
                        Label = kyc.Label
                    }).ToList();
            return list;
        }

        internal ViewModel SelectByID(int UserDocumentID)
        {
            List<ViewModel> list = new List<ViewModel>();

            List<UserDocumentMaster> listDocuments = SelectAll().Where(u => u.UserDocumentID == UserDocumentID).ToList();
            List<UserMaster> usersList = new UserDAL().Select();

            list = (from documents in listDocuments
                    join user in usersList on documents.UserID equals user.UserID
                    where documents.UserDocumentID == UserDocumentID
                    select new ViewModel
                    {
                        UserID = documents.UserID,
                        Name = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                        Status = documents.Status,
                        DocumentNo = documents.DocumentNo,
                        DocumentType = documents.DocmentType,
                        UserDocumentID = documents.UserDocumentID,
                        Document = String.Concat(Company.WebAPIURL, documents.ImageURL),
                        ISApproved = user.IsKYCApproved,
                        Remark = documents.Remark,
                    }).ToList();
            return list.FirstOrDefault();
        }

        internal UserDocumentMaster SelectByKYCID(int UserDocumentID)
        {
            List<UserDocumentMaster> list = SelectAll();

            return list.FirstOrDefault(U => U.UserDocumentID == UserDocumentID);
        }

        internal async Task<KycCountViewModel> SelectKycPendingCount()
        {
            List<UserMaster> userList = new UserDAL().Select();
            List<UserDocumentMaster> docList = SelectAll();

            var Mainlist = (from Doc in docList
                            join user in userList on Doc.UserID equals user.UserID
                            where user.IsKYCApproved == false
                            select new
                            {
                                IsKYCApproved = user.IsKYCApproved,
                            }).Count();

            KycCountViewModel response = new KycCountViewModel();
            response.KycCount = Mainlist;
            await Task.Delay(1);
            return response;
        }

        internal List<ViewModel> SelectKYCPendingUsers()
        {
            List<ViewModel> list = new List<ViewModel>();

            List<UserDocumentMaster> UserIDList = SelectAll().Where(item => item.Status == StatusType.Process).Distinct().ToList();

            UserIDList = UserIDList.GroupBy(x => x.UserID).Select(x => x.FirstOrDefault()).ToList();
            var uniqueList = UserIDList.Distinct();

            List<UserMaster> userList = new UserDAL().Select();
            List<UserMaster> userList1 = new UserDAL().Select();

            list = (from u in userList1
                    join p in userList on u.ParentID equals p.UserID
                    join d in UserIDList on u.UserID equals d.UserID
                    where u.IsKYCApproved == false
                    select new ViewModel
                    {
                        PersonName = u.PersonName,
                        Name = u.FirmName.ConcatNameMobileAliasName(u.MobileNumber, u.AliasName),
                        Parent = p.FirmName.ConcatNameMobileAliasName(p.MobileNumber, p.AliasName),
                        UserID = u.UserID,
                        UserDocumentID = d.UserDocumentID,
                        ISApproved = u.IsKYCApproved,
                        RequestDate = d.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        ApproveDate = u.IsKYCApproved ? d.UpdatedDOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture) : "",
                        Remark = d.Remark,
                    }).Distinct().ToList();
            return list;
        }

        internal List<KYCViewModel> SelectActiveDocUserwise(int UserID)
        {
            List<KYCViewModel> returnlist = new List<KYCViewModel>();
            List<Response.KycDocumentMaster.ViewModel> ActiveDOClist = new KycDocumentMasterDal().SelectRequired();

            List<ViewModel> list = new UserDocumentMasterDal().SelectDocumentsByUserID(UserID);

            returnlist = (from documents in ActiveDOClist
                          join currentkyc in list on documents.DocumentType equals currentkyc.DocumentType into leftData
                          from currentList in leftData.DefaultIfEmpty()
                          orderby documents.KycDocumentMasterID ascending
                          select new KYCViewModel
                          {
                              KycDocumentMasterID = documents.KycDocumentMasterID,
                              SampleImageURL = documents.SampleImageURL,
                              DocumentType = documents?.DocumentType ?? "",
                              Status = currentList?.Status ?? "",
                              DocumentNo = currentList?.DocumentNo ?? "",
                              UserDocumentID = currentList?.UserDocumentID ?? 0,
                              ApprovedImageURL = currentList?.Document ?? "",
                              Label = documents.Label
                          }).ToList();
            return returnlist;
        }

        internal UserDocumentMaster SelectByImageURL(string path)
        {
            List<UserDocumentMaster> list = SelectAll();
            return list.FirstOrDefault(U => U.ImageURL == path);
        }
    }
}