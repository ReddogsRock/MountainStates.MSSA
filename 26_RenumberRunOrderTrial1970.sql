-- Fixes Trial 1970's (Event 683, Cattle, 2026-10-24) run order, which was corrupted by
-- a bug in the run-order preview's up/down reorder arrows: MoveProposalRow renumbered
-- the whole trial 1..N in one continuous pass instead of restarting at 1 for each
-- class (fixed separately in Client/Modules/.../MSSA_Entries/Index.razor). The trial
-- hasn't run yet (confirmed with Janet 2026-10-24), so it's safe to renumber in place -
-- this preserves each class's existing relative order, it just restarts the count at 1
-- for Nursery/Intermediate/Novice/Jr Handler instead of continuing from the class
-- before it. Does NOT reshuffle who runs in what order within a class.
--
-- Run against WinHost production. Safe to re-run (idempotent - renumbering an
-- already-correct sequence produces the same result).

SET NOCOUNT ON;

-- Preview before/after - review this before running the UPDATE below.
SELECT EntryId, ClassId, RunOrder AS OldRunOrder,
       ROW_NUMBER() OVER (PARTITION BY ClassId ORDER BY RunOrder) AS NewRunOrder
FROM MSSA_Entries
WHERE TrialId = 1970
ORDER BY ClassId, RunOrder;

;WITH Renumbered AS (
    SELECT EntryId,
           ROW_NUMBER() OVER (PARTITION BY ClassId ORDER BY RunOrder) AS NewRunOrder
    FROM MSSA_Entries
    WHERE TrialId = 1970
)
UPDATE e
SET e.RunOrder = r.NewRunOrder,
    e.ModifiedDate = SYSUTCDATETIME()
FROM MSSA_Entries e
JOIN Renumbered r ON e.EntryId = r.EntryId;

-- Verify
SELECT e.EntryId, e.ClassId, c.ClassName, e.RunOrder
FROM MSSA_Entries e
JOIN MSSA_Classes c ON e.ClassId = c.ClassId
WHERE e.TrialId = 1970
ORDER BY e.ClassId, e.RunOrder;
