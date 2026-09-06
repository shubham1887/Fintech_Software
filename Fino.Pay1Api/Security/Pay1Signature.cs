using System.Security.Cryptography;
using System.Text;

namespace Fino.Pay1Api.Security
{
    // Pay1 signs every exchange the same way, in both directions:
    //     hash_code = sha1(partner_id + trans_id + api_key)
    // On an outbound call trans_id is the request id we send; on the inbound status push it is the
    // client_req_id Pay1 echoes back. The API key itself never travels over the wire.
    public static class Pay1Signature
    {
        public static string Compute(string partnerId, string transId, string apiKey)
        {
            byte[] digest = SHA1.HashData(Encoding.UTF8.GetBytes($"{partnerId}{transId}{apiKey}"));
            return Convert.ToHexString(digest).ToLowerInvariant();
        }

        /// <summary>Constant-time compare, so verifying an inbound push cannot be timed.</summary>
        public static bool Verify(string partnerId, string transId, string apiKey, string providedHash)
        {
            if (string.IsNullOrWhiteSpace(providedHash)) return false;
            var expected = Encoding.UTF8.GetBytes(Compute(partnerId, transId, apiKey));
            var actual = Encoding.UTF8.GetBytes(providedHash.Trim().ToLowerInvariant());
            return expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(expected, actual);
        }
    }
}
