-- The original Access migration (03_InsertFromOriginal_v2.sql) hard-coded every
-- migrated event's ResultsApprovalStatus to 'Approved', regardless of whether it
-- actually had any results - the legacy system had no such workflow, so this was a
-- blanket "treat all history as final" default rather than anything derived from real
-- submission state.
--
-- 9 of the 648 currently-Approved events have zero scored entries anywhere across
-- their trials (no EnteredTotalScore and no obstacle scores on any entry in any trial
-- for the event) - 5 are empty placeholder rows from the migration (no dates, no
-- trials at all: EventId 16, 19, 21, 39, 40), and 4 have real dates and trials but no
-- entries were ever recorded (EventId 49, 146, 183, 193). None of these have any
-- actual results to have been "approved" in the first place.
--
-- This resets those 9 back to NotSubmitted (and clears the submitted/approved
-- metadata that was likewise backfilled to the migration date) so they show up again
-- in the Results Entry dropdown, in case paper results for the 4 real ones ever
-- surface and someone wants to enter them.
--
-- One-time data fix - not a recurring migration.

SET QUOTED_IDENTIFIER ON;

UPDATE e
SET e.ResultsApprovalStatus = 'NotSubmitted',
    e.ResultsSubmittedDate = NULL,
    e.ResultsSubmittedByUserId = NULL,
    e.ResultsApprovedDate = NULL,
    e.ResultsApprovedByUserId = NULL
FROM MSSA_Events e
WHERE e.ResultsApprovalStatus = 'Approved'
  AND NOT EXISTS (
      SELECT 1
      FROM MSSA_Entries en
      JOIN MSSA_Trials t ON t.TrialId = en.TrialId
      WHERE t.EventId = e.EventId
        AND (en.EnteredTotalScore IS NOT NULL
             OR en.ObstacleScore1 IS NOT NULL OR en.ObstacleScore2 IS NOT NULL OR en.ObstacleScore3 IS NOT NULL
             OR en.ObstacleScore4 IS NOT NULL OR en.ObstacleScore5 IS NOT NULL OR en.ObstacleScore6 IS NOT NULL
             OR en.ObstacleScore7 IS NOT NULL OR en.ObstacleScore8 IS NOT NULL OR en.ObstacleScore9 IS NOT NULL)
  );
