using System;

namespace MountainStates.MSSA.Module.MSSA_YearEndStandings.Models
{
    // One trial result contributing to a standing's TotalPoints - shown on the
    // Details drill-down page.
    public class YearEndStandingDetail
    {
        public string EventName { get; set; }
        public string TrialName { get; set; }
        public DateTime TrialDate { get; set; }
        public string HandlerName { get; set; }
        public string Stock { get; set; }
        public decimal Points { get; set; }

        // The class this entry was run in (Open, Nursery, etc.) - On-foot/Horseback
        // combined under one Level name, same as everywhere else in this module.
        // Mainly useful when the parent standing is "All Levels" (Lifetime), where a
        // single dog's rows can span several different classes.
        public string Level { get; set; }

        // Shown when the parent standing is "Lifetime" (spans multiple years),
        // so each row's year is visible; otherwise redundant with the selected year.
        public int? PointYear { get; set; }
    }
}
