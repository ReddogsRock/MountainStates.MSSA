-- Like ResultsApprovalStatus, ResultsReceivedDate was never actually populated for
-- most events - it's a manual admin field, not something the scoring workflow sets.
-- 620 of the 648 events on the Outstanding Report (see OutstandingReport.razor) are
-- past events where every entry in every trial already has a Placing - i.e. results
-- were clearly received and fully processed - just never marked as such.
--
-- This marks all of them received as of today, rather than clicking "Mark Received"
-- 620 times in the report one at a time. Leaves FeeReceivedDate untouched - that's
-- a separate, unrelated condition on the same report.
--
-- One-time data fix - not a recurring migration.

SET QUOTED_IDENTIFIER ON;

UPDATE e
SET e.ResultsReceivedDate = CAST(GETDATE() AS DATE)
FROM MSSA_Events e
WHERE e.IsActive = 1
  AND e.StartDate <= CAST(GETDATE() AS DATE)
  AND e.ResultsReceivedDate IS NULL
  AND EXISTS (
      SELECT 1
      FROM MSSA_Entries en
      JOIN MSSA_Trials t ON t.TrialId = en.TrialId
      WHERE t.EventId = e.EventId
  )
  AND NOT EXISTS (
      SELECT 1
      FROM MSSA_Entries en
      JOIN MSSA_Trials t ON t.TrialId = en.TrialId
      WHERE t.EventId = e.EventId AND en.Placing IS NULL
  );
