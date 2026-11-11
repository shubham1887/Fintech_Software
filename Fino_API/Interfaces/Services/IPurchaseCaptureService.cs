using FintechApp.Entities;
using FintechApp.Requests.Phase3;
using FintechApp.Responses.Phase3;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Records what we bought. A purchase is a TaxInvoiceHeader with Direction = PURCHASE carrying
    /// the SUPPLIER's own number, and the tax exactly as they billed it — never re-resolved,
    /// because the credit we may claim is the tax that is on their document.
    /// </summary>
    public interface IPurchaseCaptureService
    {
        /// <summary>Capture a supplier invoice from entered figures. Idempotent on (supplier, number).</summary>
        Task<PurchaseCaptureResult> CapturePurchase(PurchaseCaptureRequest request);

        /// <summary>Capture a supplier invoice from its e-invoice JSON (the NIC/IRP schema).</summary>
        Task<(PurchaseCaptureResult? Result, string Failure)> ImportEInvoice(EInvoiceImportRequest request);

        /// <summary>Purchase documents in a period, with their ITC status — the purchase register.</summary>
        Task<List<TaxInvoiceHeader>> GetPurchases(DateTime periodFrom, DateTime periodTo);
    }
}
