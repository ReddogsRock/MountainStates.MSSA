using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oqtane.Modules;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Repository;

namespace MountainStates.MSSA.Module.MSSA_Events.Manager
{
    public class MailgunService : IMailgunService, ITransientService
    {
        // Mailgun's batch-send limit per API call - each call here covers one chunk of
        // recipients, not the whole list at once.
        private const int BatchSize = 1000;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IMSSA_MailingListRepository _mailingListRepository;
        private readonly ILogger<MailgunService> _logger;

        public MailgunService(IHttpClientFactory httpClientFactory, IConfiguration configuration,
            IMSSA_MailingListRepository mailingListRepository, ILogger<MailgunService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _mailingListRepository = mailingListRepository;
            _logger = logger;
        }

        public async Task SendEventAnnouncementAsync(MSSA_Event evt, string baseUrl, int moduleId)
        {
            try
            {
                var baseApiUrl = _configuration["Mailgun:BaseUrl"];
                var domain = _configuration["Mailgun:Domain"];
                var fromAddress = _configuration["Mailgun:FromAddress"];
                var templateName = _configuration["Mailgun:AnnouncementTemplateName"];
                var apiKey = _configuration["Mailgun:ApiKey"];

                if (string.IsNullOrEmpty(baseApiUrl) || string.IsNullOrEmpty(domain) ||
                    string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(templateName) || string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Mailgun is not fully configured - event announcement for {EventId} was not sent.", evt.EventId);
                    return;
                }

                var subscribers = await _mailingListRepository.GetActiveSubscribersAsync();
                if (subscribers.Count == 0)
                {
                    _logger.LogError("No active mailing list subscribers - event announcement for {EventId} was not sent.", evt.EventId);
                    return;
                }

                var client = _httpClientFactory.CreateClient("Mailgun");
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{apiKey}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                // Event-level variables are the same for every recipient/batch; built once.
                var eventVariablesJson = JsonSerializer.Serialize(BuildEventVariables(evt, baseUrl, moduleId));
                var subject = $"New MSSA Event: {evt.EventName}";
                var requestUrl = $"{baseApiUrl}/{domain}/messages";

                foreach (var batch in subscribers.Chunk(BatchSize))
                {
                    // recipient-variables is what makes this a batch send - Mailgun splits
                    // delivery so each recipient's copy only shows their own address in To,
                    // never the rest of the batch. Per-recipient values (first_name) land in
                    // the same template variable namespace as the event-level ones above, so
                    // the template just references {{first_name}} like any other variable.
                    var recipientVariables = batch.ToDictionary(
                        s => s.Email,
                        s => new Dictionary<string, string> { { "first_name", s.FirstName ?? "" } });

                    var form = new Dictionary<string, string>
                    {
                        { "from", fromAddress },
                        { "to", string.Join(",", batch.Select(s => s.Email)) },
                        { "subject", subject },
                        { "template", templateName },
                        { "h:X-Mailgun-Variables", eventVariablesJson },
                        { "recipient-variables", JsonSerializer.Serialize(recipientVariables) }
                    };

                    var response = await client.PostAsync(requestUrl, new FormUrlEncodedContent(form));

                    if (!response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Mailgun send failed for event {EventId}, batch of {Count} ({StatusCode}): {Body}",
                            evt.EventId, batch.Length, response.StatusCode, body);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event announcement for event {EventId}", evt.EventId);
            }
        }

        // Fixed, non-editable header/footer every "Send a Campaign" email gets - the
        // admin only ever controls what goes in the middle (bodyHtml). No Mailgun
        // Template involved here (unlike SendEventAnnouncementAsync) - the admin
        // composes an ad hoc body each time, so there's nothing worth authoring once
        // in Mailgun's dashboard. Wrapped in a real document with an explicit charset
        // so nothing downstream has to guess the encoding - see
        // scripts/Send-RenewalCampaign.ps1 for the mojibake bug this avoids.
        private const string CampaignLogoUrl = "https://mssadogs.com/files/id/2";

        private static string BuildCampaignHtml(string bodyHtml)
        {
            return $@"<!DOCTYPE html>
<html>
<head>
<meta charset=""UTF-8"">
</head>
<body>
<div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;"">
  <img src=""{CampaignLogoUrl}"" alt=""Mountain States Stockdog Association"" style=""max-width: 400px; margin-bottom: 16px;"">

{bodyHtml}

  <hr style=""margin-top: 24px; border: none; border-top: 1px solid #ddd;"">
  <p><a href=""https://mssadogs.com"">Visit mssadogs.com</a></p>
  <p style=""font-size: 12px; color: #888;"">
    <a href=""%unsubscribe_url%"" style=""color: #888;"">Unsubscribe from MSSA</a>
  </p>
</div>
</body>
</html>";
        }

        // {FirstName}/{RenewalLink} are the admin-facing placeholders in the composer -
        // friendlier than asking her to learn Mailgun's own %recipient.xxx% merge
        // syntax, so this is the one place that translates between the two.
        private static string ReplaceFriendlyPlaceholders(string text) =>
            (text ?? "")
                .Replace("{FirstName}", "%recipient.FirstName%")
                .Replace("{RenewalLink}", "%recipient.RenewalLink%");

        // Same link format as MSSA_MailingListController.ExportExpiring's CSV export -
        // keep the two in sync if this ever changes.
        private static string BuildRenewalLink(int? handlerId) =>
            handlerId.HasValue ? $"https://mssadogs.com/memberservices/handlers/*/44/Detail?id={handlerId.Value}" : "";

        public async Task<int> SendCampaignAsync(string subject, string bodyHtml, List<MSSA_MailingListSubscriber> recipients, int moduleId)
        {
            var baseApiUrl = _configuration["Mailgun:BaseUrl"];
            var domain = _configuration["Mailgun:Domain"];
            var fromAddress = _configuration["Mailgun:FromAddress"];
            var apiKey = _configuration["Mailgun:ApiKey"];

            if (string.IsNullOrEmpty(baseApiUrl) || string.IsNullOrEmpty(domain) ||
                string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Mailgun is not fully configured - check Mailgun:BaseUrl/Domain/FromAddress/ApiKey.");
            }

            if (recipients == null || recipients.Count == 0)
            {
                return 0;
            }

            var handlerIdsByEmail = await _mailingListRepository.GetHandlerIdsByEmailAsync();

            var client = _httpClientFactory.CreateClient("Mailgun");
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{apiKey}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var mailgunSubject = ReplaceFriendlyPlaceholders(subject);
            var mailgunHtml = BuildCampaignHtml(ReplaceFriendlyPlaceholders(bodyHtml));
            var requestUrl = $"{baseApiUrl}/{domain}/messages";

            var sentCount = 0;

            foreach (var batch in recipients.Chunk(BatchSize))
            {
                var recipientVariables = batch.ToDictionary(
                    s => s.Email,
                    s =>
                    {
                        var key = s.Email.Trim().ToLowerInvariant();
                        var handlerId = handlerIdsByEmail.TryGetValue(key, out var id) ? (int?)id : null;
                        return new Dictionary<string, string>
                        {
                            { "FirstName", s.FirstName ?? "" },
                            { "RenewalLink", BuildRenewalLink(handlerId) }
                        };
                    });

                var form = new Dictionary<string, string>
                {
                    { "from", fromAddress },
                    { "to", string.Join(",", batch.Select(s => s.Email)) },
                    { "subject", mailgunSubject },
                    { "html", mailgunHtml },
                    { "recipient-variables", JsonSerializer.Serialize(recipientVariables) },
                    { "o:tracking-clicks", "no" }
                };

                // Explicit charset here, rather than relying on FormUrlEncodedContent's
                // default header - same lesson as the PowerShell script fix, applied
                // here as cheap insurance even though .NET's own percent-encoding is
                // already correct UTF-8 regardless of this header.
                var content = new FormUrlEncodedContent(form);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") { CharSet = "utf-8" };

                var response = await client.PostAsync(requestUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Mailgun campaign send failed for batch of {Count} ({StatusCode}): {Body}",
                        batch.Length, response.StatusCode, body);

                    // Unlike SendEventAnnouncementAsync, this is a deliberate admin
                    // action - throw so the controller can report a real failure
                    // instead of a false "sent" success.
                    throw new InvalidOperationException($"Mailgun rejected the send ({response.StatusCode}): {body}");
                }

                sentCount += batch.Length;
            }

            return sentCount;
        }

