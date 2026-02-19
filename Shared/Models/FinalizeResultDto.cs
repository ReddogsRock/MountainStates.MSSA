namespace MountainStates.MSSA.Module.BackOfficeEntry.Models
{
    /// <summary>
    /// Returned after a Finalize call, summarizing what was calculated.
    /// </summary>
    public class FinalizeResultDto
    {
        public int EntriesFinalized { get; set; }
        public string Message { get; set; }
    }
}
