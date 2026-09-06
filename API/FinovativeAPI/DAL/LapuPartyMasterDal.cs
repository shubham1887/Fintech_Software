using Project.Model;
using Project.Processing;
using Project.Requests.LapuPartyMaster;
using Project.Response.LapuPartyMaster;
using Project.StaticModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class LapuPartyMasterDal
    {
        private readonly string key = "_LapuPartyMaster";

        internal LapuPartyMaster SelectDuplicate(InsertRequest lapuparty)
        {
            List<LapuPartyMaster> list = SelectAll();
            return (from lapu in list
                    where lapu.PartyName == lapuparty.PartyName && lapu.MobileNumber == lapuparty.MobileNumber
                        && lapu.EmailID == lapuparty.EmailID && lapu.IsActive == true
                    select lapu).FirstOrDefault();
        }

        internal List<LapuPartyMaster> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<LapuPartyMaster>>();

            List<LapuPartyMaster> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<LapuPartyMaster> SelectWithoutCache()

        {
            List<LapuPartyMaster> list = new List<LapuPartyMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.LapuPartyMaster.ToList();
            }
            return list;
        }

        internal int Insert(LapuPartyMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.LapuPartyMaster.Add(objModel);
            context.SaveChanges();
            return objModel.LapuPartyMasterID;
        }

        public int Update(LapuPartyMaster ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.LapuPartyMaster.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.LapuPartyMasterID;
        }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<LapuPartyMaster>>();
            cache.Remove(key);
        }

        internal List<ViewModel> Select()

        {
            using (var context = new DatabaseContext())
            {
                return (from lapu in context.LapuPartyMaster
                        where lapu.IsActive == true
                        orderby lapu.LapuPartyMasterID descending
                        select new ViewModel
                        {
                            LapuPartyMasterID = lapu.LapuPartyMasterID,
                            State = lapu.State,
                            PartyName = lapu.PartyName,
                            MobileNumber = lapu.MobileNumber,
                            EmailID = lapu.EmailID,
                            City = lapu.City,
                            Address = lapu.Address,
                            DOC = lapu.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        }).ToList();
            }
        }

        internal ViewModel SelectByMasterID(int LapuPartyMasterID)
        {
            List<LapuPartyMaster> lapuparty = SelectWithoutCache();
            List<ViewModel> response = null;
            List<StateMaster> stateList = new StateDAL().Select();
            using (var context = new DatabaseContext())
            {
                response = (from lapu in lapuparty
                            join state in stateList on lapu.State equals state.Name
                            where lapu.LapuPartyMasterID == LapuPartyMasterID && lapu.IsActive == true

                            select new ViewModel
                            {
                                LapuPartyMasterID = lapu.LapuPartyMasterID,
                                State = lapu.State,
                                StateID = state.ID,
                                PartyName = lapu.PartyName,
                                MobileNumber = lapu.MobileNumber,
                                EmailID = lapu.EmailID,
                                City = lapu.City,
                                Address = lapu.Address,
                                DOC = lapu.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            }).ToList();
            }

            return response.FirstOrDefault();
        }

        internal LapuPartyMaster SelectByID(int LapuPartyMasterID)
        {
            return SelectAll().FirstOrDefault(T => T.LapuPartyMasterID == LapuPartyMasterID && T.IsActive == true);
        }
    }
}