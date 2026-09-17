--Found while spot-checking trial results after the final import: 27 MSSA_Dogs.Name
--values had a legacy trailing "+" baked in from the old Access system's manual
--Futurity-marking convention (e.g. "AC Eden +", "Pedro+"). Now that
--MSSA_DogFuturityParticipation is correctly populated, the app's own
--ApplyFuturityMarkerAsync (MSSA_EntryRepository.cs, MSSA_EventRepository.cs, etc.)
--also appends "+" for any entry in a Futurity year - stacking into "++" in trial
--results wherever both applied.
--
--All 27 only had the "+" at the very end (some with a trailing space before it, some
--without), never mid-name, so a trailing-only strip is safe. Run against both local
--Oqtane-MSSA and WinHost production (WinHost already had these names from the
--Import/Export Wizard copy).

--============================================================
--Local Oqtane-MSSA
--============================================================

UPDATE [Oqtane-MSSA].dbo.MSSA_Dogs
SET Name = RTRIM(LEFT(RTRIM(Name), LEN(RTRIM(Name))-1))
WHERE Name LIKE '%+%' AND RIGHT(RTRIM(Name),1) = '+';

--============================================================
--WinHost production - run connected directly to WinHost, not through a linked
--server (unnecessary for a plain UPDATE with no join).
--============================================================

UPDATE MSSA_Dogs
SET Name = RTRIM(LEFT(RTRIM(Name), LEN(RTRIM(Name))-1))
WHERE Name LIKE '%+%' AND RIGHT(RTRIM(Name),1) = '+';
