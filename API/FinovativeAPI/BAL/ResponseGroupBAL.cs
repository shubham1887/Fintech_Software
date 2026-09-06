using Project.DAL;
using Project.Model;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ResponseGroupBal
    {
        internal string GetKeyValue(string inputkey)
        {
            return new ResponseGroupDal().GetValue(inputkey);
        }

        internal List<ResponseKeyValue> Select()
        {
            return new ResponseGroupDal().Select();
        }
    }
}