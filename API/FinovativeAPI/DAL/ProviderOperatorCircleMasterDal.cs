using Project.Model;
using Project.Processing;
using Project.Response.CircleSwitching;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderOperatorCircleMasterDal
    {
        public static string Key
        { get { return "_ProviderOperatorCircleMaster"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ProviderOperatorCircleMaster>>();
            cache.Remove(Key);
        }

        internal int Insert(ProviderOperatorCircleMaster ObjModel)
        {
            RemoveCache();
            int result = 1;
            try
            {
                using var context = new DatabaseContext();
                context.ProviderOperatorCircleMaster.Add(ObjModel);
                context.SaveChanges();
                result = ObjModel.ProviderOperatorCircleMasterID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal int Update(ProviderOperatorCircleMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderOperatorCircleMaster.Update(objModel);
            context.SaveChanges();
            return objModel.ProviderOperatorCircleMasterID;
        }

        internal int Delete(int operatorID, int providerID)
        {
            int result = 1;
            RemoveCache();
            List<ProviderOperatorCircleMaster> list = SelectByProviderOperatorID(operatorID, providerID);
            try
            {
                using var context = new DatabaseContext();
                context.ProviderOperatorCircleMaster.RemoveRange(list);
                result = context.SaveChanges();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return result;
        }

        internal List<ProviderOperatorCircleMaster> Select()
        {
            var _ProviderOperatorCircleList = new MemoryCacheProcessing<List<ProviderOperatorCircleMaster>>();
            List<ProviderOperatorCircleMaster> list = _ProviderOperatorCircleList.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<ProviderOperatorCircleMaster> SelectWithoutCache()
        {
            List<ProviderOperatorCircleMaster> list = new List<ProviderOperatorCircleMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = context.ProviderOperatorCircleMaster.ToList();
            }
            return list;
        }

        internal List<ProviderOperatorCircleMaster> SelectByProviderOperatorID(int operatorID, int providerID)
        {
            List<ProviderOperatorCircleMaster> list = Select();
            return list.Where(l => l.OperatorID == operatorID && l.ProviderID == providerID).ToList();
        }

        internal ProviderOperatorCircleMaster SelectByProviderOperatorIDStateID(int operatorID, int providerID, int stateID)
        {
            List<ProviderOperatorCircleMaster> list = Select();
            return list.Where(l => l.OperatorID == operatorID && l.ProviderID == providerID && 
                              l.StateID == stateID && l.IsActive == true)
                       .FirstOrDefault();
        }

        internal List<ProviderOperatorCircleMaster> SelectByProviderOperatorIDStateIDList(int operatorID, int providerID)
        {
            List<ProviderOperatorCircleMaster> list = Select();
            return list.Where(l => l.OperatorID == operatorID && l.ProviderID == providerID && l.IsActive == true).ToList();
        }

        internal List<Project.Response.User.StateViewModel> SelectCircleListByProviderIDOperatorID(int OperatorID, int ProviderID)
        {
            List<ProviderOperatorCircleMaster> mainList = SelectByProviderOperatorID(OperatorID, ProviderID);

            List<ZoneMaster> stateList = new ZoneMasterDAL().Select();

            List<Project.Response.User.StateViewModel> list = (from pc in mainList
                                                               join state in stateList on pc.StateID equals state.ZoneMasterID
                                                               where pc.OperatorID == OperatorID
                                                               select new Project.Response.User.StateViewModel
                                                               {
                                                                   StateID = pc.StateID,
                                                                   Name = state.Name
                                                               }).ToList();
            return list;
        }

        internal List<ViewModel> SelectCircleSwitching()
        {
            List<ViewModel> response = new List<ViewModel>();
            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<ProviderOperatorCircleMaster> list = Select().Where(s => s.IsActive == true).ToList();

            return (from item in list
                    join op in operatorList on item.OperatorID equals op.OperatorID
                    join provider in providerList on item.ProviderID equals provider.ProviderID
                    select new ViewModel
                    {
                        ProviderOperatorCircleMasterID = item.ProviderOperatorCircleMasterID,
                        OperatorID = item.OperatorID,
                        Operator = op.OperatorName,
                        ProviderID = item.ProviderID,
                        ProviderName = provider.ProviderName,
                        StateID = item.StateID
                    }).ToList();
        }

        internal List<OperatorCircleMaster> SelectCircles(int OperatorID, int ProviderID)
        {
            List<ProviderOperatorCircleMaster> list = Select().Where(s => s.IsActive == true).ToList();
            List<ZoneMaster> circles = new ZoneMasterDAL().Select();
            return (from item in list
                    join circle in circles on item.StateID equals circle.ZoneMasterID
                    where item.OperatorID == OperatorID
                    && item.ProviderID == ProviderID
                    select new OperatorCircleMaster
                    {
                        CircleID = item.StateID,
                        CircleName = circle.Name,
                        Opcode = item.Opcode,
                        IsActive = true,
                    }).Distinct().ToList();
        }

        internal List<ProviderOperatorCircleMaster> SelectByID(int operatorID)
        {
            List<ProviderOperatorCircleMaster> list = Select().Where(s => s.OperatorID == operatorID && s.IsActive == true).ToList();
            return list;
        }

        internal int Delete(List<ProviderOperatorCircleMaster> objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderOperatorCircleMaster.RemoveRange(objModel);

            return context.SaveChanges();
        }
    }
}