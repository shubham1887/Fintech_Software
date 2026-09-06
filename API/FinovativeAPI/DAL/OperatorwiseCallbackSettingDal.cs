using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OperatorwiseCallbackSettingDal
    {
        public static string Key = "_OperatorwiseCallbackSetting";
        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<Response.OperatorwiseCallbackSetting.OperatorWiseCallbackSettingResponse>>();
            cache.Remove(Key);
        }

        internal Response.OperatorwiseCallbackSetting.OperatorWiseCallbackSettingResponse SelectByOperatorID(int operatorID)
        {
            List<Response.OperatorwiseCallbackSetting.OperatorWiseCallbackSettingResponse> list = Select();

            Response.OperatorwiseCallbackSetting.OperatorWiseCallbackSettingResponse obj = (from item in list
                             where item.OperatorID == operatorID
                             select item).FirstOrDefault();

            return obj;
        }

        internal List<Response.OperatorwiseCallbackSetting.OperatorWiseCallbackSettingResponse> Select()
        {
            var _list = new MemoryCacheProcessing<List<Response.OperatorwiseCallbackSetting.OperatorWiseCallbackSettingResponse>>();

            List<Response.OperatorwiseCallbackSetting.OperatorWiseCallbackSettingResponse> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<Response.OperatorwiseCallbackSetting.OperatorWiseCallbackSettingResponse> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from Operator in context.OperatorwiseCallbackSetting
                        orderby Operator.OperatorwiseCallbackSettingID descending
                        where Operator.IsActive == true
                        select new Response.OperatorwiseCallbackSetting.OperatorWiseCallbackSettingResponse
                        {
                            OperatorID = Operator.OperatorID,
                            Url = Operator.Url,                      
                            IsActive = Operator.IsActive,
                            IsDelete = Operator.IsDelete,
                            MethodType= Operator.MethodType,
                            HeaderJson = Operator.HeaderJson,
                            DateFormat = Operator.DateFormat

                        }).ToList();
            }
        }

    }
}
