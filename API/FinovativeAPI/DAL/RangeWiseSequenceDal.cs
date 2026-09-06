using Project.Model;
using Project.Processing;
using Project.Requests.RangeWiseSequence;
using Project.Response.RangeWiseSequence;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class RangeWiseSequenceDal
    {
        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(CacheKeys.RangeWiseSequence);
        }

        internal int Insert(RangeWiseSequence objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.RangeWiseSequence.Add(objModel);
            return context.SaveChanges();
        }

        internal int Update(RangeWiseSequence objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.RangeWiseSequence.Update(objModel);
            return context.SaveChanges();
        }

        internal RangeWiseSequence SelectByID(int RangeWiseSequenceID)
        {
            using var context = new DatabaseContext();
            return context.RangeWiseSequence.Find(RangeWiseSequenceID);
        }

        internal ViewModel SelectByRangeWiseSequenceID(int RangeWiseSequenceID)
        {
            using var context = new DatabaseContext();
            return (from rangeWiseSequence in context.RangeWiseSequence
                    where rangeWiseSequence.RangeWiseSequenceID == RangeWiseSequenceID
                                               && rangeWiseSequence.IsDelete == false
                    select new ViewModel
                    {
                        RangeWiseSequenceID = rangeWiseSequence.RangeWiseSequenceID,
                        ProviderID = rangeWiseSequence.ProviderID,
                        ServiceID = rangeWiseSequence.ServiceID,
                        OperatorID = rangeWiseSequence.OperatorID,
                        FromAmount = rangeWiseSequence.FromAmount,
                        ToAmount = rangeWiseSequence.ToAmount,
                    }).FirstOrDefault();
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(CacheKeys.RangeWiseSequence, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return (from rangeWiseSequence in context.RangeWiseSequence
                    where rangeWiseSequence.IsDelete == false
                    orderby rangeWiseSequence.ProviderID descending
                    select new ViewModel
                    {
                        RangeWiseSequenceID = rangeWiseSequence.RangeWiseSequenceID,
                        ProviderID = rangeWiseSequence.ProviderID,
                        ServiceID = rangeWiseSequence.ServiceID,
                        OperatorID = rangeWiseSequence.OperatorID,
                        Operator = rangeWiseSequence.OperatorMaster.OperatorName,
                        ProviderName = rangeWiseSequence.ProviderMaster.ProviderName,
                        ServiceName = rangeWiseSequence.ServiceMaster.Name,
                        FromAmount = rangeWiseSequence.FromAmount,
                        ToAmount = rangeWiseSequence.ToAmount,
                    }).ToList();
        }

        internal RangeWiseSequence SelectForDuplicate(RangeWiseSequenceInsertRequest insertRequest)
        {
            using var context = new DatabaseContext();
            return (from rangeWiseSequence in context.RangeWiseSequence

                    where rangeWiseSequence.ProviderID == insertRequest.ProviderID
                    && rangeWiseSequence.OperatorID == insertRequest.OperatorID
                    && rangeWiseSequence.FromAmount == insertRequest.FromAmount
                    && rangeWiseSequence.ToAmount == insertRequest.ToAmount
                            && rangeWiseSequence.IsDelete == false
                    select new RangeWiseSequence
                    {
                        RangeWiseSequenceID = rangeWiseSequence.RangeWiseSequenceID,
                        ProviderID = rangeWiseSequence.ProviderID,
                        ServiceID = rangeWiseSequence.ServiceID,
                        OperatorID = rangeWiseSequence.OperatorID,
                        FromAmount = rangeWiseSequence.FromAmount,
                        ToAmount = rangeWiseSequence.ToAmount,
                    }).FirstOrDefault();
        }

        internal RangeWiseSequence CheckIfTwoRangeValuesExistsBetSequence(RangeWiseSequenceInsertRequest insertRequest, decimal Amount)
        {
            using var context = new DatabaseContext();
            RangeWiseSequence trans = null;

            if (insertRequest.RangeWiseSequenceID <= 0)
            {
                trans = context.RangeWiseSequence
                   .FirstOrDefault(t => t.ProviderID == insertRequest.ProviderID &&
                                                                                   t.OperatorID == insertRequest.OperatorID &&
                                                                                   t.IsDelete == false
                                                                                   && (t.FromAmount <= Amount && Amount <= t.ToAmount));
            }

            if (insertRequest.RangeWiseSequenceID > 0)
            {
                trans = context.RangeWiseSequence
                   .FirstOrDefault(t => t.ProviderID == insertRequest.ProviderID &&
                                                                                   t.OperatorID == insertRequest.OperatorID &&
                                                                                   t.IsDelete == false &&
                                                                                   t.RangeWiseSequenceID != insertRequest.RangeWiseSequenceID
                                                                                   && (t.FromAmount <= Amount && Amount <= t.ToAmount));
            }

            return trans;
        }

        internal RangeWiseSequence SelectRangeWiseProviderID(int OperatorID, decimal Amount)
        {
            using var context = new DatabaseContext();
            RangeWiseSequence trans = context.RangeWiseSequence
                .FirstOrDefault(t => t.OperatorID == OperatorID &&
                        t.IsDelete == false
                          && (t.FromAmount <= Amount && Amount <= t.ToAmount));

            return trans;
        }
    }
}