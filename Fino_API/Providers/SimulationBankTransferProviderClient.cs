using FintechApp.Interfaces.Providers;

namespace FintechApp.Providers
{
    // Sandbox / simulation bank-transfer provider. Used when no real DMT aggregator is wired but the
    // full money-out flow still needs to complete end-to-end (dev, demo, UAT). It generates a UTR-like
    // reference and reports SUCCESS so the pipeline settles the transaction (wallet stays debited).
    //
    // Deterministic test hooks (drive the SUCCESS / PENDING / FAILED branches without external state):
    //   • whole-rupee amount ending in 9  → FAILED  (pipeline auto-refunds the debit)
    //   • whole-rupee amount ending in 5  → PENDING (settles later via status-check)
    //   • everything else                 → SUCCESS
    //
    // SAFETY: this is opt-in via config "DMTProvider:Mode" = "Simulation". The default registration
    // (Program.cs) is the Unconfigured client, so a production build never auto-succeeds transfers
    // unless the operator explicitly turns simulation on.
    public class SimulationBankTransferProviderClient : IBankTransferProviderClient
    {
        public Task<BankTransferProviderResult> SendTransfer(BankTransferContext context)
        {
            int lastDigit = (int)(Math.Floor(context.Amount) % 10);

            var result = new BankTransferProviderResult
            {
                IsConfigured = true,
                ProviderRequestID = context.ProviderRequestID,
                ProviderRefNo = GenerateUtr(context),
                RawRequest = $"[SIMULATION] {context.Kind} {context.Amount:0.00} → {context.AccountNo} ({context.IFSC}) {context.PaymentType}",
            };

            switch (lastDigit)
            {
                case 9:
                    result.Status = "FAILED";
                    result.Message = "Transfer declined by bank (simulated).";
                    result.ProviderRefNo = string.Empty;
                    break;
                case 5:
                    result.Status = "PENDING";
                    result.Message = "Transfer submitted to bank, awaiting confirmation (simulated).";
                    break;
                default:
                    result.Status = "SUCCESS";
                    result.Message = "Transfer completed successfully (simulated).";
                    break;
            }

            result.RawResponse = $"[SIMULATION] {result.Status}: {result.Message} ref={result.ProviderRefNo}";
            return Task.FromResult(result);
        }

        // 12-char numeric UTR-style reference, unique per dispatch.
        private static string GenerateUtr(BankTransferContext context)
        {
            string seed = Guid.NewGuid().ToString("N").ToUpperInvariant();
            return $"SIM{context.TransactionMasterID}{seed}".Substring(0, 12);
        }
    }
}
