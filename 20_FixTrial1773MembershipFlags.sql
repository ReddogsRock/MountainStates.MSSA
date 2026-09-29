-- Trial 1773 (Event 664) was entered by hand via the one-at-a-time "Add Entry" form,
-- which - like the bulk import path before it was fixed (see
-- 19_FixTripleCrownMembershipFlags.sql) - defaults every new entry's
-- HandlerIsMSSAMember to false; staff have to check the box per entry. With 19
-- entries added one at a time and the box left unchecked on all but one, everyone
-- else got 0 points despite correct placings - including one handler (Brian Jacobs)
-- whose first-place dog got 0 points while his own second-place dog got points,
-- because only that second entry had the box checked.
--
-- This backfills HandlerIsMSSAMember for every distinct handler in this trial who
-- currently holds an active membership per MSSA_Memberships (StartYear <= 2026 and
-- EndYear NULL or >= 2026 - same rule as MSSA_Membership.IsCurrentlyActive). All 7
-- distinct handlers in this trial qualify, so this fixes 18 of the 19 entries (the
-- 19th already had the flag set). After running this, re-run "Calculate Placing &
-- Points" for Trial 1773 in the app so points reflect the corrected flags.
--
-- One-time data fix for this trial only - not a recurring migration.

SET QUOTED_IDENTIFIER ON;

UPDATE en
SET en.HandlerIsMSSAMember = 1,
    en.ModifiedDate = SYSUTCDATETIME()
FROM MSSA_Entries en
WHERE en.TrialId = 1773
  AND en.HandlerIsMSSAMember = 0
  AND EXISTS (
      SELECT 1
      FROM MSSA_MembershipHandlers mh
      JOIN MSSA_Memberships m ON m.MembershipId = mh.MembershipId
      WHERE mh.HandlerId = en.HandlerId
        AND m.StartYear <= 2026
        AND (m.EndYear IS NULL OR m.EndYear >= 2026)
  );
