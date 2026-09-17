--Created a new database for the import of Access data
--Imported data using SSMS Tasks > Import data
--Data Source = Microsoft Access (Microsoft Access Database Engine)
--when selecting file use all file types
--Data Destination = SQL Server Native Client 11.0

--Dogs

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_dogs  ON

insert into [Oqtane-MSSA].dbo.MSSA_dogs (dogid,name, breed, DateOfBirth, RegistrationNumber, FirstCompetitionYear, 
	OwnerName, OwnerIsMSSAMember, isdeceased, IsSold,CreatedDate, ModifiedDate, IsActive)
select Next_ID, Dog_Name, Breed, cast(dob as date), Reg_Breed_Num, ISNULL(try_convert(int, firstyear), 0), Owner, Owner_MSSA, Dog_Deceased, Sold,
	GETDATE(), GETDATE(), 1
from [MSSA-Migrate].dbo.Dogs

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_dogs  Off

select *
from MSSA_dogs

--Events

truncate table event

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_events ON
insert into [Oqtane-MSSA].dbo.MSSA_Events([EventID],[EventIdentifier],[EventName],[City] ,[StateCode],
	[PointYear],[ChairmanName],[ChairmanPhone],[IsMSSASanctioned],[CreatedDate],[ResultsReceivedDate],[ResultsUploaded],
	[SanctionFee] ,[FeeReceivedDate]
	)
select event_Id, Event_Identifer, Event_Name, City, State,ISNULL(try_convert(int, Pt_Year), 0), Chairman, Phone, MSSA_Sanctioned, 
	isnull([Event Added], '1/1/1900'), [Results Rec'd], [Results Uploaed], [Sanction Fees], [Fee Rec'd]
from [MSSA-Migrate].dbo.Events
SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_events Off

--** add planning fields


select *
from MSSA_Events

select count(*)
from [MSSA-Migrate].dbo.Events

--Handler

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Handlers ON
insert into [Oqtane-MSSA].dbo.MSSA_Handlers ([HandlerId],[LastName], [FirstName], [Email], Phone, AlternatePhone, 
	Address, City, StateCode, HandlerLevel, LevelMoveUpDate, PhotoReleaseConsent)
select ID, isnull([Last Name], ''), isnull([First Name],''), [E-mail], Phone, Other_Phone, Address, City, State, 
	--case
	--	when Handler_Level = 'Open' then 1
	--	when Handler_Level = 'Novice' then 4
	--	when Handler_Level = 'Nursery' then 2
	--	when Handler_Level = 'Intermediate' then 3
	--	when Handler_Level = '%J%' then 5
	--end as HandlerLevel, 
	Handler_Level,
	[Move Up Date], 
	case 
		when [Photo Release] like '%y%' then 1
		when [Photo Release] like '%f%' then 0
		else 0
		end as release
from [MSSA-Migrate].dbo.Handlers
order by id
SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Handlers Off

select *
from MSSA_handlers

--Trials



SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_trials On

INSERT INTO [Oqtane-MSSA].dbo.MSSA_trials 
    (trialid, eventid, TrialIdentifier, trialdate, TrialName, Stock)
SELECT 
    c.trial_id, 
    a.eventid, 
    c.Trial_Identifier, 
    c.Trial_Date, 
    c.Trial_Name, 
    COALESCE(c.Species, e.Species) as Species
FROM [Oqtane-MSSA].dbo.MSSA_events a
JOIN [MSSA-Migrate].dbo.Events b
    ON a.EventIdentifier = b.Event_Identifer
JOIN [MSSA-Migrate].dbo.Trials c
    ON b.Event_Identifer = c.Event_Identifer
LEFT JOIN (
    SELECT Trial_Identifier, MIN(Species) as Species
    FROM [MSSA-Migrate].dbo.Entries
    WHERE Species IS NOT NULL
    GROUP BY Trial_Identifier
) e ON c.Trial_Identifier = e.Trial_Identifier
ORDER BY a.eventid, c.Trial_Identifier;

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_trials Off

select count(*)
from [MSSA-Migrate].dbo.Trials

--insert startdate and enddate into events table
update [Oqtane-MSSA].dbo.MSSA_events set startdate = (select min(trialdate) from [Oqtane-MSSA].dbo.MSSA_trials  group by eventid having eventid = [Oqtane-MSSA].dbo.MSSA_events.eventid)
update [Oqtane-MSSA].dbo.MSSA_events set enddate = (select max(trialdate) from [Oqtane-MSSA].dbo.MSSA_trials  group by eventid having eventid = [Oqtane-MSSA].dbo.MSSA_events.eventid)



SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_entries On
INSERT INTO [Oqtane-MSSA].dbo.MSSA_entries (
    entryid, trialid, handlerid, dogid, ClassId, runorder, placing, RunTime, TieBreakerTime, 
    ObstacleScore1, ObstacleScore2, ObstacleScore3, ObstacleScore4, ObstacleScore5, 
    ObstacleScore6, ObstacleScore7, ObstacleScore8, ObstacleScore9, Penalty, TrialPoints, 
    HandlerIsMSSAMember)
SELECT 
    a.ID, 
    b.Trial_ID, 
    a.Handler_ID, 
    a.Dog_ID, 
    (SELECT ClassId 
     FROM [Oqtane-MSSA].dbo.MSSA_Classes 
     WHERE ClassName = a.Class 
       AND SubClassName = CASE 
           WHEN LOWER(LTRIM(RTRIM(a.subclass))) = 'horseback' THEN 'Horseback'
           WHEN LOWER(LTRIM(RTRIM(a.subclass))) = 'onfoot' THEN 'On-foot'
           ELSE 'On-foot' -- default when NULL or empty
       END) as ClassId,
    a.Run_Order, 
    a.Placing, 
    LEFT(CONVERT(VARCHAR(8), CAST(a.[time] AS TIME), 108), 7), 
    LEFT(CONVERT(VARCHAR(8), CAST(a.[tie_time] AS TIME), 108), 7),
    [O1], [O2], [O3], [O4], [O5], [O6], [O7], [O8], [O9], 
    penalty,
    a.Trial_Pts, 
    MSSA_Mem
FROM [MSSA-Migrate].dbo.Entries a
JOIN [MSSA-Migrate].dbo.Trials b ON a.Trial_Identifier = b.Trial_Identifier
JOIN [MSSA-Migrate].dbo.Events c ON b.Event_Identifer = c.Event_Identifer
SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_entries Off


select *
from mssa_entries

--Set stock in event
update MSSA_events set cattle = 1
where eventid in (select eventid from mssa_trials where Stock = 'cattle')

update MSSA_events set cattle = 1
where eventid in (select eventid from mssa_trials where Stock = 'sheep')

update mssa_events set [Open] = 1
where eventid in (select distinct(t.eventid)
			from mssa_trials t
			join mssa_entries e
			on t.trialid = e.trialid
			join mssa_events ev
			on ev.eventid = t.eventid
			where e.classid in (1,5))

update mssa_events set [Nursery] = 1
where eventid in (select distinct(t.eventid)
			from mssa_trials t
			join mssa_entries e
			on t.trialid = e.trialid
			join mssa_events ev
			on ev.eventid = t.eventid
			where e.classid in (2,6))

update mssa_events set [Intermediate] = 1
where eventid in (select distinct(t.eventid)
			from mssa_trials t
			join mssa_entries e
			on t.trialid = e.trialid
			join mssa_events ev
			on ev.eventid = t.eventid
			where e.classid in (3,7))

update mssa_events set [Novice] = 1
where eventid in (select distinct(t.eventid)
			from mssa_trials t
			join mssa_entries e
			on t.trialid = e.trialid
			join mssa_events ev
			on ev.eventid = t.eventid
			where e.classid in (4,8))

update mssa_events set [Junior] = 1
where eventid in (select distinct(t.eventid)
			from mssa_trials t
			join mssa_entries e
			on t.trialid = e.trialid
			join mssa_events ev
			on ev.eventid = t.eventid
			where e.classid in (9,10))

update mssa_events set [Horseback] = 1
where eventid in (select distinct(t.eventid)
			from mssa_trials t
			join mssa_entries e
			on t.trialid = e.trialid
			join mssa_events ev
			on ev.eventid = t.eventid
			where e.classid in  (1,2,3,4,10,11))

update mssa_events set [OnFoot] = 1
where eventid in (select distinct(t.eventid)
			from mssa_trials t
			join mssa_entries e
			on t.trialid = e.trialid
			join mssa_events ev
			on ev.eventid = t.eventid
			where e.classid in (5,6,7,8,9,12))

