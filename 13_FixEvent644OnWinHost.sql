--Cleanup for the EnsureTrialsForOfferingsAsync / SaveEvent() bugs found while
--investigating a "missing" trial (Event 644, BOTR 2026):
--
--1. Editing an existing Event through Events/Edit.razor unconditionally regenerates
--   EventIdentifier and PointYear from EventName/StartDate (SaveEvent(), lines
--   539-547) - fine for a brand-new event, destructive for an existing migrated one
--   with a stable, curated identifier. Overwrote "BOTR 2026" with the auto-generated
--   "2026BattleOnTheRedCDT2026" and PointYear 27 -> 2027.
--2. EnsureTrialsForOfferingsAsync (MSSA_EventRepository.cs:165) counts existing
--   trials by Stock+Venue to decide how many new trial stubs to generate up to
--   PlannedRuns. Every migrated Trial has Venue = NULL (never populated by any
--   migration script), so the count always comes back 0 for historical events -
--   generated 100 junk trial stubs (matching the Cattle/Arena offering's PlannedRuns,
--   which the earlier Z-trial fix had just recomputed to its true, now-uncapped
--   total) the moment the event was saved.
--
--Both are fixed in code (Client/Modules/.../MSSA_Events/Edit.razor and
--Server/Repository/MSSA_EventRepository.cs) so this shouldn't recur, but the
--damage already done to WinHost needs cleaning up directly. Run connected directly
--to WinHost.

DELETE FROM MSSA_Trials WHERE TrialIdentifier LIKE '2026BattleOnTheRedCDT2026-Cattle-Arena-T%';
SELECT @@ROWCOUNT as junk_trials_deleted;

UPDATE MSSA_Events
SET EventIdentifier = 'BOTR 2026', PointYear = 27
WHERE EventId = 644;
SELECT @@ROWCOUNT as event_restored;

SELECT EventId, EventIdentifier, EventName, PointYear FROM MSSA_Events WHERE EventId = 644;
