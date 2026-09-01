using System.Collections.Generic;

namespace MountainStates.MSSA.Module.MSSA_Results.Models
{
    // Result of uploading a scoring sheet (system-generated or the Trial Secretary's
    // own spreadsheet). Warnings cover skipped rows (no match, ambiguous match, empty
    // file) rather than failing the whole import.
    public class ScoreSheetImportResult
    {
        public int RowsProcessed { get; set; }
        public int RowsUpdated { get; set; }
        public int RowsSkipped { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
