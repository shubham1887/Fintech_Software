using Fino.Pay1Api.Configuration;
using Fino.Pay1Api.Requests;
using Fino.Pay1Api.Responses;

namespace Fino.Pay1Api.Interfaces
{
    // Service layer: typed Pay1 Utility (BBPS) operations over recharges.pay1.in. Builds the signed
    // receiveApi URL, delegates transport to IPay1ApiClient and parses the reply into a typed
    // response. Credentials come from the optional per-call override (the provider row serving the
    // biller) when supplied, otherwise from the static "Pay1Api" configuration section.
    //
    // Pay1 operation_type map:  2 balance · 3 status · 4 complaint · 5 complaint status
    //                           7 biller list · 8 bill fetch · 9 bill payment
    public interface IPay1ApiService
    {
        /// <summary>Biller catalogue — every biller and the inputs it asks for (operation_type 7).</summary>
        Task<Pay1BillerResponse> BillerInfo(Pay1Credentials? creds = null);

        /// <summary>Live bill enquiry before payment (operation_type 8).</summary>
        Task<Pay1ApiResponse> BillFetch(BillFetchRequest request, Pay1Credentials? creds = null);

        /// <summary>Bill payment (operation_type 9).</summary>
        Task<Pay1ApiResponse> BillPayment(BillPaymentRequest request, Pay1Credentials? creds = null);

        /// <summary>Partner wallet balance at Pay1 (operation_type 2).</summary>
        Task<Pay1ApiResponse> Balance(Pay1Credentials? creds = null);

        /// <summary>Transaction enquiry, up to 10 request ids per call (operation_type 3).</summary>
        Task<Pay1ApiResponse> StatusCheck(StatusCheckRequest request, Pay1Credentials? creds = null);

        /// <summary>Raise a complaint against a transaction (operation_type 4).</summary>
        Task<Pay1ApiResponse> RaiseComplaint(ComplaintRequest request, Pay1Credentials? creds = null);

        /// <summary>Complaint progress (operation_type 5).</summary>
        Task<Pay1ApiResponse> ComplaintStatus(string complaintID, Pay1Credentials? creds = null);

        /// <summary>Verifies and maps Pay1's auto status-update push. `values` is everything the
        /// host collected from the request — query string, form fields or parsed JSON body.</summary>
        Pay1CallbackResult ParseCallback(IDictionary<string, string> values, string raw, Pay1Credentials? creds = null);
    }
}
