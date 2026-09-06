using Project.Model;
using Project.Processing;
using Project.Response.OperatorStateMapping;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OperatorStateMappingDal
    {
        internal void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(CacheKeys.OperatorStateMapping);
        }

        public int Insert(OperatorStateMapping ObjModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.OperatorStateMapping.Add(ObjModel);
                context.SaveChanges();
                return ObjModel.OperatorStateMappingID;
            }
        }

        internal int Update(OperatorStateMapping ObjModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.OperatorStateMapping.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.OperatorStateMappingID;
            }
        }

        internal OperatorStateMapping SelectByID(int operatorStateMappingID)
        {
            using (var context = new DatabaseContext())
            {
                return context.OperatorStateMapping.Find(operatorStateMappingID);
            }
        }

        public List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(CacheKeys.OperatorStateMapping, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from operatorState in context.OperatorStateMapping
                        join state in context.StateMaster on
                        operatorState.ZoneMasterID equals state.ID
                        where operatorState.IsDelete == false
                        select new ViewModel
                        {
                            OperatorID = operatorState.OperatorID,
                            OperatorStateMappingID = operatorState.OperatorStateMappingID,
                            OperatorName = operatorState.OperatorMaster.OperatorName,
                            StateID = operatorState.ZoneMasterID,
                            StateName = state.Name,
                            HLR = operatorState.HLR,
                            IsActive = operatorState.IsActive,
                            IsDelete = operatorState.IsDelete
                        }).ToList();
            }
        }

        internal ViewModel SelectByOperatorStateMappingID(int OperatorStateMappingID)
        {
            List<ViewModel> list = Select();

            return (from operatorStateMapping in list

                    where operatorStateMapping.OperatorStateMappingID == OperatorStateMappingID

                    && operatorStateMapping.IsDelete == false

                    select operatorStateMapping).FirstOrDefault();
        }

        internal ViewModel SelectByOperatorIDNumber(int operatorID, string Number)
        {
            ViewModel model = new ViewModel();

            if (Number.Length <= 4)
            {
                return model;
            }
            string strNumber = Number.Substring(0, 4);
            List<ViewModel> list = Select();
            return list.Where(l => l.OperatorID == operatorID && l.HLR.Contains(strNumber)).FirstOrDefault();
        }
    }
}