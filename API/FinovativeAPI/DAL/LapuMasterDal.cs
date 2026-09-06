using Project.Model;
using Project.Processing;
using Project.Response.LapuMaster;
using Project.StaticModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class LapuMasterDal
    {
        private readonly string key = "_LapuMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<LapuPartyMaster>>();
            cache.Remove(key);
        }

        internal List<LapuMaster> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<LapuMaster>>();

            List<LapuMaster> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<LapuMaster> SelectWithoutCache()

        {
            List<LapuMaster> list = new List<LapuMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.LapuMaster.ToList();
            }
            return list;
        }

        internal int Insert(LapuMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.LapuMaster.Add(objModel);
            context.SaveChanges();
            return objModel.LapuMasterID;
        }

        public int Update(LapuMaster ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.LapuMaster.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.LapuMasterID;
        }

        internal LapuMaster SelectByID(int LapuMasterID)
        {
            return SelectAll().FirstOrDefault(T => T.LapuMasterID == LapuMasterID);
        }

        internal LapuMaster SelectByOperatorIDAndLapuNo(int OperatorID, string LapuNo)
        {
            List<LapuMaster> list = new List<LapuMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.LapuMaster.ToList();
            }
            return list.FirstOrDefault(T => T.OperatorID == OperatorID && T.LapuNo == LapuNo);
        }

        internal LapuMaster SelectDuplicate(int OperatorID, string LapuNo, int LapuMasterID)
        {
            List<LapuMaster> list = SelectAll();
            return (from lapu in list
                    where lapu.LapuNo == LapuNo && lapu.OperatorID == OperatorID
                    select lapu).FirstOrDefault(T => T.LapuMasterID != LapuMasterID);
        }

        internal int Delete(LapuMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.LapuMaster.Remove(objModel);
            return context.SaveChanges();
        }

        internal List<ViewModel> Select()

        {
            using (var context = new DatabaseContext())
            {
                return (from lapu in context.LapuMaster
                        orderby lapu.LapuMasterID descending

                        select new ViewModel
                        {
                            LapuMasterID = lapu.LapuMasterID,
                            LapuPartyMasterID = lapu.LapuPartyMasterID,
                            PartyName = lapu.LapuPartyMaster.PartyName,
                            OperatorID = lapu.OperatorID,
                            OperatorName = lapu.OperatorMaster.OperatorName,
                            LapuNo = lapu.LapuNo,
                            ProivderLapuID = lapu.ProivderLapuID,
                            ClosingBalance = lapu.ClosingBalance,
                            DOC = lapu.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        }).ToList();
            }
        }

        internal ViewModel SelectByMasterID(int LapuMasterID)
        {
            List<LapuMaster> lapuparty = SelectWithoutCache();
            List<ViewModel> response = null;
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll();
            List<LapuPartyMaster> lapuList = new LapuPartyMasterDal().SelectAll();
            using (var context = new DatabaseContext())
            {
                response = (from lapu in lapuparty
                            join operators in operatorList on lapu.OperatorID equals operators.OperatorID
                            join lapupartys in lapuList on lapu.LapuPartyMasterID equals lapupartys.LapuPartyMasterID
                            where lapu.LapuMasterID == LapuMasterID

                            select new ViewModel
                            {
                                LapuMasterID = lapu.LapuMasterID,
                                LapuPartyMasterID = lapu.LapuPartyMasterID,
                                PartyName = lapupartys.PartyName,
                                OperatorID = lapu.OperatorID,
                                OperatorName = operators.OperatorName,
                                LapuNo = lapu.LapuNo,
                                ProivderLapuID = lapu.ProivderLapuID,
                                ClosingBalance = lapu.ClosingBalance,
                                DOC = lapu.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            }).ToList();
            }

            return response.FirstOrDefault();
        }
    }
}