namespace YESBankCore.Controller
{
    internal class Util
    {
        public static Dictionary<string, string> getFields(string fields, string data)
        {
            string[] fieldList = fields.Split('|')
                               .Select(s => s.Trim())
                               .ToArray();

            string[] dataList = data.Split('|')
                            .Select(s => s.Trim())
                            .ToArray();

            Dictionary<string, string> record = fieldList.Zip(dataList, (field, value) => new { field, value })
                                                 .ToDictionary(pair => pair.field, pair => pair.value);

            return record;
        }

        public static string getValue(Dictionary<string, string> record, string key)
        {
            if (record.ContainsKey(key))
            {
                return record[key];
            }
            return "";
        }
    }
}