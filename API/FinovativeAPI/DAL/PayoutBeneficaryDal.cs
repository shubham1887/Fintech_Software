using Project.Model;
using Project.Processing;
using Project.Requests.PayoutBeneficiary;
using Project.Response.PayoutBeneficiary;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Poject.DAL
{
    public class PayoutBeneficaryDal
    {
        private readonly string key = "_PayoutBeneficiary";

        public int Insert(PayoutBeneficiary ObjModel)
        {
            using (var context = new DatabaseContext())
            {
                context.PayoutBeneficiary.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.PayoutBeneficaryID;
            }
        }

        internal int Update(PayoutBeneficiary ObjModel)
        {
            using (var context = new DatabaseContext())
            {
                context.PayoutBeneficiary.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.PayoutBeneficaryID;
            }
        }

        internal PayoutBeneficiary SelectByAccountIFSCSender(string AccountNo, string IFSC, int UserID)
        {
            PayoutBeneficiary obj;
            using (var context = new DatabaseContext())
            {
                obj = (from bene in context.PayoutBeneficiary
                       where bene.AccountNo == AccountNo && bene.IsOTPVerified == true
                       && bene.IFSC == IFSC
                       && bene.IsDelete == false
                       && bene.UserID == UserID
                       orderby bene.PayoutBeneficaryID descending
                       select bene).FirstOrDefault();
            }
            return obj;
        }

        internal PayoutBeneficiary SelectBeneByAccountIFSC(string AccountNo, string IFSC)
        {
            PayoutBeneficiary obj;
            using (var context = new DatabaseContext())
            {
                obj = (from bene in context.PayoutBeneficiary
                       where bene.AccountNo == AccountNo
                       && bene.IFSC == IFSC
                       && bene.IsDelete == false
                       orderby bene.PayoutBeneficaryID descending
                       select bene).FirstOrDefault();
            }
            return obj;
        }

        internal string SelectBenificiaryNameByID(int BeneficiaryID)
        {
            using var context = new DatabaseContext();
            PayoutBeneficiary Bene = context.PayoutBeneficiary.FirstOrDefault(B => B.PayoutBeneficaryID == BeneficiaryID);
            if (Bene == null)
            {
                return "";
            }

            return Bene.AccountHolderName;
        }

        internal PayoutBeneficiary SelectByID(int PayoutBeneficaryID)
        {
            using var context = new DatabaseContext();
            return context.PayoutBeneficiary.Find(PayoutBeneficaryID);
        }

        internal int Delete(PayoutBeneficiary ObjModel)
        {
            using var context = new DatabaseContext();
            context.PayoutBeneficiary.Remove(ObjModel);
            context.SaveChanges();
            return ObjModel.PayoutBeneficaryID;
        }

        internal PayoutBeneficiary SelectBeneficiaryIsNameVerified(string accountNo, string iFSC)
        {
            PayoutBeneficiary obj;
            using (var context = new DatabaseContext())
            {
                obj = (from bene in context.PayoutBeneficiary
                       where bene.AccountNo == accountNo
                       && bene.IFSC == iFSC
                       orderby bene.PayoutBeneficaryID descending
                       select bene).FirstOrDefault();
            }
            return obj;
        }

        internal int SelectBeneficiaryCountByUserID(int UserID)
        {
            List<PayoutBeneficiary> Bene;
            using (var context = new DatabaseContext())
            {
                Bene = context.PayoutBeneficiary.Where(S => S.UserID == UserID && S.Status == StatusType.Approve).ToList();
            }
            if (Bene == null)
            {
                return 0;
            }

            return Bene.Count();
        }

        internal List<BeneficiaryViewModel> SelectAllBene(PayoutBeneficiaryReportRequest mainRequest, int UserID)
        {
            List<BeneficiaryViewModel> obj;
            using var context = new DatabaseContext();
            obj = (from bene in context.PayoutBeneficiary
                   join user in context.UserMaster
                   on
                   bene.UserID equals user.UserID
                   where bene.IsOTPVerified == true

                   orderby bene.UserID descending, bene.PayoutBeneficaryID descending
                   select new BeneficiaryViewModel
                   {
                       UserID = bene.UserID,
                       Status = bene.Status,
                       BankName = bene.BankName,
                       Name = bene.AccountHolderName,
                       FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                       AccountNo = bene.AccountNo,
                       IFSC = bene.IFSC,
                       BeneficiaryID = bene.PayoutBeneficaryID,
                       IsOTPVerified = bene.IsOTPVerified,
                       IsNameVerified = bene.IsNameVerified,
                       Document = bene.Document.ConcatWebAPIURL()
                   }).ToList();

            if (mainRequest.UserID > Company.AdminUserID)
            {
                obj = (from item in obj
                       where item.UserID == mainRequest.UserID
                       select item).ToList();
            }

            if (!mainRequest.Status.EqualsIgnoreCase(StatusType.ALL))
            {
                obj = (from item in obj
                       where item.Status == mainRequest.Status
                       select item).ToList();
            }

            return obj;
        }

        internal List<PayoutBeneficiary> Select()
        {
            var _List = new MemoryCacheProcessing<List<PayoutBeneficiary>>();
            List<PayoutBeneficiary> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<PayoutBeneficiary> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.PayoutBeneficiary.ToList();
            }
        }
    }
}