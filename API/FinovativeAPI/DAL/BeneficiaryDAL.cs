using Project.Model;
using Project.Processing;
using Project.Requests.Beneficiary;
using Project.Response.Beneficiary;
using Project.Response.Sender;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class BeneficiaryDAL
    {
        private readonly string key = "_BeneficiaryDAL";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<Beneficiary>>();
            cache.Remove(key);
        }

        internal List<Beneficiary> Select()
        {
            var _List = new MemoryCacheProcessing<List<Beneficiary>>();
            List<Beneficiary> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;
             
        }

        private List<Beneficiary> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.Beneficiary.ToList();
            }
        }

        public int Insert(Beneficiary ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.Beneficiary.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.BeneficiaryID;
            }
        }

        internal int Update(Beneficiary ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.Beneficiary.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.BeneficiaryID;
            }
        }

        internal int Delete(Beneficiary ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.Beneficiary.Remove(ObjModel);
            context.SaveChanges();
            return ObjModel.BeneficiaryID;
        }

        internal Beneficiary SelectByAccountIFSCSender(string AccountNo, string IFSC, int SenderID)
        {
            List<Beneficiary> list = Select();
            Beneficiary obj;

            obj = (from bene in list
                   where bene.AccountNo == AccountNo
                   && bene.IFSC == IFSC
                   && bene.SenderID == SenderID
                   && bene.IsActive == true
                   orderby bene.BeneficiaryID descending
                   select bene).FirstOrDefault();

            return obj;
        }

        internal string SelectBenificiaryNameByID(string BeneficiaryID)
        {
            List<Beneficiary> list = Select();

            Beneficiary Bene = list.FirstOrDefault(B => B.GUID == BeneficiaryID);
            if (Bene == null)
            {
                return BeneficiaryID;
            }

            return Bene.Name;
        }

        internal Beneficiary SelectByGUID(string guid)
        {
            List<Beneficiary> list = Select();
            Beneficiary obj;

            obj = (from bene in list
                   where bene.GUID == guid
                   && bene.IsActive == true
                   orderby bene.BeneficiaryID descending
                   select bene).FirstOrDefault();

            return obj;
        }

        internal List<Beneficiary> SelectBySenderID(int SenderID)
        {
            List<Beneficiary> list = Select();

            List<Beneficiary> obj;

            obj = (from bene in list
                   where bene.SenderID == SenderID
                   && bene.IsActive == true && bene.IsOTPVerified == true
                   orderby bene.BeneficiaryID descending
                   select bene).ToList();

            return obj;
        }

        internal int SelectAllBeneficiary(int SenderID)
        {
            List<Beneficiary> list = Select();

            List<Beneficiary> obj;

            obj = (from bene in list
                   where bene.SenderID == SenderID
                   select bene).ToList();

            return obj.Count;
        }

        internal Beneficiary SelectByID(int BeneficiaryID)
        {
            using var context = new DatabaseContext();
            return context.Beneficiary.Find(BeneficiaryID);
        }

        internal List<SelectByAccountNumberResponse> SelectByAccountNumber(string accountNumber)
        {
            List<Beneficiary> list = Select();

            List<Sender> senderList = new SenderDAL().Select();

            List<SelectByAccountNumberResponse> obj;

            obj = (from bene in list
                   join sender in senderList on
                   bene.SenderID equals sender.SenderID
                   where bene.AccountNo == accountNumber
                   && bene.IsActive == true
                   orderby bene.BeneficiaryID descending
                   select new SelectByAccountNumberResponse
                   {
                       SenderName = sender.Name,
                       SenderNumber = sender.Mobile
                   }).Distinct().ToList();

            return obj;
        }

        internal Beneficiary SelectBeneficiaryIsNameVerified(string accountNo, string iFSC)
        {
            List<Beneficiary> list = Select();
            Beneficiary obj;

            obj = (from bene in list
                   where bene.AccountNo == accountNo
                   && bene.IFSC == iFSC
                   orderby bene.BeneficiaryID descending
                   select bene).FirstOrDefault();

            return obj;
        }

        internal List<BeneficiaryViewModel> SelectAllBene(BeneficiaryReportRequest mainRequest)
        {
            List<Beneficiary> list = Select();
            List<Sender> senderList = new SenderDAL().Select();

            List<BeneficiaryViewModel> obj;

            obj = (from bene in list
                   join sender in senderList on
                   bene.SenderID equals sender.SenderID

                   orderby sender.SenderID descending
                   select new BeneficiaryViewModel
                   {
                       Name = bene.Name,
                       AccountNo = bene.AccountNo,
                       IFSC = bene.IFSC,
                       GUID = bene.GUID,
                       SenderID = bene.SenderID,
                       BeneficiaryID = bene.BeneficiaryID,
                       IsOTPVerified = bene.IsOTPVerified,
                       IsActive = bene.IsActive,
                       IsNameVerified = bene.IsNameVerified,
                       Sender = sender.Name,
                       SenderMobile = sender.Mobile,
                   }).ToList();

            return obj;
        }
    }
}