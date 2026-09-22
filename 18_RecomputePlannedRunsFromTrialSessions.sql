--PlannedRuns on MSSA_EventClassOfferings means "how many separate trial sessions a
--contestant can enter that class" (per Janet) - it's shown to contestants so they
--know how many chances they have to run. The original migration scripts populated
--it as COUNT(*) of Entries per Event+Class+Stock instead, which conflates total
--entries (many contestants each running once) with distinct trial sessions - wildly
--inflating the number for popular classes (e.g. 20 entries in one session showed as
--PlannedRuns=20, not 1). This is also what made EnsureTrialsForOfferingsAsync
--manufacture junk trials trying to "catch up" to that inflated number whenever an
--already-run event's offerings got saved (see project_event_edit_corruption_bug
--memory) - the StartDate guard fix stops the symptom, but the underlying data was
--simply wrong.
--
--Recomputes PlannedRuns as COUNT(DISTINCT TrialId) instead, scoped to only events
--whose StartDate has already passed - never touches an upcoming event's PlannedRuns,
--since that's a real planning number a Trial Secretary deliberately typed in, not a
--migration artifact.
--
--Run against both local Oqtane-MSSA and WinHost (connect directly to WinHost).
--Note: this only updates existing MSSA_EventClassOfferings rows matched by
--EventId+ClassId+Stock - if a Trial's Stock gets corrected such that no existing
--offering row matches its new Stock (e.g. Cattle -> Sheep with no Sheep offering
--yet), that offering needs to be added/corrected by hand first (through the Edit
--Event page, or directly) before re-running this to pick it up.

;WITH Recomputed AS (
    SELECT t.EventId, e.ClassId, ISNULL(t.Stock, 'Cattle') as Stock,
           COUNT(DISTINCT e.TrialId) as TrialSessionCount
    FROM MSSA_Entries e
    JOIN MSSA_Trials t ON e.TrialId = t.TrialId
    JOIN MSSA_Events ev ON ev.EventId = t.EventId
    WHERE ev.StartDate IS NOT NULL AND ev.StartDate < CAST(GETDATE() AS DATE)
    GROUP BY t.EventId, e.ClassId, t.Stock
)
UPDATE o
SET o.PlannedRuns = r.TrialSessionCount
FROM MSSA_EventClassOfferings o
JOIN Recomputed r ON r.EventId = o.EventId AND r.ClassId = o.ClassId AND r.Stock = o.Stock
WHERE o.PlannedRuns <> r.TrialSessionCount;

SELECT @@ROWCOUNT as offerings_corrected;
