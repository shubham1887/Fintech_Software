namespace FintechApp.Requests.ChargeGst
{
    /// <summary>
    /// Work one hypothetical transaction through the rate cards as they stand and show both sides
    /// of it: what the provider would charge and what the downline would earn, each split into
    /// base and GST, with the layer that decided named in words.
    ///
    /// The same reason the tax simulator and the commission-tax "try a figure" exist: a band's GST
    /// type changes what every later transaction in that band is worth, and an admin has to be able
    /// to see the answer before a month of them is booked on it.
    /// </summary>
    public class ChargeGstPreviewRequest
    {
        /// <summary>The transacting user, whose slab mapping decides the downline side. 0 previews
        /// the provider side only.</summary>
        public int UserID { get; set; }

        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }

        /// <summary>The transaction amount, which is what picks the band on each side.</summary>
        public decimal Amount { get; set; }

        /// <summary>Resolve as of this date — a rate change is effective-dated. Default is today.</summary>
        public DateTime? AsOf { get; set; }
    }
}
