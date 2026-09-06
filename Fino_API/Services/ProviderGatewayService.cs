using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderGateway;
using FintechApp.Responses.ProviderGateway;
using FintechApp.StaticModels;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    // The config-driven provider gateway engine.
    //
    // A provider integration is described entirely by data:
    //   ProviderConfiguration  → which provider serves an operator (+ provider-side operator code,
    //                            SIM/lapu, channel type, health thresholds)
    //   ProviderDetails        → the endpoint per provider+service+kind: URL/body template with
    //                            [Token] placeholders, HTTP method, header JSON, date format,
    //                            transaction-id prefix
    //   ProviderResponseMaster → reply-reading rules: a rule matches when every fragment of its
    //                            match text is present in the reply; its template then extracts
    //                            values ([RefID]/[TxnID]/[Balance]/[Message]…) and its Status maps
    //                            the reply to SUCCESS / FAILED / PENDING.
    //
    // Unknown replies are deliberately mapped to PENDING (never FAILED) so money is only refunded
    // when a rule explicitly says the transaction failed.
    public class ProviderGatewayService : IProviderGatewayService
    {
        private const int HttpTimeoutSeconds = 60;

        // Canonical endpoint / rule kinds. Legacy spellings are normalised into these.
        public const string KindTransaction = "TRANSACTION";
        public const string KindBalance = "BALANCE";
        public const string KindStatus = "STATUS";
        public const string KindBillFetch = "BILLFETCH";

        private readonly IProviderMasterRepository _providerRepo;
        private readonly IProviderDetailsRepository _endpointRepo;
        private readonly IProviderConfigurationRepository _routeRepo;
        private readonly IProviderResponseMasterRepository _ruleRepo;
        private readonly IProviderRequestResponseLogRepository _logRepo;
        private readonly IOperatorSequenceRepository _sequenceRepo;
        private readonly IOperatorSequenceUserWiseRepository _sequenceUserWiseRepo;
        private readonly IOperatorRepository _operatorRepo;
        private readonly IProviderOperatorDenominationMappingRepository _denominationMappingRepo;
        private readonly IDenominationMasterRepository _denominationRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Fino.JaiJuiApi.Interfaces.IJaiJuiApiService _jaijui;
        private readonly Fino.Pay1Api.Interfaces.IPay1ApiService _pay1;
        private readonly Logger _logger;

        public ProviderGatewayService(
            IProviderMasterRepository providerRepo,
            IProviderDetailsRepository endpointRepo,
            IProviderConfigurationRepository routeRepo,
            IProviderResponseMasterRepository ruleRepo,
            IProviderRequestResponseLogRepository logRepo,
            IOperatorSequenceRepository sequenceRepo,
            IOperatorSequenceUserWiseRepository sequenceUserWiseRepo,
            IOperatorRepository operatorRepo,
            IProviderOperatorDenominationMappingRepository denominationMappingRepo,
            IDenominationMasterRepository denominationRepo,
            IHttpClientFactory httpClientFactory,
            Fino.JaiJuiApi.Interfaces.IJaiJuiApiService jaijui,
            Fino.Pay1Api.Interfaces.IPay1ApiService pay1)
        {
            _jaijui = jaijui;
            _pay1 = pay1;
            _providerRepo = providerRepo;
            _endpointRepo = endpointRepo;
            _routeRepo = routeRepo;
            _ruleRepo = ruleRepo;
            _logRepo = logRepo;
            _sequenceRepo = sequenceRepo;
            _sequenceUserWiseRepo = sequenceUserWiseRepo;
            _operatorRepo = operatorRepo;
            _denominationMappingRepo = denominationMappingRepo;
            _denominationRepo = denominationRepo;
            _httpClientFactory = httpClientFactory;
            _logger = new Logger();
        }

        // ── Recharge dispatch ────────────────────────────────────────────────────────

        public async Task<RechargeProviderResult> DispatchRecharge(RechargeDispatchContext context)
        {
            var notConfigured = new RechargeProviderResult
            {
                Status = StatusType.Failed,
                IsConfigured = false,
                ProviderRequestID = context.ProviderRequestID,
                Message = Messages.Common.Message.ServiceUnavailable
            };

            try
            {
                // 1. Resolve candidate routes: active operator→provider mappings for this service,
                //    ordered by the configured switching sequence (user-wise first, then the
                //    operator's default provider sequence), so admin priorities drive dispatch.
                var routes = (await _routeRepo.GetByOperatorID(context.OperatorID) ?? new List<ProviderConfiguration>())
                    .Where(r => r.IsActive && (r.ServiceID == context.ServiceID || r.ServiceID == 0))
                    .ToList();
                if (routes.Count == 0) return notConfigured;

                List<ProviderConfiguration> orderedRoutes;
                if (context.ForceProviderID > 0)
                {
                    // Admin shift: the named provider is the authorisation, so the sequence (which
                    // only ranks defaults) does not gate it. It must still have an active route —
                    // and only this provider is tried, never a fallback down the sequence.
                    orderedRoutes = routes
                        .Where(r => r.ProviderID == context.ForceProviderID)
                        .OrderByDescending(r => r.ServiceID == context.ServiceID)
                        .ThenBy(r => r.ConfigurationID)
                        .ToList();
                    if (orderedRoutes.Count == 0)
                    {
                        notConfigured.Message = "The selected provider is not configured for this operator.";
                        return notConfigured;
                    }
                }
                else
                {
                    // No sequence (neither user-wise nor the operator default) = the admin has not
                    // authorised any provider for this operator — the recharge must NOT hit anyone.
                    var sequenceOrder = await ResolveSequenceOrder(context.OperatorID, context.UserID);
                    if (sequenceOrder.Count == 0)
                    {
                        notConfigured.Message = Messages.Provider.Message.SequenceNotSet;
                        return notConfigured;
                    }

                    // Only providers that appear in the sequence may serve the call.
                    orderedRoutes = routes
                        .Where(r => sequenceOrder.Contains(r.ProviderID))
                        .OrderBy(r => sequenceOrder.IndexOf(r.ProviderID))
                        .ThenByDescending(r => r.ServiceID == context.ServiceID)
                        .ThenBy(r => r.ConfigurationID)
                        .ToList();
                    if (orderedRoutes.Count == 0)
                    {
                        notConfigured.Message = Messages.Provider.Message.SequenceNotSet;
                        return notConfigured;
                    }
                }

                // Denomination gating. Runs whenever the operator actually has denomination
                // mappings (ApplyDenominationFilter no-ops and returns the routes unchanged when it
                // has none), so a range marked STOP for a provider ALWAYS blocks — an existing
                // mapping is intent enough. The per-operator IsDenominationApplicable flag is kept
                // only as a UI hint/badge and is no longer a prerequisite (previously, forgetting to
                // switch it on silently skipped the whole check and let stopped ranges through).
                // Both lookups hit cached in-memory lists, so this stays cheap on the hot path.
                var filteredRoutes = await ApplyDenominationFilter(orderedRoutes, context.OperatorID, context.Amount);
                if (filteredRoutes.Count == 0)
                {
                    notConfigured.Message = Messages.Provider.Message.DenominationBlocked;
                    return notConfigured;
                }
                orderedRoutes = filteredRoutes;

                RechargeProviderResult? lastFailure = null;
                bool anyDispatched = false;

                foreach (var route in orderedRoutes)
                {
                    var provider = await _providerRepo.GetByID(route.ProviderID);
                    if (provider == null || !provider.IsActive || provider.IsDelete) continue;

                    // Pay1 Utility (BBPS) has no endpoint template - see DispatchPay1.
                    if (Providers.Pay1ProviderResolver.IsPay1(provider.ProviderName))
                    {
                        var pay1Result = await DispatchPay1(provider, route, context);
                        if (!pay1Result.IsConfigured)
                        {
                            // Nothing left our server (credentials or biller code missing). No money
                            // is at risk, so fall through to the next provider in the sequence; if
                            // there is none, the pipeline refunds with this reason.
                            notConfigured.Message = pay1Result.Message;
                            lastFailure = pay1Result;
                            continue;
                        }
                        anyDispatched = true;
                        return pay1Result;
                    }

                    // 2. Resolve the endpoint template for this provider + service.
                    var endpoint = await ResolveEndpoint(provider.ProviderID, KindTransaction, context.ServiceID);
                    if (endpoint == null) continue;

                    string requestID = string.Concat(endpoint.TransactionIDPrefix ?? string.Empty, context.ProviderRequestID);
                    anyDispatched = true;

                    // 3. Render the template and fire the HTTP call.
                    var values = BuildRequestValues(provider, route, endpoint, context, requestID);
                    var (rawResponse, transportState) = await ExecuteEndpoint(endpoint, values, provider.ProviderID,
                        context.UserID, context.TransactionMasterID, context.ServiceID, context.IPAddress);

                    if (transportState == TransportState.Unreachable)
                    {
                        // Provider not reachable at all — safe to try the next one in sequence.
                        lastFailure = new RechargeProviderResult
                        {
                            Status = StatusType.Failed,
                            ProviderRequestID = requestID,
                            ProviderID = provider.ProviderID,
                            RawResponse = Truncate(rawResponse, 4000),
                            Message = Messages.Provider.Message.ProviderUnreachable
                        };
                        continue;
                    }
                    if (transportState == TransportState.TimedOut)
                    {
                        // The call may have gone through — never re-dispatch, hold the money.
                        return new RechargeProviderResult
                        {
                            Status = StatusType.Pending,
                            ProviderRequestID = requestID,
                            ProviderID = provider.ProviderID,
                            RawResponse = Truncate(rawResponse, 4000),
                            Message = Messages.Provider.Message.ProviderTimeout
                        };
                    }

                    // 4. Read the reply through the provider's TRANSACTION rules.
                    var rules = await GetActiveRules(provider.ProviderID, KindTransaction);
                    var outcome = EvaluateRules(rules, rawResponse);

                    if (!outcome.Matched)
                    {
                        // Unknown reply — hold the transaction rather than guessing an outcome.
                        return new RechargeProviderResult
                        {
                            Status = StatusType.Pending,
                            ProviderRequestID = requestID,
                            ProviderID = provider.ProviderID,
                            RawResponse = Truncate(rawResponse, 4000),
                            Message = Messages.Provider.Message.AwaitingConfirmation
                        };
                    }

                    var result = BuildDispatchResult(outcome, requestID, provider.ProviderID, rawResponse);

                    // Provider commission = what the admin earns from the provider for this route.
                    // The route configuration (Provider Setting → Commission) is AUTHORITATIVE — it is
                    // the markup the admin set (e.g. ₹10 Fix), and must not be silently overridden by
                    // whatever the provider/simulator happens to echo back in its reply. Only fall back
                    // to the reply's commission when the route has none configured (0).
                    decimal routeCommission = ComputeRouteCommission(route, context.Amount);
                    result.ProviderCommission = routeCommission != 0m
                        ? routeCommission
                        : (result.ProviderCommission ?? 0m);

                    // Explicit FAILURE → fall through to the next provider in the sequence.
                    if (result.Status == StatusType.Failed) { lastFailure = result; continue; }

                    return result; // SUCCESS or PENDING — settled by the reply.
                }

                if (!anyDispatched) return notConfigured;
                return lastFailure ?? notConfigured;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                // Something inside the gateway itself broke after the call may have gone out —
                // keep the transaction pending for manual/status-check settlement.
                return new RechargeProviderResult
                {
                    Status = StatusType.Pending,
                    ProviderRequestID = context.ProviderRequestID,
                    Message = Messages.Provider.Message.AwaitingConfirmation
                };
            }
        }

        // Provider priority for an operator: the user's own switching sequence wins,
        // otherwise the operator's default sequence (lower Count = higher priority).
        private async Task<List<int>> ResolveSequenceOrder(int operatorID, int userID)
        {
            try
            {
                var userRows = await _sequenceUserWiseRepo.GetByUserAndOperator(userID, operatorID);
                if (userRows.Count > 0)
                    return userRows.Select(r => r.ProviderID).Distinct().ToList();

                var defaultRows = await _sequenceRepo.GetByOperatorID(operatorID);
                return defaultRows.OrderBy(r => r.Count).Select(r => r.ProviderID).Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return new List<int>();
            }
        }

        // Legacy denomination switching, applied ONLY when the operator is flagged denomination-
        // applicable. A provider whose STOP denomination covers the amount is removed; a provider
        // whose allowed denomination covers the amount is promoted to the front (keeping the
        // sequence order otherwise). Providers with no mapping for this operator are unaffected.
        private async Task<List<ProviderConfiguration>> ApplyDenominationFilter(
            List<ProviderConfiguration> orderedRoutes, int operatorID, decimal amount)
        {
            var mappings = await _denominationMappingRepo.GetByOperatorID(operatorID);
            if (mappings.Count == 0) return orderedRoutes;

            var denominations = await _denominationRepo.GetAll();
            var denominationByID = denominations.ToDictionary(d => d.DenominationMasterID);

            var allowed = new HashSet<int>();
            var blocked = new HashSet<int>();
            foreach (var mapping in mappings)
            {
                if (!denominationByID.TryGetValue(mapping.DenominationMasterID, out var denomination)) continue;
                if (!AmountInDenominationRange(denomination.DenominationRange, amount)) continue;
                if (denomination.IsStop) blocked.Add(mapping.ProviderID);
                else allowed.Add(mapping.ProviderID);
            }

            var result = orderedRoutes.Where(r => !blocked.Contains(r.ProviderID)).ToList();
            return result
                .OrderByDescending(r => allowed.Contains(r.ProviderID))
                .ThenBy(r => result.IndexOf(r))
                .ToList();
        }

        // DenominationRange grammar (legacy): comma-separated entries, each either a single
        // amount ("10") or an inclusive range ("10-5000").
        private static bool AmountInDenominationRange(string range, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(range)) return false;
            foreach (var part in range.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var bounds = part.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (bounds.Length == 1)
                {
                    if (decimal.TryParse(bounds[0], out var single) && single == amount) return true;
                }
                else if (bounds.Length >= 2)
                {
                    if (decimal.TryParse(bounds[0], out var from) && decimal.TryParse(bounds[1], out var to) &&
                        amount >= from && amount <= to) return true;
                }
            }
            return false;
        }

        // Commission earned from the provider per the route configuration (Provider Setting →
        // Commission tab): FLEXI = percent of amount, FIX = flat; SURCHARGE category = we pay them.
        private static decimal ComputeRouteCommission(ProviderConfiguration route, decimal amount)
            => SlabCommissionResolver.Normalize(route.AdminComm, route.AdminCommType, amount, route.CommissionCategory);

        // Builds the dispatch result from a matched rule, backfilling well-known fields from the
        // raw JSON reply when the rule template did not capture them.
        private static RechargeProviderResult BuildDispatchResult(RuleOutcome outcome, string requestID, int providerID, string rawResponse)
        {
            var values = new Dictionary<string, string>(outcome.ExtractedValues, StringComparer.OrdinalIgnoreCase);
            MergeJsonFallback(rawResponse, values);

            var result = new RechargeProviderResult
            {
                Status = outcome.MappedStatus,
                ProviderRequestID = requestID,
                ProviderID = providerID,
                RawResponse = Truncate(rawResponse, 4000),
                ProviderRefNo = FirstValue(values, "RefID", "TxnID", "TransactionID", "OrderID", "ReferenceNo"),
                Message = FirstValue(values, "Message", "Description") is { Length: > 0 } msg
                    ? msg
                    : DefaultMessageFor(outcome.MappedStatus)
            };

            if (TryReadDecimal(values, out var opBal, "OpeningBalance", "OpBal"))
                result.ProviderOpeningBalance = opBal;
            if (TryReadDecimal(values, out var clBal, "ClosingBalance", "ClBal", "Balance"))
                result.ProviderClosingBalance = clBal;
            if (TryReadDecimal(values, out var pComm, "Commission", "Comm", "CommissionAmount"))
                result.ProviderCommission = pComm;

            return result;
        }

        // Providers reply JSON in slightly different shapes (e.g. Payin adds refrenceID/referenceID
        // and unquoted numbers). When the rule template misses a well-known field, read it straight
        // from the JSON so the transaction still records the real reply.
        private static void MergeJsonFallback(string rawResponse, Dictionary<string, string> values)
        {
            if (string.IsNullOrWhiteSpace(rawResponse)) return;
            Dictionary<string, object?>? parsed;
            try { parsed = JsonConvert.DeserializeObject<Dictionary<string, object?>>(rawResponse); }
            catch { return; /* not a flat JSON object — nothing to backfill */ }
            if (parsed == null) return;

            foreach (var (key, rawValue) in parsed)
            {
                if (rawValue == null) continue;
                string value = Convert.ToString(rawValue, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
                if (value.Length == 0) continue;

                string? canonical = key.Trim().ToLowerInvariant() switch
                {
                    "message" or "description" or "remark" => "Message",
                    "commission" or "comm" or "commissionamount" or "commamount" => "Commission",
                    "transactionid" or "referenceno" or "referenceid" or "refrenceid" or "providerrefno" or "opid" or "operatorref" => "RefID",
                    "openingbalance" => "OpeningBalance",
                    "closingbalance" => "ClosingBalance",
                    "rechargebalance" => "RechargeBalance",
                    "dmtbalance" => "DMTBalance",
                    "aepsbalance" => "AEPSBalance",
                    "outstanding" => "Outstanding",
                    // Bill-fetch replies (names differ per BBPS/utility aggregator)
                    "customername" or "consumername" or "custname" or "billername" => "CustomerName",
                    "billamount" or "dueamount" or "netamount" or "billamt" => "BillAmount",
                    "duedate" or "billduedate" or "paybydate" => "DueDate",
                    "billnumber" or "billno" or "billid" => "BillNumber",
                    "billdate" => "BillDate",
                    _ => null
                };
                if (canonical == null) continue;
                if (!values.TryGetValue(canonical, out var existing) || string.IsNullOrWhiteSpace(existing))
                    values[canonical] = value;
            }
        }

        // ── Balance enquiry ──────────────────────────────────────────────────────────

        public async Task<(string failureMessage, ProviderBalanceResponse? data)> CheckBalance(int providerID)
        {
            var provider = await _providerRepo.GetByID(providerID);
            if (provider == null || provider.IsDelete) return (Messages.Provider.Message.NotFound, null);

            // JaiJui is a coded library integration (not a template-driven gateway), so it has no
            // BALANCE endpoint/rules. Fetch its wallet balance directly through Fino.JaiJuiApi using the
            // credentials stored on this "Add API" provider row.
            if (Providers.JaiJuiProviderResolver.IsJaiJui(provider.ProviderName))
                return await CheckJaiJuiBalance(provider);

            // Pay1 Utility is a library integration too - no BALANCE endpoint/rules to run.
            if (Providers.Pay1ProviderResolver.IsPay1(provider.ProviderName))
                return await CheckPay1Balance(provider);

            var endpoint = await ResolveEndpoint(providerID, KindBalance, serviceID: 0);
            if (endpoint == null) return (Messages.Provider.Message.BalanceEndpointMissing, null);

            var values = BuildRequestValues(provider, route: null, endpoint, context: null, requestID: string.Empty);
            var (rawResponse, transportState) = await ExecuteEndpoint(endpoint, values, providerID,
                userID: 0, transactionID: 0, endpoint.ServiceID, ipAddress: string.Empty);

            if (transportState != TransportState.Completed)
                return (Messages.Provider.Message.ProviderUnreachable, null);

            var rules = await GetActiveRules(providerID, KindBalance);
            var outcome = EvaluateRules(rules, rawResponse);

            // Rule extraction first, then backfill from the raw JSON — providers reply in
            // slightly different shapes (e.g. Payin's balance reply is camelCase with no Status).
            var captured = new Dictionary<string, string>(outcome.ExtractedValues, StringComparer.OrdinalIgnoreCase);
            MergeJsonFallback(rawResponse, captured);

            bool balancePresent = captured.ContainsKey("Balance") || captured.ContainsKey("RechargeBalance") || captured.ContainsKey("DMTBalance");
            string providerMessage = FirstValue(captured, "Message");

            // Some providers (Payin) report errors IN-BAND: the reply still carries zeroed balance
            // fields plus an error message ("Request from invalid IP…"). A figures-only success is
            // assumed ONLY when the reply carries no message; anything else needs a matching rule.
            bool cleanFigures = balancePresent && providerMessage.Length == 0;

            var result = new ProviderBalanceResponse
            {
                ProviderID = providerID,
                ProviderName = provider.ProviderName,
                Status = outcome.Matched ? outcome.MappedStatus : (cleanFigures ? StatusType.Success : StatusType.Pending),
                Message = providerMessage.Length > 0
                    ? providerMessage
                    : (outcome.Matched || cleanFigures ? Messages.Provider.Message.BalanceFetched : Messages.Provider.Message.NoRuleMatched),
                ExtractedValues = captured,
                RawResponse = Truncate(rawResponse, 4000)
            };

            // Persist balances only for a SUCCESS reply — an error reply with zeroed figures
            // must not wipe the last known good balance.
            if (result.Status == StatusType.Success)
            {
                if (TryReadDecimal(captured, out var rechargeBal, "Balance", "ClBal", "MainBalance", "RechargeBalance"))
                {
                    result.RechargeBalance = rechargeBal;
                    provider.RechargeBalance = rechargeBal;
                }
                if (TryReadDecimal(captured, out var dmtBal, "DMTBalance", "PayoutBalance"))
                {
                    result.DMTBalance = dmtBal;
                    provider.DMTBalance = dmtBal;
                }
                if (result.RechargeBalance.HasValue || result.DMTBalance.HasValue)
                    await _providerRepo.Update(provider);
            }

            return (StatusType.Ok, result);
        }

        // Balance enquiry for a JaiJui gateway — bypasses the endpoint/rule engine and calls the
        // Fino.JaiJuiApi wallet-balance endpoint with the credentials on the provider row. JaiJui is a
        // DMT/payout provider, so the wallet figure is stored as the DMT balance.
        private async Task<(string failureMessage, ProviderBalanceResponse? data)> CheckJaiJuiBalance(ProviderMaster provider)
        {
            var creds = Providers.JaiJuiProviderResolver.CredentialsFrom(provider);
            var resp = await _jaijui.GetBalance(creds);

            var result = new ProviderBalanceResponse
            {
                ProviderID = provider.ProviderID,
                ProviderName = provider.ProviderName,
                ExtractedValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                RawResponse = Truncate(resp.Data ?? string.Empty, 4000)
            };

            // Provider returns HTTP 200 even on decline: transport failure OR an explicit FAILURE status
            // means we could not read a balance.
            string status = (resp.Status ?? string.Empty).Trim().ToUpperInvariant();
            if (!resp.Success || status == "FAILURE")
            {
                result.Status = StatusType.Failed;
                result.Message = string.IsNullOrWhiteSpace(resp.Message)
                    ? Messages.Provider.Message.ProviderUnreachable
                    : resp.Message;
                return (StatusType.Ok, result);
            }

            result.Status = StatusType.Success;
            result.DMTBalance = resp.Balance;
            result.Message = string.IsNullOrWhiteSpace(resp.Message)
                ? Messages.Provider.Message.BalanceFetched
                : resp.Message;
            result.ExtractedValues["DMTBalance"] = resp.Balance.ToString(CultureInfo.InvariantCulture);

            provider.DMTBalance = resp.Balance;
            await _providerRepo.Update(provider);

            return (StatusType.Ok, result);
        }

        // ---- Pay1 Utility (BBPS) ---------------------------------------------------
        //
        // Pay1 signs every call with sha1(partner_id + trans_id + api_key). A ProviderDetails URL
        // template cannot produce that hash, so this provider talks through the Fino.Pay1Api
        // library instead of an endpoint row - the same arrangement JaiJui uses for DMT.
        // Everything around it is unchanged: the route and the switching sequence still decide
        // that Pay1 serves this biller (ProviderConfiguration.OpCode carries the Pay1 product id),
        // every call is still written to the provider log, and the money rules still hold - a
        // reply we never received leaves the transaction PENDING, never FAILED.

        private async Task<RechargeProviderResult> DispatchPay1(
            ProviderMaster provider, ProviderConfiguration route, RechargeDispatchContext context)
        {
            var request = new Fino.Pay1Api.Requests.BillPaymentRequest
            {
                Operator = route.OpCode ?? string.Empty,
                AccountNumber = context.ConsumerNumber,
                Amount = context.Amount,
                Param = context.Param1,
                Param1 = context.Param2,
                RequestID = context.ProviderRequestID,
            };

            var reply = await _pay1.BillPayment(request, Providers.Pay1ProviderResolver.CredentialsFrom(provider));

            await WriteLog(provider.ProviderID, context.UserID, context.TransactionMasterID, context.ServiceID,
                context.IPAddress,
                $"Pay1 BillPayment operator={request.Operator} account={request.AccountNumber} amount={request.Amount:0.##} trans_id={request.RequestID}",
                reply.Attempted ? reply.Data : reply.Message);

            var result = new RechargeProviderResult
            {
                ProviderID = provider.ProviderID,
                ProviderRequestID = context.ProviderRequestID,
                ProviderRefNo = reply.TransactionID,
                RawResponse = Truncate(reply.Data, 4000),
                ProviderCommission = route.AdminComm > 0 ? route.AdminComm : null,
            };

            // Never sent: credentials or biller code missing. No money is at risk, so the caller
            // may try the next provider in the sequence.
            if (!reply.Attempted)
            {
                result.Status = StatusType.Failed;
                result.IsConfigured = false;
                result.Message = reply.Message;
                return result;
            }

            // Sent, but no readable answer: the payment may well have gone through - hold it.
            if (!reply.Success)
            {
                result.Status = StatusType.Pending;
                result.Message = string.IsNullOrWhiteSpace(reply.Message)
                    ? Messages.Provider.Message.AwaitingConfirmation : reply.Message;
                return result;
            }

            if (reply.Balance > 0) result.ProviderClosingBalance = reply.Balance;

            result.Status = reply.IsSuccess ? StatusType.Success
                          : reply.IsFailure ? StatusType.Failed
                          : StatusType.Pending;   // "pending" and anything unrecognised hold the money
            result.Message = string.IsNullOrWhiteSpace(reply.Message)
                ? DefaultMessageFor(result.Status) : reply.Message;
            return result;
        }

        private async Task<(string failureMessage, BillFetchResponse? data)> FetchPay1Bill(
            ProviderMaster provider, ProviderConfiguration route, BillFetchContext context)
        {
            var request = new Fino.Pay1Api.Requests.BillFetchRequest
            {
                Operator = route.OpCode ?? string.Empty,
                AccountNumber = context.ConsumerNumber,
                Param = context.Param1,
                Param1 = context.Param2,
            };

            var reply = await _pay1.BillFetch(request, Providers.Pay1ProviderResolver.CredentialsFrom(provider));

            await WriteLog(provider.ProviderID, context.UserID, transactionID: 0, context.ServiceID, context.IPAddress,
                $"Pay1 BillFetch operator={request.Operator} account={request.AccountNumber}",
                reply.Attempted ? reply.Data : reply.Message);

            // Could not reach Pay1 - no money involved, let the caller try the next provider.
            if (!reply.Attempted || !reply.Success) return (reply.Message, null);

            return (StatusType.Ok, new BillFetchResponse
            {
                Status = reply.IsSuccess ? StatusType.Success : StatusType.Failed,
                OperatorID = context.OperatorID,
                ProviderID = provider.ProviderID,
                CustomerName = reply.CustomerName,
                BillAmount = reply.BillAmount > 0 ? reply.BillAmount : null,
                DueDate = reply.DueDate,
                BillDate = reply.BillDate,
                BillNumber = reply.BillNumber,
                Message = reply.Message,
                RawResponse = Truncate(reply.Data, 4000),
            });
        }

        private async Task<ProviderStatusResult> CheckPay1Status(ProviderMaster provider, ProviderStatusContext context)
        {
            var reply = await _pay1.StatusCheck(
                new Fino.Pay1Api.Requests.StatusCheckRequest { RequestIDs = context.ProviderRequestID },
                Providers.Pay1ProviderResolver.CredentialsFrom(provider));

            await WriteLog(provider.ProviderID, context.UserID, context.TransactionMasterID, context.ServiceID,
                context.IPAddress, $"Pay1 StatusCheck trans_id={context.ProviderRequestID}",
                reply.Attempted ? reply.Data : reply.Message);

            var result = new ProviderStatusResult
            {
                ProviderRefNo = string.IsNullOrWhiteSpace(reply.TransactionID) ? context.ProviderRequestID : reply.TransactionID,
                RawResponse = Truncate(reply.Data, 4000),
            };

            // An enquiry we could not complete tells us nothing - leave the transaction alone.
            if (!reply.Attempted || !reply.Success)
            {
                result.Status = StatusType.Pending;
                result.Message = string.IsNullOrWhiteSpace(reply.Message)
                    ? Messages.Provider.Message.ProviderUnreachable : reply.Message;
                return result;
            }

            result.Status = reply.IsSuccess ? StatusType.Success
                          : reply.IsFailure ? StatusType.Failed
                          : StatusType.Pending;
            result.Message = string.IsNullOrWhiteSpace(reply.Message)
                ? DefaultMessageFor(result.Status) : reply.Message;
            return result;
        }

        private async Task<(string failureMessage, ProviderBalanceResponse? data)> CheckPay1Balance(ProviderMaster provider)
        {
            var reply = await _pay1.Balance(Providers.Pay1ProviderResolver.CredentialsFrom(provider));

            var result = new ProviderBalanceResponse
            {
                ProviderID = provider.ProviderID,
                ProviderName = provider.ProviderName,
                ExtractedValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                RawResponse = Truncate(reply.Data, 4000),
            };

            if (!reply.IsSuccess)
            {
                result.Status = StatusType.Failed;
                result.Message = string.IsNullOrWhiteSpace(reply.Message)
                    ? Messages.Provider.Message.ProviderUnreachable : reply.Message;
                return (StatusType.Ok, result);
            }

            result.Status = StatusType.Success;
            result.RechargeBalance = reply.Balance;
            result.Message = Messages.Provider.Message.BalanceFetched;
            result.ExtractedValues["Balance"] = reply.Balance.ToString(CultureInfo.InvariantCulture);

            // Keep the dashboard figure in step with the provider, exactly as JaiJui does.
            provider.RechargeBalance = reply.Balance;
            await _providerRepo.Update(provider);

            return (StatusType.Ok, result);
        }


        // ── Bill fetch (BBPS / utility bill details before payment) ─────────────────

        public async Task<(string failureMessage, BillFetchResponse? data)> FetchBill(BillFetchContext context)
        {
            if (context.OperatorID <= 0 || string.IsNullOrWhiteSpace(context.ConsumerNumber))
                return (Messages.Common.Message.InvalidRequest, null);

            // Same routing as recharge: operator → provider mappings in the configured sequence.
            var routes = (await _routeRepo.GetByOperatorID(context.OperatorID) ?? new List<ProviderConfiguration>())
                .Where(r => r.IsActive && (r.ServiceID == context.ServiceID || r.ServiceID == 0))
                .ToList();
            if (routes.Count == 0) return (Messages.Provider.Message.BillFetchNotConfigured, null);

            var sequenceOrder = await ResolveSequenceOrder(context.OperatorID, context.UserID);
            var orderedRoutes = routes
                .OrderBy(r => { int i = sequenceOrder.IndexOf(r.ProviderID); return i < 0 ? int.MaxValue : i; })
                .ThenByDescending(r => r.ServiceID == context.ServiceID)
                .ThenBy(r => r.ConfigurationID)
                .ToList();

            bool anyEndpoint = false;
            string lastMessage = Messages.Provider.Message.BillFetchNotConfigured;

            foreach (var route in orderedRoutes)
            {
                var provider = await _providerRepo.GetByID(route.ProviderID);
                if (provider == null || !provider.IsActive || provider.IsDelete) continue;

                if (Providers.Pay1ProviderResolver.IsPay1(provider.ProviderName))
                {
                    anyEndpoint = true;
                    var (pay1Failure, pay1Bill) = await FetchPay1Bill(provider, route, context);
                    if (pay1Bill == null) { lastMessage = pay1Failure; continue; } // no money involved - try the next provider
                    return (StatusType.Ok, pay1Bill);
                }

                var endpoint = await ResolveEndpoint(provider.ProviderID, KindBillFetch, context.ServiceID);
                if (endpoint == null) continue;
                anyEndpoint = true;

                var dispatchContext = new RechargeDispatchContext
                {
                    OperatorID = context.OperatorID,
                    ServiceID = context.ServiceID,
                    ConsumerNumber = context.ConsumerNumber,
                    Amount = 0,
                    UserID = context.UserID,
                    IPAddress = context.IPAddress,
                };
                var values = BuildRequestValues(provider, route, endpoint, dispatchContext, requestID: string.Empty);
                values["Param1"] = context.Param1 ?? string.Empty;
                values["Param2"] = context.Param2 ?? string.Empty;
                values["Param3"] = context.Param3 ?? string.Empty;

                var (rawResponse, transportState) = await ExecuteEndpoint(endpoint, values, provider.ProviderID,
                    context.UserID, transactionID: 0, context.ServiceID, context.IPAddress);

                if (transportState != TransportState.Completed)
                {
                    lastMessage = Messages.Provider.Message.ProviderUnreachable;
                    continue; // no money involved — safe to try the next provider
                }

                var rules = await GetActiveRules(provider.ProviderID, KindBillFetch);
                var outcome = EvaluateRules(rules, rawResponse);
                var captured = new Dictionary<string, string>(outcome.ExtractedValues, StringComparer.OrdinalIgnoreCase);
                MergeJsonFallback(rawResponse, captured);

                bool figuresPresent = captured.ContainsKey("BillAmount") || captured.ContainsKey("CustomerName");
                string providerMessage = FirstValue(captured, "Message", "Description");
                // A figures-only reply counts as success only when it carries no error message.
                bool cleanFigures = figuresPresent && providerMessage.Length == 0;
                string status = outcome.Matched ? outcome.MappedStatus : (cleanFigures ? StatusType.Success : StatusType.Failed);

                var response = new BillFetchResponse
                {
                    Status = status,
                    OperatorID = context.OperatorID,
                    ProviderID = provider.ProviderID,
                    CustomerName = FirstValue(captured, "CustomerName"),
                    DueDate = FirstValue(captured, "DueDate"),
                    BillDate = FirstValue(captured, "BillDate"),
                    BillNumber = FirstValue(captured, "BillNumber", "RefID"),
                    Message = providerMessage.Length > 0
                        ? providerMessage
                        : (status == StatusType.Success ? Messages.Provider.Message.BillFetched : Messages.Provider.Message.NoRuleMatched),
                    RawResponse = Truncate(rawResponse, 4000),
                };
                if (TryReadDecimal(captured, out var billAmount, "BillAmount", "Amount"))
                    response.BillAmount = billAmount;

                // The provider answered authoritatively (success OR an explicit failure like
                // "invalid consumer number") — return it; only transport problems fail over.
                return (StatusType.Ok, response);
            }

            return (anyEndpoint ? lastMessage : Messages.Provider.Message.BillFetchNotConfigured, null);
        }

        // ── Transaction status enquiry ───────────────────────────────────────────────

        public async Task<ProviderStatusResult> CheckTransactionStatus(ProviderStatusContext context, DateTime transactionDate)
        {
            var notConfigured = new ProviderStatusResult
            {
                Status = StatusType.Pending,
                IsConfigured = false,
                ProviderRefNo = context.ProviderRequestID,
                Message = Messages.Common.Message.ServiceUnavailable
            };

            if (context.ProviderID <= 0 || string.IsNullOrWhiteSpace(context.ProviderRequestID)) return notConfigured;

            var provider = await _providerRepo.GetByID(context.ProviderID);
            if (provider == null || provider.IsDelete) return notConfigured;

            if (Providers.Pay1ProviderResolver.IsPay1(provider.ProviderName))
                return await CheckPay1Status(provider, context);

            var endpoint = await ResolveEndpoint(context.ProviderID, KindStatus, context.ServiceID);
            if (endpoint == null) return notConfigured;

            // [Date] must reflect the transaction's own date (Payin: DateInteger=yyyyMMdd of the txn).
            var values = BuildRequestValues(provider, route: null, endpoint, context: null, requestID: context.ProviderRequestID);
            string dateFormat = string.IsNullOrWhiteSpace(endpoint.DateFormat) ? "yyyyMMdd" : endpoint.DateFormat;
            try { values["Date"] = transactionDate.ToString(dateFormat, CultureInfo.InvariantCulture); }
            catch { values["Date"] = transactionDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture); }

            var (rawResponse, transportState) = await ExecuteEndpoint(endpoint, values, context.ProviderID,
                context.UserID, context.TransactionMasterID, context.ServiceID, context.IPAddress);

            if (transportState != TransportState.Completed)
            {
                return new ProviderStatusResult
                {
                    Status = StatusType.Pending,
                    ProviderRefNo = context.ProviderRequestID,
                    RawResponse = Truncate(rawResponse, 4000),
                    Message = Messages.Provider.Message.ProviderUnreachable
                };
            }

            var rules = await GetActiveRules(context.ProviderID, KindStatus);
            var outcome = EvaluateRules(rules, rawResponse);

            var extracted = new Dictionary<string, string>(outcome.ExtractedValues, StringComparer.OrdinalIgnoreCase);
            MergeJsonFallback(rawResponse, extracted);

            return new ProviderStatusResult
            {
                // Unknown replies stay PENDING — money only moves on an explicit rule match.
                Status = outcome.Matched ? outcome.MappedStatus : StatusType.Pending,
                ProviderRefNo = FirstValue(extracted, "RefID", "TxnID", "TransactionID", "ReferenceNo"),
                RawResponse = Truncate(rawResponse, 4000),
                Message = FirstValue(extracted, "Message", "Description") is { Length: > 0 } msg
                    ? msg
                    : (outcome.Matched ? DefaultMessageFor(outcome.MappedStatus) : Messages.Provider.Message.AwaitingConfirmation)
            };
        }

        // ── Rule dry-run (admin "Test" button) ───────────────────────────────────────

        public async Task<(string failureMessage, ResponseRuleTestResponse? data)> TestResponse(TestProviderResponseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SampleText))
                return (Messages.Provider.Message.SampleRequired, null);

            var provider = await _providerRepo.GetByID(request.ProviderID);
            if (provider == null) return (Messages.Provider.Message.NotFound, null);

            string kind = NormalizeKind(request.RuleKind);
            var rules = await GetActiveRules(request.ProviderID, kind);
            var outcome = EvaluateRules(rules, request.SampleText);

            return (StatusType.Ok, new ResponseRuleTestResponse
            {
                Matched = outcome.Matched,
                MatchedRuleID = outcome.MatchedRuleID,
                MatchedPattern = outcome.MatchedPattern,
                MappedStatus = outcome.Matched ? outcome.MappedStatus : string.Empty,
                Message = outcome.Matched ? Messages.Provider.Message.RuleMatched : Messages.Provider.Message.NoRuleMatched,
                ExtractedValues = outcome.ExtractedValues
            });
        }

        // ── Placeholder catalogue (drives the UI helper chips) ───────────────────────

        public List<PlaceholderInfo> GetPlaceholders()
        {
            static PlaceholderInfo P(string token, string purpose, string direction) =>
                new() { Token = token, Purpose = purpose, Direction = direction };

            return new List<PlaceholderInfo>
            {
                // Request-side tokens (endpoint URL / body templates)
                P("[Number]", "Consumer / mobile number of the transaction", "request"),
                P("[Amount]", "Transaction amount", "request"),
                P("[OperatorCode]", "Provider-side operator code from the operator mapping", "request"),
                P("[RequestID]", "Our unique transaction reference (prefix applied)", "request"),
                P("[Type]", "Channel type from the operator mapping (e.g. STV flag)", "request"),
                P("[Lapu]", "SIM / lapu number from the operator mapping", "request"),
                P("[LoginID]", "Provider account login / member id", "request"),
                P("[Token]", "Provider API token / key", "request"),
                P("[Date]", "Current date, formatted using the endpoint's date format", "request"),
                P("[IP]", "Caller IP address", "request"),
                P("[Param1]", "Extra bill parameter 1 (e.g. billing unit / cycle from BBPS fields)", "request"),
                P("[Param2]", "Extra bill parameter 2", "request"),
                P("[Param3]", "Extra bill parameter 3", "request"),

                // Capture-side tokens (response rule templates)
                P("[RefID]", "Provider's reference number for the transaction", "capture"),
                P("[TxnID]", "Provider's transaction id", "capture"),
                P("[Message]", "Human-readable message from the provider", "capture"),
                P("[Balance]", "Remaining recharge/main balance at the provider", "capture"),
                P("[DMTBalance]", "Remaining payout/DMT balance at the provider", "capture"),
                P("[Commission]", "Commission the provider paid for this transaction", "capture"),
                P("[CustomerName]", "Bill fetch: customer / consumer name", "capture"),
                P("[BillAmount]", "Bill fetch: outstanding bill amount", "capture"),
                P("[DueDate]", "Bill fetch: bill due date", "capture"),
                P("[BillNumber]", "Bill fetch: bill number / reference", "capture"),
                P("[BillDate]", "Bill fetch: bill issue date", "capture"),
                P("[Skip1]", "Ignore a value (not stored)", "capture"),
                P("[Skip2]", "Ignore a value (not stored)", "capture"),
                P("[Skip3]", "Ignore a value (not stored)", "capture"),
            };
        }

        // ── Endpoint resolution + HTTP execution ─────────────────────────────────────

        private async Task<ProviderDetails?> ResolveEndpoint(int providerID, string kind, int serviceID)
        {
            var endpoints = await _endpointRepo.GetByProviderID(providerID);
            var ofKind = (endpoints ?? new List<ProviderDetails>())
                .Where(e => e.IsActive && NormalizeKind(e.UrlType) == kind)
                .ToList();
            if (ofKind.Count == 0) return null;

            // Prefer the exact service; fall back to any endpoint of the kind (balance/status
            // endpoints are usually account-wide rather than service-specific).
            return ofKind.FirstOrDefault(e => serviceID > 0 && e.ServiceID == serviceID)
                ?? (kind == KindTransaction ? null : ofKind[0]);
        }

        private enum TransportState { Completed, TimedOut, Unreachable }

        private async Task<(string body, TransportState state)> ExecuteEndpoint(
            ProviderDetails endpoint, Dictionary<string, string> values,
            int providerID, int userID, int transactionID, int serviceID, string ipAddress)
        {
            string rendered = RenderTemplate(endpoint.ProviderURL, values);
            bool isPost = string.Equals(endpoint.MethodType, "POST", StringComparison.OrdinalIgnoreCase);

            // POST templates may carry the JSON body after the endpoint path: https://host/path?{...}
            string url = rendered;
            string body = string.Empty;
            if (isPost)
            {
                int bodyStart = rendered.IndexOf("?{", StringComparison.Ordinal);
                if (bodyStart >= 0)
                {
                    url = rendered[..bodyStart];
                    body = rendered[(bodyStart + 1)..];
                }
            }

            string rawResponse;
            TransportState state = TransportState.Completed;
            try
            {
                var client = _httpClientFactory.CreateClient(nameof(ProviderGatewayService));
                client.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

                using var httpRequest = new HttpRequestMessage(isPost ? HttpMethod.Post : HttpMethod.Get, url);
                ApplyHeaders(httpRequest, endpoint.HeaderJson, out string? contentType);
                if (isPost)
                    httpRequest.Content = new StringContent(body, Encoding.UTF8, contentType ?? "application/json");

                using var httpResponse = await client.SendAsync(httpRequest);
                rawResponse = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                rawResponse = "TRANSPORT: request timed out";
                state = TransportState.TimedOut;
            }
            catch (HttpRequestException ex) when (
                ex.HttpRequestError is HttpRequestError.ConnectionError or HttpRequestError.NameResolutionError or HttpRequestError.SecureConnectionError)
            {
                rawResponse = $"TRANSPORT: {ex.HttpRequestError}";
                state = TransportState.Unreachable;
            }
            catch (Exception ex)
            {
                // Sent but the reply could not be read — treat like a timeout (do not fail money).
                _logger.Write(ex);
                rawResponse = "TRANSPORT: response could not be read";
                state = TransportState.TimedOut;
            }

            await WriteLog(providerID, userID, transactionID, serviceID, ipAddress, MaskSecrets(rendered, values), rawResponse);
            return (rawResponse, state);
        }

        private static void ApplyHeaders(HttpRequestMessage request, string headerJson, out string? contentType)
        {
            contentType = null;
            if (string.IsNullOrWhiteSpace(headerJson)) return;
            Dictionary<string, string>? headers = null;
            try { headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headerJson); }
            catch { /* malformed header JSON is ignored — the endpoint still fires with defaults */ }
            if (headers == null) return;

            foreach (var (key, value) in headers)
            {
                if (string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                    contentType = value;
                else
                    request.Headers.TryAddWithoutValidation(key, value);
            }
        }

        private async Task WriteLog(int providerID, int userID, int transactionID, int serviceID,
            string ipAddress, string request, string response)
        {
            try
            {
                await _logRepo.Insert(new ProviderRequestResponseLog
                {
                    ProviderID = providerID,
                    UserID = userID > 0 ? userID : null,
                    TransactionID = transactionID,
                    ServiceID = serviceID,
                    IPAddress = ipAddress,
                    Through = "ProviderGateway",
                    Request = Truncate(request, 8000),
                    Response = Truncate(response, 8000),
                    DateTime = DateTime.Now,
                    UpdatedateTime = DateTime.Now
                });
            }
            catch (Exception ex) { _logger.Write(ex); /* logging must never break a dispatch */ }
        }

        // ── Template rendering (request side) ────────────────────────────────────────

        private static readonly Regex TokenPattern = new(@"\[([A-Za-z0-9_]+)\]", RegexOptions.Compiled);

        private Dictionary<string, string> BuildRequestValues(
            ProviderMaster provider, ProviderConfiguration? route, ProviderDetails endpoint,
            RechargeDispatchContext? context, string requestID)
        {
            string dateFormat = string.IsNullOrWhiteSpace(endpoint.DateFormat) ? "dd/MM/yyyy" : endpoint.DateFormat;
            string date;
            try { date = DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture); }
            catch { date = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture); }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["LoginID"] = provider.LoginID ?? string.Empty,
                ["UserName"] = provider.LoginID ?? string.Empty,
                ["Token"] = provider.Token ?? string.Empty,
                ["Date"] = date,
                ["Number"] = context?.ConsumerNumber ?? string.Empty,
                ["Mobile"] = context?.ConsumerNumber ?? string.Empty,
                ["Amount"] = context?.Amount.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty,
                ["RequestID"] = requestID,
                ["OrderID"] = requestID,
                ["IP"] = context?.IPAddress ?? string.Empty,
                ["IPAddress"] = context?.IPAddress ?? string.Empty,
                ["OperatorCode"] = route?.OpCode ?? string.Empty,
                ["Operator"] = route?.OpCode ?? string.Empty,
                ["OpCode"] = route?.OpCode ?? string.Empty,
                ["Type"] = route?.Type ?? string.Empty,
                ["Lapu"] = route?.Lapu ?? string.Empty,
            };
            return values;
        }

        private static string RenderTemplate(string template, Dictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;
            return TokenPattern.Replace(template, m =>
                values.TryGetValue(m.Groups[1].Value, out var v) ? v : string.Empty);
        }

        // Replaces secret values with *** in the request log so tokens are not stored in plain text.
        private static string MaskSecrets(string rendered, Dictionary<string, string> values)
        {
            string masked = rendered;
            foreach (var key in new[] { "Token" })
            {
                if (values.TryGetValue(key, out var secret) && !string.IsNullOrEmpty(secret))
                    masked = masked.Replace(secret, "***");
            }
            return masked;
        }

        // ── Rule evaluation (response side) ──────────────────────────────────────────

        private async Task<List<ProviderResponseMaster>> GetActiveRules(int providerID, string kind)
        {
            var rules = await _ruleRepo.GetByProviderID(providerID);
            return (rules ?? new List<ProviderResponseMaster>())
                .Where(r => r.IsActive && NormalizeKind(r.Type) == kind)
                .ToList();
        }

        private sealed class RuleOutcome
        {
            public bool Matched { get; init; }
            public int? MatchedRuleID { get; init; }
            public string MatchedPattern { get; init; } = string.Empty;
            public string MappedStatus { get; init; } = StatusType.Pending;
            public Dictionary<string, string> ExtractedValues { get; init; } = new();
        }

        private static RuleOutcome EvaluateRules(List<ProviderResponseMaster> rules, string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse) || rules.Count == 0) return new RuleOutcome();

            // Most specific first: more match fragments, then longer overall match text.
            var ordered = rules
                .Select(r => new { Rule = r, Fragments = SplitFragments(r.TextMustexits) })
                .Where(x => x.Fragments.Count > 0)
                .OrderByDescending(x => x.Fragments.Count)
                .ThenByDescending(x => x.Rule.TextMustexits.Length)
                .ThenBy(x => x.Rule.ProviderResponseMasterID);

            foreach (var candidate in ordered)
            {
                bool allPresent = candidate.Fragments.All(f => FlexibleContains(rawResponse, f));
                if (!allPresent) continue;

                return new RuleOutcome
                {
                    Matched = true,
                    MatchedRuleID = candidate.Rule.ProviderResponseMasterID,
                    MatchedPattern = candidate.Rule.TextMustexits,
                    MappedStatus = NormalizeStatus(candidate.Rule.Status),
                    ExtractedValues = ExtractValues(candidate.Rule.Response, rawResponse)
                };
            }
            return new RuleOutcome();
        }

        // Match text = comma-separated fragments; every fragment must appear in the reply.
        private static List<string> SplitFragments(string mustExist)
        {
            if (string.IsNullOrWhiteSpace(mustExist)) return new List<string>();
            return mustExist.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(f => f.Length > 0)
                .ToList();
        }

        // Escapes a literal for regex use and makes JSON structural whitespace optional, so a
        // rule written compact ("status":"SUCCESS") still matches a pretty-printed reply
        // ("status": "SUCCESS") and vice versa.
        private static string EscapeFlexible(string literal)
        {
            return Regex.Escape(literal)
                .Replace("\\ ", "\\s*")
                .Replace(":", ":\\s*")
                .Replace(",", ",\\s*")
                .Replace("\\{", "\\{\\s*")
                .Replace("\\[", "\\[\\s*")
                .Replace("}", "\\s*}")
                .Replace("]", "\\s*]");
        }

        // Substring check tolerant of whitespace around JSON separators.
        private static bool FlexibleContains(string body, string fragment)
        {
            if (body.Contains(fragment, StringComparison.OrdinalIgnoreCase)) return true;
            try
            {
                return Regex.IsMatch(body, EscapeFlexible(fragment),
                    RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromSeconds(1));
            }
            catch (RegexMatchTimeoutException) { return false; }
        }

        // Turns a rule template into a regex — literal text is escaped, [Token] becomes a lazy
        // capture — then reads the captured values out of the actual reply.
        private static Dictionary<string, string> ExtractValues(string template, string rawResponse)
        {
            var extracted = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(template)) return extracted;

            var groupNames = new List<string>();
            var pattern = new StringBuilder();
            int cursor = 0;

            foreach (Match token in TokenPattern.Matches(template))
            {
                pattern.Append(EscapeFlexible(template[cursor..token.Index]));
                string group = $"g{groupNames.Count}";
                groupNames.Add(token.Groups[1].Value);
                pattern.Append($"(?<{group}>.*?)");
                cursor = token.Index + token.Length;
            }
            pattern.Append(EscapeFlexible(template[cursor..]));

            try
            {
                var match = Regex.Match(rawResponse, pattern.ToString(),
                    RegexOptions.Singleline | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));
                if (!match.Success) return extracted;

                for (int i = 0; i < groupNames.Count; i++)
                {
                    string name = groupNames[i];
                    if (name.StartsWith("Skip", StringComparison.OrdinalIgnoreCase)) continue;
                    if (name.StartsWith("NoUse", StringComparison.OrdinalIgnoreCase)) continue;
                    string value = match.Groups[$"g{i}"].Value.Trim().Trim('"');
                    if (value.Length > 0 && !extracted.ContainsKey(name)) extracted[name] = value;
                }
            }
            catch (RegexMatchTimeoutException) { /* a pathological template must not stall dispatch */ }

            return extracted;
        }

        // ── Normalisation helpers ────────────────────────────────────────────────────

        public static string NormalizeKind(string? kind)
        {
            string k = (kind ?? string.Empty).Trim().ToUpperInvariant();
            return k switch
            {
                "PAY" or "RECHARGE" or "TXN" or "TRANSACTION" or "INITIAL" => KindTransaction,
                "BAL" or "BALANCE" or "BALANCECHECK" => KindBalance,
                "STATUS" or "STATUSCHECK" or "ENQUIRY" or "CHECKSTATUS" => KindStatus,
                "BILL" or "BILLFETCH" or "FETCHBILL" or "BILLINFO" or "BILLDETAILS" => KindBillFetch,
                _ => k
            };
        }

        private static string NormalizeStatus(string? status)
        {
            string s = (status ?? string.Empty).Trim().ToUpperInvariant();
            if (s.StartsWith("SUCCESS") || s == "OK") return StatusType.Success;
            if (s.StartsWith("FAIL")) return StatusType.Failed;
            return StatusType.Pending; // PENDING / PROCESS / anything unknown holds the money
        }

        private static string DefaultMessageFor(string status) => status switch
        {
            StatusType.Success => Messages.Transaction.Message.RechargeSuccess,
            StatusType.Failed => Messages.Transaction.Message.RechargeFailedRefunded,
            _ => Messages.Transaction.Message.RechargePending
        };

        private static string FirstValue(Dictionary<string, string> values, params string[] keys)
        {
            foreach (var key in keys)
                if (values.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v)) return v;
            return string.Empty;
        }

        private static bool TryReadDecimal(Dictionary<string, string> values, out decimal result, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (values.TryGetValue(key, out var raw) &&
                    decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                    return true;
            }
            result = 0;
            return false;
        }

        private static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) || value.Length <= max ? value ?? string.Empty : value[..max];
    }
}
