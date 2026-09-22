--EnsureTrialsForOfferingsAsync (MSSA_EventRepository.cs) auto-generates trial stubs
--to "catch up" to an offering's PlannedRuns whenever an event's Offerings get saved.
--For an already-run event, PlannedRuns holds the historical entry count (not a
--trial-session count), so this manufactures junk trials on every save - already
--fixed in code (StartDate-in-the-past guard), but ~53 events on WinHost already had
--junk-pattern trials (TrialIdentifier like '...-Stock-Venue-Tn') by the time that
--fix was found, from before it was deployed.
--
--Naming-pattern alone isn't enough to tell bug damage from a legitimately-planned
--future event (the feature produces the exact same naming for both) - classified by
--comparing each event's StartDate against when its matching trials were created
--(see 2026-09-22 conversation). Of 53 events / ~153 junk-pattern trials, only these
--16 events / 46 trials had a StartDate already in the past at generation time -
--confirmed bug damage, zero entries attached. The other 37 events' trials are real
--future-event planning and were left alone.

SET NOCOUNT ON;

DELETE FROM MSSA_Trials
WHERE EventId IN (707,717,660,708,709,710,711,712,713,714,715,716,668,669,718,670)
  AND (TrialIdentifier LIKE '%-Cattle-Arena-T%'
    OR TrialIdentifier LIKE '%-Cattle-Field-T%'
    OR TrialIdentifier LIKE '%-Sheep-Arena-T%'
    OR TrialIdentifier LIKE '%-Sheep-Field-T%')
  AND NOT EXISTS (SELECT 1 FROM MSSA_Entries e WHERE e.TrialId = MSSA_Trials.TrialId);

SELECT @@ROWCOUNT as rows_deleted;
