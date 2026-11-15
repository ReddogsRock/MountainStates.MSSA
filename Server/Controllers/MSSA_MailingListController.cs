using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using MountainStates.MSSA.Module.MSSA_Handlers.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_MailingListController : ModuleControllerBase
    {
        private readonly IMSSA_MailingListManager _manager;

        public MSSA_MailingListController(IMSSA_MailingListManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
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
                var subscribers = await _manager.GetExpiringMembershipSubscribersAsync(moduleId);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("FirstName,LastName,Email");
                foreach (var s in subscribers)
                {
                    sb.AppendLine($"{CsvField(s.FirstName)},{CsvField(s.LastName)},{CsvField(s.Email)}");
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
    }
}
