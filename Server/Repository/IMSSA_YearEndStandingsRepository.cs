using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_YearEndStandings.Models;

namespace MountainStates.MSSA.Module.MSSA_YearEndStandings.Repository
{
    public interface IMSSA_YearEndStandingsRepository
    {
        // level: a ClassName (e.g. "Open", "Novice" - On-foot/Horseback combined), or
        // the special value "Futurity" (Nursery entries restricted to nominated dogs).
        // year: null means Lifetime (all years combined).
        Task<List<MSSA_YearEndStanding>> GetStandingsAsync(int? year, string level, string species);

        // handlerId is null for Dog-only classes (see MSSA_YearEndStanding.HandlerId).
        Task<List<YearEndStandingDetail>> GetStandingDetailsAsync(int dogId, int? handlerId, int? year, string level, string species);

        // Distinct Point Years available across all events, normalized to 4-digit
        // form (some legacy events store PointYear as a 2-digit value).
        Task<List<int>> GetAvailableYearsAsync();

        // Distinct level (ClassName) options for the dropdown - On-foot/Horseback
        // combined under one name, Pro-Novice excluded. Does not include "Futurity",
        // which is a UI-only addition since it isn't a real class.
        Task<List<string>> GetLevelsAsync();
    }
}
