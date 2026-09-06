namespace MplanCore.Response
{
    public class EzytmPlanResponse
    {
        public string ERROR { get; set; }
        public string STATUS { get; set; }
        public string Operator { get; set; }
        public string Circle { get; set; }
        public RDATAPLAN RDATA { get; set; }
        public string MESSAGE { get; set; }
    }

    public class FULLTT
    {
        public int rs { get; set; }
        public string validity { get; set; }
        public string desc { get; set; }
        public string Type { get; set; }
    }

    public class TOPUP
    {
        public int rs { get; set; }
        public string validity { get; set; }
        public string desc { get; set; }
        public string Type { get; set; }
    }

    public class Datum
    {
        public int rs { get; set; }
        public string validity { get; set; }
        public string desc { get; set; }
        public string Type { get; set; }
    }

    public class SM
    {
        public int rs { get; set; }
        public string validity { get; set; }
        public string desc { get; set; }
        public string Type { get; set; }
    }

    public class FRC
    {
        public int rs { get; set; }
        public string validity { get; set; }
        public string desc { get; set; }
        public string Type { get; set; }
    }

    public class STV
    {
        public int rs { get; set; }
        public string validity { get; set; }
        public string desc { get; set; }
        public string Type { get; set; }
    }

    public class Romaing
    {
        public int rs { get; set; }
        public string validity { get; set; }
        public string desc { get; set; }
        public string Type { get; set; }
    }

    public class RATE_CUTTER
    {
        public int rs { get; set; }
        public string validity { get; set; }
        public string desc { get; set; }
        public string Type { get; set; }
    }

    public class TwoG
    {
        public int rs { get; set; }
        public string validity { get; set; }
        public string desc { get; set; }
        public string Type { get; set; }
    }

    public class JioPhone
    {
        public int rs { get; set; }
        public string validity { get; set; }
        public string desc { get; set; }
        public string Type { get; set; }
    }

    public class RDATAPLAN
    {
        public List<FULLTT> FULLTT { get; set; }
        public List<TOPUP> TOPUP { get; set; }
        public List<Datum> DATA { get; set; }
        public List<SM> SMS { get; set; }
        public List<Romaing> Romaing { get; set; }
        public List<FRC> FRC { get; set; }
        public List<STV> STV { get; set; }
        public List<RATE_CUTTER> RATE_CUTTER { get; set; }
        public List<JioPhone> JioPhone { get; set; }
        public List<TwoG> TwoG { get; set; }
    }
}