using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ResponseGroupDal
    {
        private string key = "_ResponseKeyValue";

        internal List<ResponseKeyValue> Select()
        {
            var _list = new MemoryCacheProcessing<List<ResponseKeyValue>>();

            List<ResponseKeyValue> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<ResponseKeyValue> SelectWithoutCache()
        {
            List<ResponseKeyValue> list;
            using (var context = new DatabaseContext())
            {
                list = context.ResponseKeyValue.ToList();
            }
            return list;
        }

        internal string GetValue(string inputkey)
        {
            List<ResponseKeyValue> list = Select();

            ResponseKeyValue obj = (from item in list
                                    where item.Response == inputkey.Trim()
                                    select item).FirstOrDefault();
            if (obj == null)
            {
                return "";
            }

            return obj.Value.Trim();
        }
    }
}