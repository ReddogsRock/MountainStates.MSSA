using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Repository
{
    public interface IMSSA_MailingListRepository
    {
        // Everyone currently subscribed (IsActive) - what an announcement send goes to.
        Task<List<MSSA_MailingListSubscriber>> GetActiveSubscribersAsync();

        // Admin screen search - matches Email/FirstName/LastName (contains), active and
        // unsubscribed alike, capped at 100 results so a blank/short search doesn't try
        // to render the whole list at once.
        Task<List<MSSA_MailingListSubscriber>> SearchSubscribersAsync(string searchTerm);

        Task<MSSA_MailingListSubscriber> AddSubscriberAsync(MSSA_MailingListSubscriber subscriber);
        Task<MSSA_MailingListSubscriber> UpdateSubscriberAsync(MSSA_MailingListSubscriber subscriber);
        Task DeleteSubscriberAsync(int subscriberId);

        // Public self-service signup. Idempotent by design - an email that's already
        // active just succeeds quietly, and one that previously unsubscribed is
        // reactivated, since this is reachable by anyone without logging in and a
        // visitor re-submitting shouldn't see an error either way.
        Task SubscribeAsync(string email, string firstName, string lastName);
    }
}
