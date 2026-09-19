--Same SaveEvent() identifier/PointYear regeneration bug as Event 644
--(13_FixEvent644OnWinHost.sql), found via a full compare of every WinHost Event
--against local Oqtane-MSSA (never touched by live edits). This one was corrupted
--on 2026-09-17 - predates the PlannedRuns cap removal, confirming the identifier
--bug has been live independently of that. No junk trial stubs were generated for
--this event (confirmed via a scan across all trials), so only the Event row needs
--restoring. Run connected directly to WinHost.

UPDATE MSSA_Events
SET EventIdentifier = 'BD 2026', PointYear = 27
WHERE EventId = 664;
SELECT @@ROWCOUNT as event_restored;

SELECT EventId, EventIdentifier, EventName, PointYear FROM MSSA_Events WHERE EventId = 664;
