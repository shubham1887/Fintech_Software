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

        private string ClientName => _configuration["Company:ClientName"] ?? "System";

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

                bool sendSms = setting?.IsSMS ?? true;   // no template configured → still send fallback SMS
                bool sendWa = setting?.IsWhatsUp ?? false;
                bool sendMail = setting?.IsMail ?? false;
                bool isOtp = setting?.IsOTP ?? isOtpDefault;
                string templateId = setting?.TemplateID ?? string.Empty;

                if (sendSms)
                {
                    string tpl = !string.IsNullOrWhiteSpace(setting?.SmsMessage) ? setting!.SmsMessage : fallbackSms;
                    string body = ApplyTokens(tpl, tokens);
                    if (!string.IsNullOrWhiteSpace(body))
                        await SendSmsCoreAsync(sp, userId, mobile, body, templateId, eventType, isOtp);
                }

                if (sendWa && !string.IsNullOrWhiteSpace(whatsAppNumber))
                {
                    string tpl = !string.IsNullOrWhiteSpace(setting?.WhatsUpMessage) ? setting!.WhatsUpMessage
                               : (!string.IsNullOrWhiteSpace(setting?.SmsMessage) ? setting!.SmsMessage : fallbackSms);
                    string body = ApplyTokens(tpl, tokens);
                    if (!string.IsNullOrWhiteSpace(body))
                        await SendWhatsAppCoreAsync(sp, userId, mobile, whatsAppNumber!, body, templateId, eventType);
                }

                if (sendMail && !string.IsNullOrWhiteSpace(emailId))
                {
                    string tpl = !string.IsNullOrWhiteSpace(setting?.EmailMessage) ? setting!.EmailMessage
                               : (!string.IsNullOrWhiteSpace(setting?.SmsMessage) ? setting!.SmsMessage : fallbackSms);
                    string body = ApplyTokens(tpl, tokens);
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
            string norm = NormalizeType(type);
            return all.FirstOrDefault(x => NormalizeType(x.Type) == norm);
        }

        private static string NormalizeType(string s) =>
            new string((s ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        // Fill both {Token} and [Token] placeholders. Built-ins are always available; caller-supplied
        // tokens override them.
        private string ApplyTokens(string template, IDictionary<string, string>? tokens)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["AppName"] = ClientName,
                ["SystemName"] = ClientName,
                ["Date"] = DateTime.Now.ToString("dd/MMM/yyyy"),
                ["DateTime"] = DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"),
            };
            if (tokens != null)
                foreach (var kv in tokens) map[kv.Key] = kv.Value ?? string.Empty;

            string result = template.Replace("%20", " ");
            foreach (var kv in map)
                result = result.Replace("{" + kv.Key + "}", kv.Value)
                               .Replace("[" + kv.Key + "]", kv.Value);
            return result;
        }

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

            message = ReplaceTokens(message);
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

            message = ReplaceTokens(message);
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

                message = ReplaceTokens(message);
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

                message = ReplaceTokens(message);
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
            RunInBackground(sp =>
            {
                SendEmailCore(sp.GetRequiredService<ISettingMasterRepository>(), emailId, ReplaceTokens(message), subject, attachment);
                return Task.CompletedTask;
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

        private string BuildSmsUrl(string template, string mobile, string message, string templateId) =>
            ReplaceCI(
                ReplaceCI(
                    ReplaceCI(template, "[mobile]", Uri.EscapeDataString(mobile ?? "")),
                    "[message]", Uri.EscapeDataString(message ?? "")),
                "[TemplateID]", Uri.EscapeDataString(templateId ?? ""));

        // Case-insensitive literal replace — gateway URLs use [MOBILE]/[MESSAGE] (uppercase) while the
        // code historically only replaced lowercase, so placeholders were shipping unfilled. MatchEvaluator
        // avoids '$' being treated as a substitution group in the replacement text.
        private static string ReplaceCI(string input, string token, string replacement) =>
            string.IsNullOrEmpty(input) ? input
            : Regex.Replace(input, Regex.Escape(token), _ => replacement, RegexOptions.IgnoreCase);

        private string ReplaceTokens(string msg) =>
            (msg ?? "")
                .Replace("%20", " ")
                .Replace("[SystemName]", ClientName)
                .Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"));

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
