using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;
using Microsoft.AspNetCore.Http;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_HandlerController : ModuleControllerBase
    {
        private readonly IMSSA_HandlerManager _manager;

        public MSSA_HandlerController(IMSSA_HandlerManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_Handler?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Handler>> Get(int moduleId)
        {
            try
            {
                return await _manager.GetHandlersAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting handlers");
                throw;
            }
        }

        // GET: api/MSSA_Handler/5?moduleid=x
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<MSSA_Handler> Get(int id, int moduleId)
        {
            try
            {
                return await _manager.GetHandlerAsync(id, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting handler {HandlerId}", id);
                throw;
            }
        }

        // GET: api/MSSA_Handler/search?searchTerm=smith&stateCode=CO&moduleId=x
        [HttpGet("search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Handler>> Search(
            string searchTerm = null,
            string stateCode = null,
            string handlerLevel = null,
            int moduleId = -1)
        {
            try
            {
                return await _manager.SearchHandlersAsync(
                    searchTerm,
                    stateCode,
                    handlerLevel,
                    moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching handlers");
                throw;
            }
        }

        // POST: api/MSSA_Handler?moduleid=x
        // Open to everyone (including anonymous visitors) per project decision:
        // there is currently no link between an authenticated person and handler
        // ownership, so gating "add" behind login wouldn't buy real protection -
        // only editing existing records requires authentication (see Put below).
        [HttpPost]
        [AllowAnonymous]
        public async Task<MSSA_Handler> Post([FromBody] MSSA_Handler handler, int moduleId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    handler = await _manager.AddHandlerAsync(handler, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Handler added {Handler}", handler);
                    return handler;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating handler");
                throw;
            }
        }

        // PUT: api/MSSA_Handler/5?moduleid=x
        // Requires the caller to be logged in (any authenticated user, not
        // limited to Admin) since we can't yet verify ownership of the handler record.
        [HttpPut("{id}")]
        [Authorize]
        public async Task<MSSA_Handler> Put(int id, [FromBody] MSSA_Handler handler, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && handler.HandlerId == id)
                {
                    handler = await _manager.UpdateHandlerAsync(handler, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Handler updated {Handler}", handler);
                    return handler;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating handler {HandlerId}", id);
                throw;
            }
        }

        // DELETE: api/MSSA_Handler/5?moduleid=x
        // Left as Admin-only - not part of today's change.
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin))
                {
                    await _manager.DeleteHandlerAsync(id, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Handler deleted {HandlerId}", id);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized handler delete attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting handler {HandlerId}", id);
                throw;
            }
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(RoleNames.Admin);
        }

        // GET: api/MSSA_Handler/5/memberships?moduleid=x
        [HttpGet("{handlerId}/memberships")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<MSSA_Membership>> GetMemberships(int handlerId, int moduleId)
        {
            try
            {
                return await _manager.GetHandlerMembershipsAsync(handlerId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting memberships for handler {HandlerId}", handlerId);
                throw;
            }
        }

        // POST: api/MSSA_Handler/membership?moduleid=x
        // Open to everyone - same reasoning as Handler creation (Post above): there's
        // no link between an authenticated person and handler ownership, and per
        // project decision, someone paying for another handler's membership isn't
        // something to gate against.
        [HttpPost("membership")]
        [AllowAnonymous]
        public async Task<MSSA_Membership> AddMembership([FromBody] MSSA_Membership membership, int moduleId)
        {
            try
            {
                if (!ModelState.IsValid || membership.MemberHandlerIds == null || !membership.MemberHandlerIds.Any())
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var saved = await _manager.AddMembershipAsync(membership, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Membership added {Membership}", saved);
                return saved;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating membership");
                throw;
            }
        }

        // PUT: api/MSSA_Handler/membership/5?moduleid=x
        [HttpPut("membership/{membershipId}")]
        [Authorize]
        public async Task<MSSA_Membership> UpdateMembership(int membershipId, [FromBody] MSSA_Membership membership, int moduleId)
        {
            try
            {
                if (!ModelState.IsValid || membership.MembershipId != membershipId)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var saved = await _manager.UpdateMembershipAsync(membership, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Membership updated {Membership}", saved);
                return saved;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating membership {MembershipId}", membershipId);
                throw;
            }
        }

        // DELETE: api/MSSA_Handler/membership/5?moduleid=x
        [HttpDelete("membership/{membershipId}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task DeleteMembership(int membershipId, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin))
                {
                    await _manager.DeleteMembershipAsync(membershipId, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Membership deleted {MembershipId}", membershipId);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized membership delete attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting membership {MembershipId}", membershipId);
                throw;
            }
        }

        // POST: api/MSSA_Handler/membership/5/member/12?moduleid=x
        // No request body needed - both ids are in the route. Anonymous for the same
        // reason AddMembership is - adding a family member while purchasing happens
        // before there's any authenticated session to require.
        [HttpPost("membership/{membershipId}/member/{handlerId}")]
        [AllowAnonymous]
        public async Task<List<MembershipMemberInfo>> AddMemberToMembership(int membershipId, int handlerId, int moduleId)
        {
            try
            {
                var members = await _manager.AddMemberToMembershipAsync(membershipId, handlerId, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Handler {HandlerId} added to membership {MembershipId}", handlerId, membershipId);
                return members;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error adding handler {HandlerId} to membership {MembershipId}", handlerId, membershipId);
                throw;
            }
        }

        // POST: api/MSSA_Handler/membership/5/member/12/remove?moduleid=x
        // POST rather than DELETE, since this needs to return the updated member list
        // and every DeleteAsync call elsewhere in this app returns no response body.
        [HttpPost("membership/{membershipId}/member/{handlerId}/remove")]
        [AllowAnonymous]
        public async Task<List<MembershipMemberInfo>> RemoveMemberFromMembership(int membershipId, int handlerId, int moduleId)
        {
            try
            {
                var members = await _manager.RemoveMemberFromMembershipAsync(membershipId, handlerId, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Handler {HandlerId} removed from membership {MembershipId}", handlerId, membershipId);
                return members;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error removing handler {HandlerId} from membership {MembershipId}", handlerId, membershipId);
                throw;
            }
        }

        // POST: api/MSSA_Handler/membership/5/checkout?moduleid=x
        // Open to everyone, same reasoning as AddMembership above - creates a Stripe
        // Checkout Session for the membership's price and returns its URL for the
        // client to redirect to. Doesn't touch the membership record itself; only the
        // webhook (StripeWebhookHandler) ever marks it Paid.
        [HttpPost("membership/{membershipId}/checkout")]
        [AllowAnonymous]
        public async Task<MembershipCheckoutResult> CreateMembershipCheckout(int membershipId, [FromBody] CreateMembershipCheckoutDto dto, int moduleId)
        {
            try
            {
                if (dto == null || dto.MembershipId != membershipId
                    || string.IsNullOrEmpty(dto.MembershipType)
                    || string.IsNullOrEmpty(dto.SuccessUrl) || string.IsNullOrEmpty(dto.CancelUrl))
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var checkoutUrl = await _manager.CreateMembershipCheckoutSessionAsync(membershipId, dto.MembershipType, dto.SuccessUrl, dto.CancelUrl, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Membership checkout session created for membership {MembershipId}", membershipId);

                return new MembershipCheckoutResult { CheckoutUrl = checkoutUrl };
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating membership checkout session for membership {MembershipId}", membershipId);
                throw;
            }
        }

        // GET: api/MSSA_Handler/memberships/search?filter=ExpiringThisYear&searchTerm=smith&moduleid=x
        // filter: ExpiringThisYear, Expired, PendingPayment, or omitted for All.
        [HttpGet("memberships/search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<MSSA_Membership>> SearchMemberships(string filter, string searchTerm, int moduleId)
        {
            try
            {
                return await _manager.SearchMembershipsAsync(filter, searchTerm, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching memberships");
                throw;
            }
        }
    }
}