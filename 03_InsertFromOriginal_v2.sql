--Updated from 03_InsertFromOriginal.sql to match the current Oqtane-MSSA schema and
--what we've since learned about the source data. Targets the scratch database
--Oqtane-MSSA-Merge (schema-only copy of the live tables) from the fresh Access
--import sitting in MSSA-2026. Changes from the original, by section:
--  - Dogs: unchanged, still matches current schema.
--  - Events: removed the stray "truncate table event" (not a real table here), and
--    historical events are now inserted as already Approved/Approved rather than
--    letting them default to Pending/NotSubmitted - those defaults are meant for new
--    events going through the live workflow, not backfilled history.
--  - Trials: unchanged, still matches current schema.
--  - Entries: Time/TieBreakerTime conversion now reinterprets the stored hour as
--    minutes and stored minute as seconds, correcting the Access short-time-entry
--    ambiguity we found (a value entered as "5:16" meaning 5 min 16 sec got stored as
--    5:16 AM). Applied to both Time and TieBreakerTime, since both were almost
--    certainly entered the same way - worth a spot check after import either way.
--  - Replaced the dead boolean-flag section (Cattle/Open/Nursery/etc. on MSSA_Events -
--    those columns still exist but nothing reads them anymore) with real inserts into
--    MSSA_EventClassOfferings, which is what the app actually uses today.
--    PlannedRuns is set to the actual count of entries per Event+Class, so nothing
--    gets truncated when Entries/Edit.razor caps trial-matching at PlannedRuns.
--    Venue has no source in the original Access data - defaulted to 'Arena' (confirmed
--    acceptable for historical data).

USE [Oqtane-MSSA-Merge];

--Dogs

SET IDENTITY_INSERT [Oqtane-MSSA-Merge].dbo.MSSA_dogs ON

insert into [Oqtane-MSSA-Merge].dbo.MSSA_dogs (dogid, name, breed, DateOfBirth, RegistrationNumber, FirstCompetitionYear,
	OwnerName, OwnerIsMSSAMember, isdeceased, IsSold, CreatedDate, ModifiedDate, IsActive)
select Next_ID, Dog_Name, Breed, cast(dob as date), Reg_Breed_Num, ISNULL(try_convert(int, firstyear), 0), Owner, Owner_MSSA, Dog_Deceased, Sold,
	GETDATE(), GETDATE(), 1
from [MSSA-2026].dbo.Dogs

SET IDENTITY_INSERT [Oqtane-MSSA-Merge].dbo.MSSA_dogs OFF

select * from MSSA_dogs

--Events

SET IDENTITY_INSERT [Oqtane-MSSA-Merge].dbo.MSSA_events ON
insert into [Oqtane-MSSA-Merge].dbo.MSSA_Events([EventID],[EventIdentifier],[EventName],[City],[StateCode],
	[PointYear],[ChairmanName],[ChairmanPhone],[IsMSSASanctioned],[CreatedDate],[ResultsReceivedDate],[ResultsUploaded],
	[SanctionFee],[FeeReceivedDate],
	[ResultsApprovalStatus],[ResultsApprovedDate],
	[ApprovalStatus],[ApprovedDate]
	)
select event_Id, Event_Identifer, Event_Name, City,
	-- StateCode is only 2 characters - resolve full state/province names (e.g.
	-- "Saskatchewan") through the app's own MSSA_States lookup rather than truncating.
	CASE
		WHEN LEN(LTRIM(RTRIM(State))) <= 2 THEN State
		ELSE (SELECT TOP 1 StateCode FROM [Oqtane-MSSA-Merge].dbo.MSSA_States WHERE StateName = LTRIM(RTRIM(State)))
	END,
	ISNULL(try_convert(int, Pt_Year), 0), Chairman, Phone, MSSA_Sanctioned,
	isnull([Event Added], '1/1/1900'), [Results Rec'd], [Results Uploaed], [Sanction Fees], [Fee Rec'd],
	'Approved', GETDATE(),
	'Approved', GETDATE()
from [MSSA-2026].dbo.Events
SET IDENTITY_INSERT [Oqtane-MSSA-Merge].dbo.MSSA_events OFF

select * from MSSA_Events

select count(*) from [MSSA-2026].dbo.Events

--Handler

SET IDENTITY_INSERT [Oqtane-MSSA-Merge].dbo.MSSA_Handlers ON
insert into [Oqtane-MSSA-Merge].dbo.MSSA_Handlers ([HandlerId],[LastName], [FirstName], [Email], Phone, AlternatePhone,
	Address, City, StateCode, HandlerLevel, LevelMoveUpDate, PhotoReleaseConsent)
select ID, isnull([Last Name], ''), isnull([First Name],''), [E-mail], Phone, Other_Phone, Address, City,
	-- StateCode is only 2 characters - resolve full state/province names (e.g.
	-- "Saskatchewan") through the app's own MSSA_States lookup rather than truncating.
	CASE
		WHEN LEN(LTRIM(RTRIM(State))) <= 2 THEN State
		ELSE (SELECT TOP 1 StateCode FROM [Oqtane-MSSA-Merge].dbo.MSSA_States WHERE StateName = LTRIM(RTRIM(State)))
	END,
	Handler_Level,
	[Move Up Date],
	case
		when [Photo Release] like '%y%' then 1
		when [Photo Release] like '%f%' then 0
		else 0
		end as release
