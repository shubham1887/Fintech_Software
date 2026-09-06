using Project.BAL;
using Project.Model;
using Project.StaticModel;
using System.Collections.Generic;
using System.Text;

namespace Project.Processing
{
    public class MatcherProcessing
    {
        private string ReplaceKey(string idol, string inputkey)
        {
            string key = new ResponseGroupBal().GetKeyValue(inputkey);
            if (key.Length <= 0)
            {
                return "";
            }
            return idol.Replace(inputkey, key);
        }

        public bool isExtrated(string extratedText)
        {
            if (extratedText == null)
            {
                return false;
            }

            if (extratedText.Trim().EqualsIgnoreCase(Messages.MatcherParamNotFound))
            {
                return false;
            }
            return true;
        }

        public bool isTransactionIDValid(string extratedText)
        {
            if (extratedText == null)
            {
                return false;
            }
            if (extratedText.Trim().Length <= 0)
            {
                return false;
            }
            if (extratedText.Trim().EqualsIgnoreCase(Messages.MatcherParamNotFound))
            {
                return false;
            }
            if (extratedText.Trim().EqualsIgnoreCase("TRANSACTION_PENDING"))
            {
                return false;
            }
            if (extratedText.Trim().EqualsIgnoreCase("TRANSACTION PENDING"))
            {
                return false;
            }
            return true;
        }

        public string getDifference(string idol, string original, string inputkey)
        {
            string retStr = Messages.MatcherParamNotFound;

            string key = new ResponseGroupBal().GetKeyValue(inputkey);

            if (key.Length <= 0)
            {
                return retStr;
            }
            idol = idol.Replace(inputkey, key);

            List<ResponseKeyValue> listResponseKeyValue = new ResponseGroupBal().Select();
            foreach (ResponseKeyValue item in listResponseKeyValue)
            {
                string resp = ReplaceKey(idol, item.Response.ToString());

                if (resp.Trim().Length <= 0)
                {
                    continue;
                }

                idol = resp;
            }

            var dmp = new diff_match_patch();
            List<Diff> diffs = dmp.diff_main(idol.ToLower(), original.ToLower());

            string deleteKey = "", insertKey = "";
            StringBuilder sb = new StringBuilder();
            foreach (Diff d in diffs)
            {
                if (d.operation == Operation.DELETE)
                {
                    deleteKey = d.text.Trim();
                }

                if (d.operation == Operation.INSERT)
                {
                    insertKey = d.text.Trim();
                    if (key.EqualsIgnoreCase(deleteKey))
                    {
                        //sb.Append(string.Format("{0}:{1}{2}", deleteKey, insertKey, Environment.NewLine));
                        return insertKey;
                    }
                }
            }

            return retStr;
        }
    }
}