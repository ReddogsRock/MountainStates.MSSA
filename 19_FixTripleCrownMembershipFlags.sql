-- Trial 2127 (Triple Crown Cowdog Trial, Event 718, 2026-09-18) was created via the
-- "Import Complete Trial" bulk Excel import, which - like the manual Add Entry form -
-- defaults every new entry's HandlerIsMSSAMember to false, since membership status
-- isn't inferable from the spreadsheet. With 77 entries imported at once and nobody
-- to check the box afterward, every entry stayed non-member, so CalculatePlacingAndPoints
-- (which only awards points to member entries) gave everyone 0 points despite correct
-- placings.
--
-- This backfills HandlerIsMSSAMember for the 26 (of 30) distinct handlers in this trial
-- who currently hold an active membership per MSSA_Memberships (StartYear <= 2026 and
-- EndYear NULL or >= 2026 - same rule as MSSA_Membership.IsCurrentlyActive). After
-- running this, re-run "Calculate Placing & Points" for Trial 2127 in the app so points
-- reflect the corrected flags.
--
-- One-time data fix for this trial only - not a recurring migration. The underlying gap
-- (bulk import not looking up membership) is fixed in ImportCompleteTrialAsync itself.

SET QUOTED_IDENTIFIER ON;

UPDATE en
SET en.HandlerIsMSSAMember = 1,
    en.ModifiedDate = SYSUTCDATETIME()
FROM MSSA_Entries en
WHERE en.TrialId = 2127
  AND en.HandlerIsMSSAMember = 0
  AND EXISTS (
      SELECT 1
      FROM MSSA_MembershipHandlers mh
      JOIN MSSA_Memberships m ON m.MembershipId = mh.MembershipId
      WHERE mh.HandlerId = en.HandlerId
        AND m.StartYear <= 2026
        AND (m.EndYear IS NULL OR m.EndYear >= 2026)
  );