        // Variable names here are the contract with the Mailgun template - the template
        // references them as {{event_name}}, {{#if entry_link}}...{{/if}}, etc. An empty
        // string for a missing optional field reads as falsy in Handlebars, so {{#if}}
        // blocks in the template correctly hide an "Enter This Event" link, flyer link,
        // etc. when there's nothing to show.
        private static Dictionary<string, object> BuildEventVariables(MSSA_Event evt, string baseUrl, int moduleId)
        {
            var location = string.Join(", ", new[] { evt.TrialSite, evt.City, evt.StateCode }
                .Where(part => !string.IsNullOrWhiteSpace(part)));

            var flyerUrl = string.IsNullOrWhiteSpace(evt.FlyerFileName)
                ? ""
                : $"{baseUrl}/api/MSSA_Event/{evt.EventId}/flyer?moduleid={moduleId}";

            // One line per Class/Stock/Venue offering, e.g. "Open - Sheep - Arena: 3 runs".
            // An empty list here renders as falsy in Handlebars, so the template's
            // {{#if class_offerings}} can show a fallback line when nothing's planned yet.
            var classOfferings = (evt.Offerings ?? new List<MSSA_EventClassOffering>())
                .Select(o => $"{o.ClassName}{(string.IsNullOrWhiteSpace(o.SubClassName) ? "" : $" ({o.SubClassName})")} - {o.Stock} - {o.Venue}: {o.PlannedRuns} run{(o.PlannedRuns == 1 ? "" : "s")}")
                .ToList();

            return new Dictionary<string, object>
            {
                { "event_name", evt.EventName ?? "" },
                { "date_range", evt.DateRange ?? "" },
                { "location", location },
                { "class_offerings", classOfferings },
                { "notes", evt.Notes ?? "" },
                { "chairman_name", evt.ChairmanName ?? "" },
                { "chairman_phone", evt.ChairmanPhone ?? "" },
                { "chairman_email", evt.ChairmanEmail ?? "" },
                { "entry_link", evt.EntryLink ?? "" },
                { "flyer_url", flyerUrl }
            };
        }
    }
}
