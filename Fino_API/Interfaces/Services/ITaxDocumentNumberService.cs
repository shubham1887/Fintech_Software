namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Allocates GST document numbers.
    ///
    /// The number must be unique, consecutive and reset per financial year, and at most 16
    /// characters using only letters, digits, "/" and "-". Two duplicates on one registration is
    /// the most expensive mistake this system can make, so allocation happens in ONE atomic
    /// database statement and never from a counter held in application memory — two web workers,
    /// or one worker and a month-end batch, would otherwise hand out the same number.
    /// </summary>
    public interface ITaxDocumentNumberService
    {
        /// <summary>
        /// Take the next number in a series and return it formatted.
        ///
        /// The counter is advanced whether or not the caller goes on to save a document, which is
        /// deliberate: a gap in the sequence is explainable (GSTR-1 has a "cancelled" count for
        /// exactly this), while two documents sharing a number is not. Reserving and then
        /// releasing would reintroduce the race it exists to prevent.
        /// </summary>
        /// <returns>(number, failureMessage). The number is empty when the message is set.</returns>
        Task<(string DocumentNo, string FailureMessage)> Allocate(string seriesCode, string companyGstin, DateTime documentDate);

        /// <summary>The financial-year label a date falls in, e.g. 2026-09-30 -> "2627".</summary>
        string FinancialYearOf(DateTime date);
    }
}
