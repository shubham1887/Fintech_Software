using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace FintechApp.Services
{
    public class MessageService : IMessageService
    {
        // Scoped repos below are used ONLY by the awaited Check* test methods, which run inside the
        // request scope. All fire-and-forget sends (SendSms/Otp/WhatsApp/Email/Event) run AFTER the
        // request returns, so they MUST NOT touch these — the request's DbContext is disposed by then.
        // They open a fresh DI scope via _scopeFactory instead (see RunInBackground).
        private readonly ISmsMasterRepository _smsMasterRepo;
        private readonly ISettingMasterRepository _settingRepo;
        private readonly ISentSMSRepository _sentSmsRepo;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly Logger _logger;

        public MessageService(
            ISmsMasterRepository smsMasterRepo,
            ISettingMasterRepository settingRepo,
            ISentSMSRepository sentSmsRepo,
            IServiceScopeFactory scopeFactory,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _smsMasterRepo = smsMasterRepo;
            _settingRepo = settingRepo;
            _sentSmsRepo = sentSmsRepo;
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = new Logger();
        }

        /// <summary>
        /// The brand name that fills {AppName} / {SystemName} / [SystemName] in every template.
        ///
        /// Falls back to Bootstrap:BrandName — the name the installer already collects and seeds
        /// the admin account with — because "Company:ClientName" is not in the template settings
        /// file and was therefore unset on every install, so every message a client's customers
        /// received was signed "System". A reseller install should not need a second place to
        /// type the same brand name.
        /// </summary>
        private string ClientName =>
            Pick(_configuration["Company:ClientName"], _configuration["Bootstrap:BrandName"]) ?? "System";

        private static string? Pick(params string?[] values) =>
            values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        /// <summary>
        /// The number that fills {SupportNo} / [SupportNo].
        ///
        /// The Message Settings screen offers {SupportNo} as a one-click chip and the templates
        /// that ship with the product used it, but nothing ever supplied it — so every customer who
        /// changed their password was told to "contact support immediately at {SupportNo}", with
        /// the braces. It reads from "HelplineNumber", the key the public registration screen
        /// already shows its helpline from, rather than adding a second place to type one number.
        ///
        /// Blank when the client has not set one. Blank is then tidied away by <see cref="Tidy"/>;
        /// what must never happen is the literal token reaching a customer.
        /// </summary>
        private static async Task<string> ResolveSupportNoAsync(ISettingMasterRepository settingRepo)
        {
            try { return (await GetSettingAsync(settingRepo, "HelplineNumber")).Trim(); }
            catch { return string.Empty; }
        }

        private static Task<string> ResolveSupportNoAsync(IServiceProvider sp) =>
            ResolveSupportNoAsync(sp.GetRequiredService<ISettingMasterRepository>());

        // Runs DB-touching work on a background thread inside its OWN DI scope, so it has a live
        // DbContext even though the originating HTTP request (and its scope) has already returned.
        // Fire-and-forget: exceptions are logged, never propagated.
        private void RunInBackground(Func<IServiceProvider, Task> work)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    await work(scope.ServiceProvider);
                }
                catch (Exception ex) { _logger.Write(ex); }
            });
        }

        // ─── Event-driven (settings-backed) sender ───────────────────────────

        public Task SendEventAsync(string eventType, int userId, string mobile, string? whatsAppNumber,
            string? emailId, IDictionary<string, string>? tokens, string fallbackSms,
            bool isOtpDefault = false, string emailSubject = "")
        {
            RunInBackground(async sp =>
            {
                var setting = await GetSettingByTypeLooseAsync(sp, eventType);
                string supportNo = await ResolveSupportNoAsync(sp);

                bool sendSms = setting?.IsSMS ?? true;   // no template configured → still send fallback SMS
                bool sendWa = setting?.IsWhatsUp ?? false;
                bool sendMail = setting?.IsMail ?? false;
                bool isOtp = setting?.IsOTP ?? isOtpDefault;
                string templateId = setting?.TemplateID ?? string.Empty;

                if (sendSms)
                {
                    string tpl = !string.IsNullOrWhiteSpace(setting?.SmsMessage) ? setting!.SmsMessage : fallbackSms;
                    string body = ApplyTokens(tpl, tokens, supportNo);
                    if (!string.IsNullOrWhiteSpace(body))
                        await SendSmsCoreAsync(sp, userId, mobile, body, templateId, eventType, isOtp);
                }

                if (sendWa && !string.IsNullOrWhiteSpace(whatsAppNumber))
                {
                    string tpl = !string.IsNullOrWhiteSpace(setting?.WhatsUpMessage) ? setting!.WhatsUpMessage
                               : (!string.IsNullOrWhiteSpace(setting?.SmsMessage) ? setting!.SmsMessage : fallbackSms);
                    string body = ApplyTokens(tpl, tokens, supportNo);
                    if (!string.IsNullOrWhiteSpace(body))
                        await SendWhatsAppCoreAsync(sp, userId, mobile, whatsAppNumber!, body, templateId, eventType);
                }

                if (sendMail && !string.IsNullOrWhiteSpace(emailId))
                {
                    string tpl = !string.IsNullOrWhiteSpace(setting?.EmailMessage) ? setting!.EmailMessage
                               : (!string.IsNullOrWhiteSpace(setting?.SmsMessage) ? setting!.SmsMessage : fallbackSms);
                    string body = ApplyTokens(tpl, tokens, supportNo);
                    string subject = string.IsNullOrWhiteSpace(emailSubject) ? eventType : emailSubject;
                    if (!string.IsNullOrWhiteSpace(body))
                        SendEmailCore(sp.GetRequiredService<ISettingMasterRepository>(), emailId!, body, subject);
                }
            });
            return Task.CompletedTask;
        }

        // Match by Type ignoring case, spaces and punctuation so the UI's display names
        // ("Login OTP", "Forgot Password OTP") resolve against code events ("LoginOTP", ...).
        private static async Task<MessageSetting?> GetSettingByTypeLooseAsync(IServiceProvider sp, string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return null;
            var all = await sp.GetRequiredService<IMessageSettingRepository>().GetAll();
            return all.FirstOrDefault(x => StaticModels.MessageEventKey.Matches(x.Type, type));
        }

        // Fill both {Token} and [Token] placeholders. Built-ins are always available; caller-supplied
        // tokens override them.
        private string ApplyTokens(string template, IDictionary<string, string>? tokens, string supportNo)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["AppName"] = ClientName,
                ["SystemName"] = ClientName,
                ["SupportNo"] = supportNo ?? string.Empty,
                ["Date"] = DateTime.Now.ToString("dd/MMM/yyyy"),
                ["DateTime"] = DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"),
            };
            if (tokens != null)
                foreach (var kv in tokens) map[kv.Key] = kv.Value ?? string.Empty;

            string result = template.Replace("%20", " ");
            foreach (var kv in map)
                result = result.Replace("{" + kv.Key + "}", kv.Value)
                               .Replace("[" + kv.Key + "]", kv.Value);
            return Tidy(result);
        }

        /// <summary>
        /// Clear up what an empty token leaves behind.
        ///
        /// Several events supply a token only sometimes: Remark is blank on most wallet movements
        /// and on an approval nobody annotated, SupportNo is blank until the client sets a
        /// helpline. Substituting them raw produced real messages reading "Reason: . Please contact
        /// your upline" and WhatsApp receipts with a dangling "Remark:" line. Rather than needing a
        /// template per combination of present and absent values, a label whose value came back
        /// empty is dropped here.
        ///
        /// Deliberately narrow: a line has to consist of nothing but "Label:" to be removed, and an
        /// inline label has to be followed immediately by sentence punctuation. Ordinary prose
        /// never looks like that, so nothing a template author wrote is at risk.
        /// </summary>
        private static string Tidy(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            text = EmptyLabelLine.Replace(text, string.Empty);    // WhatsApp: a whole "Remark:" line
            text = EmptyLabelInline.Replace(text, string.Empty);  // SMS: "... rejected. Reason: . Please"
            text = DanglingPreposition.Replace(text, string.Empty); // "contact support at ." → "contact support."
            text = RepeatedStop.Replace(text, ".");
            text = RunsOfSpaces.Replace(text, " ");
            text = SpaceBeforeStop.Replace(text, "$1");
            text = RunsOfBlankLines.Replace(text, "\n\n");
            return text.Trim();
        }

        private static readonly Regex EmptyLabelLine =
            new(@"^[ \t]*[^:\r\n]{1,40}:[ \t]*\r?$\r?\n?", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex EmptyLabelInline =
            new(@"[ \t]*\b[A-Za-z][A-Za-z ]{0,24}:[ \t]*(?=[.,;])", RegexOptions.Compiled);
        // "contact support immediately at {SupportNo}." with no support number set reads
        // "...immediately at." English prose never ends a clause on one of these words, so a
        // preposition left sitting against punctuation is always a token that resolved to nothing.
        private static readonly Regex DanglingPreposition =
            new(@"[ \t]+(?:at|on|to|via|from|call)[ \t]*(?=[.,;!?])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RepeatedStop = new(@"\.{2,}", RegexOptions.Compiled);
        private static readonly Regex RunsOfSpaces = new(@"[ \t]{2,}", RegexOptions.Compiled);
        private static readonly Regex SpaceBeforeStop = new(@"[ \t]+([.,;!?])", RegexOptions.Compiled);
        private static readonly Regex RunsOfBlankLines = new(@"(\r?\n){3,}", RegexOptions.Compiled);

        // ─── SMS ─────────────────────────────────────────────────────────────

        public Task SendSmsAsync(int userId, string mobile, string message, string templateId, string type)
        {
            RunInBackground(sp => SendSmsCoreAsync(sp, userId, mobile, message, templateId, type, isOtp: false));
            return Task.CompletedTask;
        }

        public Task SendOtpSmsAsync(int userId, string mobile, string message, string templateId, string type)
        {
            RunInBackground(sp => SendSmsCoreAsync(sp, userId, mobile, message, templateId, type, isOtp: true));
            return Task.CompletedTask;
        }

        public Task SendWhatsAppAsync(int userId, string mobile, string whatsAppNumber, string message, string templateId, string type)
        {
            RunInBackground(sp => SendWhatsAppCoreAsync(sp, userId, mobile, whatsAppNumber, message, templateId, type));
            return Task.CompletedTask;
        }

        // Send one SMS (normal or OTP url) and log it. MUST run inside a background scope (sp).
        private async Task SendSmsCoreAsync(IServiceProvider sp, int userId, string mobile, string message,
            string templateId, string type, bool isOtp)
        {
            var sms = await GetActiveSmsMasterAsync(sp);
            if (sms == null) return;

            message = ReplaceTokens(message, await ResolveSupportNoAsync(sp));
            string baseTemplate = isOtp && !string.IsNullOrWhiteSpace(sms.SMSOTPURL) ? sms.SMSOTPURL : sms.SMSString;
            string url = BuildSmsUrl(baseTemplate, mobile, message, templateId);

            var sentSms = BuildSentSms(userId, mobile, message, type, sms.SMSMasterID);
            sentSms.TemplateID = templateId ?? string.Empty;
            sentSms.Response = sms.MethodType.Equals("POST", StringComparison.OrdinalIgnoreCase)
                ? await CallPostAsync(url, sms.HeaderJson)
                : await CallGetAsync(url);
            sentSms.Request = url;
            await sp.GetRequiredService<ISentSMSRepository>().Insert(sentSms);
        }

        // Send one WhatsApp message and log it. MUST run inside a background scope (sp).
        private async Task SendWhatsAppCoreAsync(IServiceProvider sp, int userId, string mobile, string whatsAppNumber,
            string message, string templateId, string type)
        {
            var sms = await GetActiveSmsMasterAsync(sp);
            if (sms == null || string.IsNullOrWhiteSpace(sms.WhatsAppURL)) return;

            message = ReplaceTokens(message, await ResolveSupportNoAsync(sp));
            string number = string.IsNullOrWhiteSpace(whatsAppNumber) ? mobile : whatsAppNumber;
            if (sms.MobileWithOrWithout91 == "Yes") number = "91" + number;

            string url = ReplaceCI(
                ReplaceCI(
                    ReplaceCI(sms.WhatsAppURL, "[mobile]", Uri.EscapeDataString(number)),
                    "[message]", Uri.EscapeDataString(message)),
                "[TemplateID]", Uri.EscapeDataString(templateId ?? ""));

            var sentSms = BuildSentSms(userId, mobile, message, type, sms.SMSMasterID);
            sentSms.WhatsAppNumber = whatsAppNumber;
            sentSms.TemplateID = templateId ?? string.Empty;
            sentSms.Response = sms.MethodTypeWhatsApp.Equals("POST", StringComparison.OrdinalIgnoreCase)
                ? await CallPostAsync(url, sms.HeaderJsonWhatsApp)
                : await CallGetAsync(url);
            sentSms.Request = url;
            await sp.GetRequiredService<ISentSMSRepository>().Insert(sentSms);
        }

        public async Task<string> CheckSmsAsync(string mobile, string message, string templateId)
        {
            try
            {
                var sms = await GetActiveSmsMasterAsync();
                if (sms == null) return "SMS URL not active";

                message = ReplaceTokens(message, await ResolveSupportNoAsync(_settingRepo));
                string url = BuildSmsUrl(sms.SMSString, mobile, message, templateId);

                string response = sms.MethodType.Equals("POST", StringComparison.OrdinalIgnoreCase)
                    ? await CallPostAsync(url, sms.HeaderJson)
                    : await CallGetAsync(url);

                var sentSms = BuildSentSms(0, mobile, message, "Testing", sms.SMSMasterID);
                sentSms.Request = url;
                sentSms.Response = response;
                await _sentSmsRepo.Insert(sentSms);
                return response;
            }
            catch (Exception ex) { _logger.Write(ex); return ex.Message; }
        }

        public async Task<string> CheckWhatsAppAsync(string whatsAppNumber, string message)
        {
            try
            {
                var sms = await GetActiveSmsMasterAsync();
                if (sms == null || string.IsNullOrWhiteSpace(sms.WhatsAppURL)) return "WhatsApp URL not active";

                message = ReplaceTokens(message, await ResolveSupportNoAsync(_settingRepo));
                string number = sms.MobileWithOrWithout91 == "Yes" ? "91" + whatsAppNumber : whatsAppNumber;
                string url = ReplaceCI(
                    ReplaceCI(
                        ReplaceCI(sms.WhatsAppURL, "[mobile]", Uri.EscapeDataString(number)),
                        "[message]", Uri.EscapeDataString(message)),
                    "[TemplateID]", "");

                string response = sms.MethodTypeWhatsApp.Equals("POST", StringComparison.OrdinalIgnoreCase)
                    ? await CallPostAsync(url, sms.HeaderJsonWhatsApp)
                    : await CallGetAsync(url);

                var sentSms = BuildSentSms(0, whatsAppNumber, message, "Testing", sms.SMSMasterID);
                sentSms.Request = url;
                sentSms.Response = response;
                await _sentSmsRepo.Insert(sentSms);
                return response;
            }
            catch (Exception ex) { _logger.Write(ex); return ex.Message; }
        }

        // ─── Email ───────────────────────────────────────────────────────────

        public Task SendEmailAsync(string emailId, string message, string subject, string attachment = "")
        {
            if (!IsValidEmail(emailId)) return Task.CompletedTask;
            RunInBackground(async sp =>
            {
                var settingRepo = sp.GetRequiredService<ISettingMasterRepository>();
                string body = ReplaceTokens(message, await ResolveSupportNoAsync(settingRepo));
                SendEmailCore(settingRepo, emailId, body, subject, attachment);
            });
            return Task.CompletedTask;
        }

        public async Task<string> CheckEmailAsync(string emailId, string message, string subject)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsValidEmail(emailId)) return "Invalid email: " + emailId;
                    SendEmailCore(_settingRepo, emailId, message, subject);
                    return "Mail sent successfully";
                }
                catch (Exception ex) { _logger.Write(ex); return ex.Message; }
            });
        }

        // ─── Private helpers ─────────────────────────────────────────────────

        // Awaited (request-scope) callers use the injected repo; background callers pass a scoped one.
        private async Task<SmsMaster?> GetActiveSmsMasterAsync() => Pick(await _smsMasterRepo.GetAll());

        private static async Task<SmsMaster?> GetActiveSmsMasterAsync(IServiceProvider sp) =>
            Pick(await sp.GetRequiredService<ISmsMasterRepository>().GetAll());

        private static SmsMaster? Pick(List<SmsMaster> all) => all.FirstOrDefault(x => x.IsActive && !x.IsDeleted);

        private static async Task<string> GetSettingAsync(ISettingMasterRepository settingRepo, string key, string defaultValue = "")
        {
            var all = await settingRepo.GetAll();
            return all.FirstOrDefault(x => x.Key == key && !x.IsDeleted)?.Value ?? defaultValue;
        }

        // The placeholder spellings a gateway URL may use for the DLT template id. Indian SMS
        // gateways all require one but none of them agree on what to call it, and the SMS Setting
        // screen takes the URL verbatim from whatever the provider's docs say — so a URL written
        // with [DLT_TE_ID] or [template_id] used to ship the placeholder through unreplaced while
        // the template id configured against the message sat unused, and the gateway answered
        // "template id missing".
        private static readonly string[] TemplateIdTokens =
        {
            "[TemplateID]", "[TEMPLATEID]", "[TEMPLATE_ID]", "[DLT_TE_ID]", "[DLTTEID]",
            "[TempID]", "[TID]", "{TemplateID}", "{template_id}",
        };

        private string BuildSmsUrl(string template, string mobile, string message, string templateId)
        {
            string url = ReplaceCI(
                ReplaceCI(template, "[mobile]", Uri.EscapeDataString(mobile ?? "")),
                "[message]", Uri.EscapeDataString(message ?? ""));

            string encodedTemplateId = Uri.EscapeDataString(templateId ?? string.Empty);
            foreach (var token in TemplateIdTokens)
                url = ReplaceCI(url, token, encodedTemplateId);

            return url;
        }

        // Case-insensitive literal replace — gateway URLs use [MOBILE]/[MESSAGE] (uppercase) while the
        // code historically only replaced lowercase, so placeholders were shipping unfilled. MatchEvaluator
        // avoids '$' being treated as a substitution group in the replacement text.
        private static string ReplaceCI(string input, string token, string replacement) =>
            string.IsNullOrEmpty(input) ? input
            : Regex.Replace(input, Regex.Escape(token), _ => replacement, RegexOptions.IgnoreCase);

        /// <summary>
        /// Fill the always-available tokens on a message that did NOT come through
        /// <see cref="SendEventAsync"/> — the DMT registration OTPs, which render their own template
        /// before handing it over, and the admin's Check SMS / Check WhatsApp test sends.
        ///
        /// Both brace styles are handled. The Message Settings screen writes its placeholder chips
        /// as {AppName}, and this path only ever replaced the bracket spelling, so a DMT template
        /// edited through that screen — the only way an admin can edit it — shipped "{AppName}"
        /// verbatim to the customer.
        /// </summary>
        private string ReplaceTokens(string msg, string supportNo)
        {
            string result = (msg ?? string.Empty).Replace("%20", " ");

            foreach (var (token, value) in new[]
            {
                ("AppName", ClientName),
                ("SystemName", ClientName),
                ("SupportNo", supportNo ?? string.Empty),
                ("Date", DateTime.Now.ToString("dd/MMM/yyyy")),
                ("DateTime", DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt")),
            })
            {
                result = result.Replace("{" + token + "}", value)
                               .Replace("[" + token + "]", value);
            }

            return Tidy(result);
        }

        private static SentSMS BuildSentSms(int userId, string mobile, string message, string type, int smsMasterId) =>
            new() { UserID = userId, Mobile = mobile, Message = message, Type = type, SMSMasterID = smsMasterId, DateTime = DateTime.Now };

        private async Task<string> CallGetAsync(string url)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                return await client.GetStringAsync(url);
            }
            catch (Exception ex) { _logger.Write(ex); return ""; }
        }

        private async Task<string> CallPostAsync(string urlWithBody, string headerJson)
        {
            try
            {
                var uri = new Uri(urlWithBody);
                string baseUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
                string body = urlWithBody.Contains('?') ? urlWithBody[(urlWithBody.IndexOf('?') + 1)..] : "";

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                if (!string.IsNullOrWhiteSpace(headerJson))
                {
                    try
                    {
                        var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headerJson);
                        if (headers != null)
                            foreach (var h in headers.Where(h => h.Key != "Content-Type"))
                                client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);
                    }
                    catch { }
                }

                var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                var resp = await client.PostAsync(baseUrl, content);
                return await resp.Content.ReadAsStringAsync();
            }
            catch (Exception ex) { _logger.Write(ex); return ""; }
        }

        private void SendEmailCore(ISettingMasterRepository settingRepo, string emailId, string message, string subject, string attachment = "")
        {
            string fromAddress = GetSettingAsync(settingRepo, "EmailIDForSend").GetAwaiter().GetResult();
            string fromPassword = GetSettingAsync(settingRepo, "EmailPassword").GetAwaiter().GetResult();
            string smtpPort = GetSettingAsync(settingRepo, "SMTPPort", "587").GetAwaiter().GetResult();
            string smtpHost = GetSettingAsync(settingRepo, "SMTPHost").GetAwaiter().GetResult();

            if (!IsValidEmail(fromAddress) || string.IsNullOrWhiteSpace(smtpHost)) return;

            using var mail = new MailMessage();
            mail.From = new MailAddress(fromAddress, ClientName);
            mail.Subject = subject;
            mail.Body = message;
            mail.IsBodyHtml = true;
            mail.To.Add(emailId);

            using var smtp = new SmtpClient(smtpHost, int.Parse(smtpPort))
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress, fromPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            smtp.Send(mail);
        }

        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try { var a = new MailAddress(email); return a.Address == email.Trim(); }
            catch { return false; }
        }
    }
}
