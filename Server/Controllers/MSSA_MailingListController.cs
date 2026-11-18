using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Shared;
using MountainStates.MSSA.Module.MSSA_Events.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_MailingListController : ModuleControllerBase
    {
        private readonly IMSSA_MailingListManager _manager;
        private readonly IMailgunService _mailgunService;
        private readonly IUserRoleRepository _userRoleRepository;

        public MSSA_MailingListController(IMSSA_MailingListManager manager, IMailgunService mailgunService, IUserRoleRepository userRoleRepository, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
            _mailgunService = mailgunService;
            _userRoleRepository = userRoleRepository;
        }

        // GET: api/MSSA_MailingList/search?searchterm=x&moduleid=x
        [HttpGet("search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<MSSA_MailingListSubscriber>> Search(string searchTerm, bool expiringOnly, int moduleId)
        {
            try
            {
                return await _manager.SearchSubscribersAsync(searchTerm, expiringOnly, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching mailing list subscribers");
                throw;
            }
        }

        // POST: api/MSSA_MailingList?moduleid=x
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_MailingListSubscriber> Post([FromBody] MSSA_MailingListSubscriber subscriber, int moduleId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var added = await _manager.AddSubscriberAsync(subscriber, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Mailing list subscriber added {Subscriber}", added);
                return added;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error adding mailing list subscriber");
                throw;
            }
        }

        // PUT: api/MSSA_MailingList/5?moduleid=x
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_MailingListSubscriber> Put(int id, [FromBody] MSSA_MailingListSubscriber subscriber, int moduleId)
        {
            try
            {
                if (!ModelState.IsValid || id != subscriber.SubscriberId)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var updated = await _manager.UpdateSubscriberAsync(subscriber, moduleId);
                if (updated == null)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    return null;
                }

                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Mailing list subscriber updated {Subscriber}", updated);
                return updated;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating mailing list subscriber {SubscriberId}", id);
                throw;
            }
        }

        // POST: api/MSSA_MailingList/subscribe?moduleid=x
        // Public self-service signup - no login required, reachable from any page the
        // Subscribe module is placed on. AllowAnonymous deliberately overrides any
        // module-level permission policy, since a visitor signing themselves up isn't
        // the same thing as having edit access to the module.
        [HttpPost("subscribe")]
        [AllowAnonymous]
        public async Task<bool> Subscribe([FromBody] SubscribeMailingListDto dto, int moduleId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return false;
                }

                // Honeypot - a real visitor never sees or fills this field. Report
                // success anyway rather than revealing that a bot got caught.
                if (!string.IsNullOrEmpty(dto.Website))
                {
                    return true;
                }

                await _manager.SubscribeAsync(dto, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Mailing list self-service signup for {Email}", dto.Email);
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error processing mailing list signup");
                throw;
            }
        }

        // POST: api/MSSA_MailingList/sync-expiring?moduleid=x
        // Admin-triggered annual refresh - not automatic. Re-run ahead of each renewal
        // notice (including a follow-up a couple weeks later) so anyone who's since
        // renewed drops out of the segment on its own.
        [HttpPost("sync-expiring")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<int> SyncExpiring(int moduleId)
        {
            try
            {
                var count = await _manager.SyncExpiringMembershipSegmentAsync(moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Expiring-membership mailing list segment synced: {Count} flagged", count);
                return count;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error syncing expiring-membership segment");
                throw;
            }
        }

        // GET: api/MSSA_MailingList/export-expiring?moduleid=x
        [HttpGet("export-expiring")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> ExportExpiring(int moduleId)
        {
            try
            {
                var rows = await _manager.GetExpiringMembershipExportRowsAsync(moduleId);

                // Renewal happens on the person's own Handler Detail page - only the
                // HandlerId varies per recipient.
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("FirstName,LastName,Email,RenewalLink");
                foreach (var r in rows)
                {
                    var renewalLink = r.HandlerId.HasValue
                        ? $"https://mssadogs.com/memberservices/handlers/*/44/Detail?id={r.HandlerId.Value}"
                        : "";
                    sb.AppendLine($"{CsvField(r.FirstName)},{CsvField(r.LastName)},{CsvField(r.Email)},{CsvField(renewalLink)}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return File(bytes, "text/csv", "expiring-members.csv");
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error exporting expiring-membership segment");
                throw;
            }
        }

        // Quotes a field only when it actually needs it (contains a comma, quote, or
        // newline) - keeps the common case readable while still round-tripping safely.
        private static string CsvField(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }

        // DELETE: api/MSSA_MailingList/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                await _manager.DeleteSubscriberAsync(id, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Mailing list subscriber deleted {SubscriberId}", id);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting mailing list subscriber {SubscriberId}", id);
                throw;
            }
        }

        // ============ Named Lists (Send a Campaign audiences) ============

        // GET: api/MSSA_MailingList/named-lists?moduleid=x
        [HttpGet("named-lists")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<List<MSSA_MailingListNamedList>> GetNamedLists(int moduleId)
        {
            try
            {
                return await _manager.GetNamedListsAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting mailing list named lists");
                throw;
            }
        }

        // GET: api/MSSA_MailingList/named-lists/5/members?moduleid=x
        [HttpGet("named-lists/{id}/members")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<List<MSSA_MailingListSubscriber>> GetNamedListMembers(int id, int moduleId)
        {
            try
            {
                return await _manager.GetNamedListMembersAsync(id, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting members for named list {NamedListId}", id);
                throw;
            }
        }

        // POST: api/MSSA_MailingList/named-lists?moduleid=x
        [HttpPost("named-lists")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_MailingListNamedList> AddNamedList([FromBody] MSSA_MailingListNamedList namedList, int moduleId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var added = await _manager.AddNamedListAsync(namedList.Name, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Mailing list named list added {NamedList}", added);
                return added;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error adding mailing list named list");
                throw;
            }
        }

        // PUT: api/MSSA_MailingList/named-lists/5?moduleid=x
        [HttpPut("named-lists/{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_MailingListNamedList> RenameNamedList(int id, [FromBody] MSSA_MailingListNamedList namedList, int moduleId)
        {
            try
            {
                if (!ModelState.IsValid || id != namedList.NamedListId)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var updated = await _manager.RenameNamedListAsync(id, namedList.Name, moduleId);
                if (updated == null)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    return null;
                }

                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Mailing list named list renamed {NamedListId}", id);
                return updated;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error renaming mailing list named list {NamedListId}", id);
                throw;
            }
        }

        // DELETE: api/MSSA_MailingList/named-lists/5?moduleid=x
        [HttpDelete("named-lists/{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task DeleteNamedList(int id, int moduleId)
        {
            try
            {
                await _manager.DeleteNamedListAsync(id, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Mailing list named list deleted {NamedListId}", id);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting mailing list named list {NamedListId}", id);
                throw;
            }
        }

        // PUT: api/MSSA_MailingList/named-lists/5/members?moduleid=x
        [HttpPut("named-lists/{id}/members")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task SetNamedListMembers(int id, [FromBody] List<int> subscriberIds, int moduleId)
        {
            try
            {
                await _manager.SetNamedListMembersAsync(id, subscriberIds ?? new List<int>(), moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Mailing list named list {NamedListId} membership set to {Count} subscribers", id, subscriberIds?.Count ?? 0);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error setting membership for named list {NamedListId}", id);
                throw;
            }
        }

        // POST: api/MSSA_MailingList/named-lists/5/sync-from-role?moduleid=x
        // Repopulates a named list to exactly match everyone currently holding the
        // given Oqtane role (e.g. "Trial Secretary") - re-running this later picks up
        // role changes (added/removed) since the last sync. Existing subscriber rows
        // are matched by email and never have IsActive flipped, so a role-holder who's
        // unsubscribed stays unsubscribed.
        [HttpPost("named-lists/{id}/sync-from-role")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<int> SyncNamedListFromRole(int id, [FromBody] SyncNamedListFromRoleDto dto, int moduleId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return 0;
                }

                var people = _userRoleRepository.GetUserRoles(dto.SiteId)
                    .Where(ur => ur.Role.Name == dto.RoleName && !string.IsNullOrWhiteSpace(ur.User.Email))
                    .GroupBy(ur => ur.UserId)
                    .Select(g => g.First())
                    .Select(ur => (Email: ur.User.Email, DisplayName: ur.User.DisplayName))
                    .ToList();

                var count = await _manager.SyncNamedListFromRoleAsync(id, people, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Mailing list named list {NamedListId} synced from role {RoleName}: {Count} people", id, dto.RoleName, count);
                return count;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error syncing named list {NamedListId} from role {RoleName}", id, dto.RoleName);
                throw;
            }
        }

        // ============ Email Templates (saved Send a Campaign content) ============

        // GET: api/MSSA_MailingList/email-templates?moduleid=x
        [HttpGet("email-templates")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<List<MSSA_MailingListEmailTemplate>> GetEmailTemplates(int moduleId)
        {
            try
            {
                return await _manager.GetEmailTemplatesAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting mailing list email templates");
                throw;
            }
        }

        // GET: api/MSSA_MailingList/email-templates/5?moduleid=x
        [HttpGet("email-templates/{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_MailingListEmailTemplate> GetEmailTemplate(int id, int moduleId)
        {
            try
            {
                return await _manager.GetEmailTemplateAsync(id, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting mailing list email template {TemplateId}", id);
                throw;
            }
        }

        // POST: api/MSSA_MailingList/email-templates?moduleid=x
        [HttpPost("email-templates")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_MailingListEmailTemplate> AddEmailTemplate([FromBody] MSSA_MailingListEmailTemplate template, int moduleId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var added = await _manager.AddEmailTemplateAsync(template, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Mailing list email template added {Template}", added);
                return added;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error adding mailing list email template");
                throw;
            }
        }

        // PUT: api/MSSA_MailingList/email-templates/5?moduleid=x
        [HttpPut("email-templates/{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_MailingListEmailTemplate> UpdateEmailTemplate(int id, [FromBody] MSSA_MailingListEmailTemplate template, int moduleId)
        {
            try
            {
                if (!ModelState.IsValid || id != template.TemplateId)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var updated = await _manager.UpdateEmailTemplateAsync(template, moduleId);
                if (updated == null)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    return null;
                }

                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Mailing list email template updated {TemplateId}", id);
                return updated;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating mailing list email template {TemplateId}", id);
                throw;
            }
        }

        // DELETE: api/MSSA_MailingList/email-templates/5?moduleid=x
        [HttpDelete("email-templates/{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task DeleteEmailTemplate(int id, int moduleId)
        {
            try
            {
                await _manager.DeleteEmailTemplateAsync(id, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Mailing list email template deleted {TemplateId}", id);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting mailing list email template {TemplateId}", id);
                throw;
            }
        }

        // ============ Send a Campaign ============

        // POST: api/MSSA_MailingList/campaign/recipient-count?moduleid=x
        [HttpPost("campaign/recipient-count")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<int> GetCampaignRecipientCount([FromBody] CampaignAudienceDto dto, int moduleId)
        {
            try
            {
                var recipients = await _manager.GetCampaignRecipientsAsync(dto.Audience, dto.NamedListId, moduleId);
                return recipients.Count;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting campaign recipient count");
                throw;
            }
        }

        // POST: api/MSSA_MailingList/campaign/send?moduleid=x
        [HttpPost("campaign/send")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<int> SendCampaign([FromBody] SendCampaignDto dto, int moduleId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return 0;
                }

                var recipients = await _manager.GetCampaignRecipientsAsync(dto.Audience, dto.NamedListId, moduleId);
                var sentCount = await _mailgunService.SendCampaignAsync(dto.Subject, dto.BodyHtml, recipients, moduleId);

                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Mailing list campaign sent to {Count} recipients ({Audience})", sentCount, dto.Audience);
                return sentCount;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error sending mailing list campaign");
                throw;
            }
        }
    }
}
