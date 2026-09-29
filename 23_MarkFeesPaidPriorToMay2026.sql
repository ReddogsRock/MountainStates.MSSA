-- FeeReceivedDate is only ever set automatically for fees paid through the newer
-- Stripe sanctioning-fee checkout - anything paid the old way (by check, before that
-- flow existed) still shows as unpaid on the Outstanding Report even though the money
-- was actually received. Admin confirmed everything before 2026-05-01 has been paid.
--
-- Marks FeeReceivedDate for every active event that started before 2026-05-01 and
-- doesn't already have one. Only clears the "not marked" state - doesn't set or
-- change SanctionFee amounts. Leaves ResultsReceivedDate untouched (unrelated
-- condition on the same report; see 22_MarkFullyPlacedPastEventsAsResultsReceived.sql
-- for that one).
--
-- One-time data fix - not a recurring migration.

SET QUOTED_IDENTIFIER ON;

UPDATE e
SET e.FeeReceivedDate = CAST(GETDATE() AS DATE)
FROM MSSA_Events e
WHERE e.IsActive = 1
  AND e.FeeReceivedDate IS NULL
  AND e.StartDate < '2026-05-01';
