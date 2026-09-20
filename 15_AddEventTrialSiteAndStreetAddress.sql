--Adds MSSA_Events.TrialSite and MSSA_Events.StreetAddress, both nullable - no data
--backfill needed, and EF Core entity tracking (AddEventAsync/UpdateEventAsync) picks
--these up automatically once the model has them, no repository code changes needed.
--
--Run against both local Oqtane-MSSA and WinHost production (connect directly to
--WinHost - simple ALTER TABLE, no need to go through the linked server).

ALTER TABLE MSSA_Events ADD TrialSite NVARCHAR(255) NULL;
ALTER TABLE MSSA_Events ADD StreetAddress NVARCHAR(255) NULL;
