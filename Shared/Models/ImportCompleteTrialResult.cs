using System.Collections.Generic;

namespace MountainStates.MSSA.Module.MSSA_Results.Models
{
    // Result of importing a Trial Secretary's own spreadsheet as a whole trial's
    // worth of new Entries - distinct from ScoreSheetImportResult, which only fills
    // in scores on Entries that already exist. RowsSkippedUnmatched and
    // RowsSkippedExisting are broken out separately (rather than one combined
    // RowsSkipped) since they call for different follow-up: an unmatched row needs
    // the Handler/Dog added first and the file re-uploaded; an existing row needs
    // nothing, it's already there.
    public class ImportCompleteTrialResult
    {
        public int RowsProcessed { get; set; }
        public int RowsCreated { get; set; }
        public int RowsSkippedExisting { get; set; }
        public int RowsSkippedUnmatched { get; set; }
        public List<string> Warnings { get; set; } = new();

        // Only the unmatched rows, in the same columns the import reads plus an Error
        // column explaining why - fix the flagged cells and re-upload the same file;
        // already-created rows are skipped as already existing, so only the fixed
        // rows go through. Null when nothing was unmatched.
        public byte[] ErrorsFile { get; set; }
    }
}
