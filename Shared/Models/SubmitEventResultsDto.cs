using System.Collections.Generic;

namespace MountainStates.MSSA.Module.MSSA_Results.Models
{
    // Response from attempting to submit an Event's results for approval. Success is
    // false (and Reasons populated) if some runs still aren't scored - the caller
    // shows those reasons rather than silently doing nothing.
    public class SubmitEventResultsDto
    {
        public bool Success { get; set; }
        public string ResultsApprovalStatus { get; set; }
        public List<string> Reasons { get; set; } = new();
    }
}