from [MSSA-2026].dbo.Handlers
order by id
SET IDENTITY_INSERT [Oqtane-MSSA-Merge].dbo.MSSA_Handlers OFF

select * from MSSA_handlers

--Trials

SET IDENTITY_INSERT [Oqtane-MSSA-Merge].dbo.MSSA_trials ON

INSERT INTO [Oqtane-MSSA-Merge].dbo.MSSA_trials
    (trialid, eventid, TrialIdentifier, trialdate, TrialName, Stock)
SELECT
    c.trial_id,
    a.eventid,
    c.Trial_Identifier,
    c.Trial_Date,
    c.Trial_Name,
    COALESCE(c.Species, e.Species) as Species
FROM [Oqtane-MSSA-Merge].dbo.MSSA_events a
JOIN [MSSA-2026].dbo.Events b
    ON a.EventIdentifier = b.Event_Identifer
JOIN [MSSA-2026].dbo.Trials c
    ON b.Event_Identifer = c.Event_Identifer
LEFT JOIN (
    SELECT Trial_Identifier, MIN(Species) as Species
    FROM [MSSA-2026].dbo.Entries
    WHERE Species IS NOT NULL
    GROUP BY Trial_Identifier
) e ON c.Trial_Identifier = e.Trial_Identifier
ORDER BY a.eventid, c.Trial_Identifier;

SET IDENTITY_INSERT [Oqtane-MSSA-Merge].dbo.MSSA_trials OFF

select count(*) from [MSSA-2026].dbo.Trials

--insert startdate and enddate into events table
update [Oqtane-MSSA-Merge].dbo.MSSA_events set startdate = (select min(trialdate) from [Oqtane-MSSA-Merge].dbo.MSSA_trials group by eventid having eventid = [Oqtane-MSSA-Merge].dbo.MSSA_events.eventid)
update [Oqtane-MSSA-Merge].dbo.MSSA_events set enddate = (select max(trialdate) from [Oqtane-MSSA-Merge].dbo.MSSA_trials group by eventid having eventid = [Oqtane-MSSA-Merge].dbo.MSSA_events.eventid)

--Entries

SET IDENTITY_INSERT [Oqtane-MSSA-Merge].dbo.MSSA_entries ON
INSERT INTO [Oqtane-MSSA-Merge].dbo.MSSA_entries (
    entryid, trialid, handlerid, dogid, ClassId, placing, RunTime, TieBreakerTime,
    TrialPoints,
    HandlerIsMSSAMember, enteredtotalscore)
SELECT
    a.ID,
    b.Trial_ID,
    a.Handler_ID,
    a.Dog_ID,
    -- This import has no per-row Style/subclass field at all, unlike future imports
    -- which will - defaulting every row to 'On-foot' here (matching the fallback the
    -- original logic already used when subclass was blank), since there's no way to
    -- tell Horseback apart from On-foot from this data source. Six classes
    -- (Intermediate, Jr Handler, Novice, Nursery, Open, Pro Novice) have duplicate
    -- ClassName rows split by SubClassName, so this can't just be dropped - matching
    -- ClassName alone would make the subquery ambiguous for all of them.
    (SELECT ClassId
     FROM [Oqtane-MSSA-Merge].dbo.MSSA_Classes
     WHERE ClassName = a.Class
       AND SubClassName = 'On-foot') as ClassId,
    a.Placing,
    -- Reinterprets the stored hour as minutes and stored minute as seconds - Access
    -- read a duration like "5:16" (5 min 16 sec) as 5:16 AM, not as a raw duration.
    TIMEFROMPARTS(0, DATEPART(HOUR, a.[time]), DATEPART(MINUTE, a.[time]), 0, 0),
    TIMEFROMPARTS(0, DATEPART(HOUR, a.[tie_time]), DATEPART(MINUTE, a.[tie_time]), 0, 0),
    a.TrialPts,
    a.[MSSA Member],
    a.ptscore
FROM [MSSA-2026].dbo.Entries a
JOIN [MSSA-2026].dbo.Trials b ON a.Trial_Identifier = b.Trial_Identifier
JOIN [MSSA-2026].dbo.Events c ON b.Event_Identifer = c.Event_Identifer
SET IDENTITY_INSERT [Oqtane-MSSA-Merge].dbo.MSSA_entries OFF

select * from mssa_entries

--Event Class Offerings (replaces the old boolean flag columns on MSSA_Events, which
--nothing in the app reads anymore). One row per Event+Class actually seen in the
--imported Entries, Stock pulled from the owning Trial, PlannedRuns set to the real
--count of entries so nothing gets truncated when the app later matches trials to
--this offering (capped at PlannedRuns). Venue defaulted to 'Arena' - no source data
--for it in the Access export, confirmed acceptable for historical records.

INSERT INTO [Oqtane-MSSA-Merge].dbo.MSSA_EventClassOfferings (EventId, ClassId, Stock, Venue, PlannedRuns)
SELECT
    t.EventId,
    e.ClassId,
    ISNULL(t.Stock, 'Cattle') as Stock,
    'Arena' as Venue,
    COUNT(*) as PlannedRuns
FROM MSSA_entries e
JOIN MSSA_trials t ON e.TrialId = t.TrialId
GROUP BY t.EventId, e.ClassId, t.Stock

select * from MSSA_EventClassOfferings
