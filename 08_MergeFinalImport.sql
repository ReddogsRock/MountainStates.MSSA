--Final Access export, imported into MSSA_final (same schema as MSSA-2026, used for
--03_InsertFromOriginal_v2.sql). Merges only what's missing into local Oqtane-MSSA -
--matched by the same ID convention already confirmed: Next_ID = DogId, ID = EntryId,
--Event_Id = EventId, Trial_ID = TrialId.
--
--Run sections in order: Dogs/DogFuturityParticipation already ran (2026-09-16, 75
--dogs / 449 futurity rows). Events and Trials must run before Entries (FK); Entries
--must run before EventClassOfferings (derives PlannedRuns by counting them).
--MSSA_final.Events has no class-offering columns of its own, same as the original
--migration - so EventClassOfferings is still derived from Entries grouped by
--Event+Class, not read directly off Events.

USE [Oqtane-MSSA];

--============================================================
--Dogs - insert only DogIds not already present.
--============================================================

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Dogs ON;

INSERT INTO [Oqtane-MSSA].dbo.MSSA_Dogs
    (DogId, Name, Breed, DateOfBirth, RegistrationNumber, FirstCompetitionYear,
     OwnerName, OwnerIsMSSAMember, IsDeceased, IsSold, CreatedDate, ModifiedDate, IsActive)
SELECT
    s.Next_ID, s.Dog_Name, s.Breed, CAST(s.DOB AS DATE), s.Reg_Breed_Num,
    ISNULL(TRY_CONVERT(INT, s.FirstYeaR), 0), s.Owner, s.Owner_MSSA, s.Dog_Deceased, s.Sold,
    GETDATE(), GETDATE(), 1
FROM [MSSA_final].dbo.Dogs s
WHERE NOT EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Dogs d WHERE d.DogId = s.Next_ID
);

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Dogs OFF;

select count(*) as dogs_inserted from [Oqtane-MSSA].dbo.MSSA_Dogs
where DogId in (select Next_ID from [MSSA_final].dbo.Dogs);

--============================================================
--DogFuturityParticipation - unpivot the nine per-year boolean columns into rows.
--Confirmed: bulk-imported historical enrollments are marked Status = 'Paid' for
--every year, 2027 included (already collected offline before this system existed).
--Guarded by DogId+Year so this is safe to re-run.
--============================================================

INSERT INTO [Oqtane-MSSA].dbo.MSSA_DogFuturityParticipation
    (DogId, Year, Status, CreatedDate, ModifiedDate)
SELECT y.DogId, y.Year, 'Paid', GETDATE(), GETDATE()
FROM (
    SELECT Next_ID AS DogId, 2019 AS Year FROM [MSSA_final].dbo.Dogs WHERE [Futurity 2019] = 1
    UNION ALL
    SELECT Next_ID, 2020 FROM [MSSA_final].dbo.Dogs WHERE [Futurity 2020] = 1
    UNION ALL
    SELECT Next_ID, 2021 FROM [MSSA_final].dbo.Dogs WHERE [Futurity 2021] = 1
    UNION ALL
    SELECT Next_ID, 2022 FROM [MSSA_final].dbo.Dogs WHERE [Futurity 2022] = 1
    UNION ALL
    SELECT Next_ID, 2023 FROM [MSSA_final].dbo.Dogs WHERE [Futurity 2023] = 1
    UNION ALL
    SELECT Next_ID, 2024 FROM [MSSA_final].dbo.Dogs WHERE [Futurity 2024] = 1
    UNION ALL
    SELECT Next_ID, 2025 FROM [MSSA_final].dbo.Dogs WHERE [Futurity 2025] = 1
    UNION ALL
    SELECT Next_ID, 2026 FROM [MSSA_final].dbo.Dogs WHERE [Futurity 2026] = 1
    UNION ALL
    SELECT Next_ID, 2027 FROM [MSSA_final].dbo.Dogs WHERE [Futurity 2027] = 1
) y
WHERE NOT EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_DogFuturityParticipation p
    WHERE p.DogId = y.DogId AND p.Year = y.Year
);

select DogId, Year, Status from [Oqtane-MSSA].dbo.MSSA_DogFuturityParticipation order by DogId, Year;

--============================================================
--Events - insert only EventIds not already present. Same column mapping as
--03_InsertFromOriginal_v2.sql, including the StateCode full-name-vs-code resolution.
--============================================================

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Events ON;

INSERT INTO [Oqtane-MSSA].dbo.MSSA_Events
    ([EventID],[EventIdentifier],[EventName],[City],[StateCode],[PointYear],[ChairmanName],
     [ChairmanPhone],[IsMSSASanctioned],[CreatedDate],[ResultsReceivedDate],[ResultsUploaded],
     [SanctionFee],[FeeReceivedDate],[ResultsApprovalStatus],[ResultsApprovedDate],
     [ApprovalStatus],[ApprovedDate])
SELECT
    s.Event_ID, s.Event_Identifer, s.Event_Name, s.City,
    CASE
        WHEN LEN(LTRIM(RTRIM(s.State))) <= 2 THEN s.State
        ELSE (SELECT TOP 1 StateCode FROM [Oqtane-MSSA].dbo.MSSA_States WHERE StateName = LTRIM(RTRIM(s.State)))
    END,
    ISNULL(TRY_CONVERT(INT, s.Pt_Year), 0), s.Chairman, s.Phone, s.MSSA_Sanctioned,
    ISNULL(s.[Event Added], '1/1/1900'), s.[Results Rec'd], s.[Results Uploaed], s.[Sanction Fees], s.[Fee Rec'd],
    'Approved', GETDATE(),
    'Approved', GETDATE()
FROM [MSSA_final].dbo.Events s
WHERE NOT EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Events e WHERE e.EventId = s.Event_ID
);

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Events OFF;

select count(*) as events_inserted from [Oqtane-MSSA].dbo.MSSA_Events
where EventId in (select Event_ID from [MSSA_final].dbo.Events);

--============================================================
--Trials - insert only TrialIds not already present. MSSA_final's Trials already
--carries Species per row (unlike the original migration's source), so no need for
--the Entries-species fallback this time - keeping it anyway in case a row is blank.
--============================================================

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Trials ON;

INSERT INTO [Oqtane-MSSA].dbo.MSSA_Trials
    (TrialId, EventId, TrialIdentifier, TrialDate, TrialName, Stock)
SELECT
    c.Trial_ID,
    a.EventId,
    c.Trial_Identifier,
    c.Trial_Date,
    c.Trial_Name,
    COALESCE(c.Species, e.Species) as Stock
FROM [Oqtane-MSSA].dbo.MSSA_Events a
JOIN [MSSA_final].dbo.Events b ON a.EventIdentifier = b.Event_Identifer
JOIN [MSSA_final].dbo.Trials c ON b.Event_Identifer = c.Event_Identifer
LEFT JOIN (
    SELECT Trial_Identifier, MIN(Species) as Species
    FROM [MSSA_final].dbo.Entries
    WHERE Species IS NOT NULL
    GROUP BY Trial_Identifier
) e ON c.Trial_Identifier = e.Trial_Identifier
WHERE NOT EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Trials t WHERE t.TrialId = c.Trial_ID
);

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Trials OFF;

select count(*) as trials_inserted from [Oqtane-MSSA].dbo.MSSA_Trials
where TrialId in (select Trial_ID from [MSSA_final].dbo.Trials);

--Recompute StartDate/EndDate for all events (idempotent - already-correct events just
--get the same value back).
update [Oqtane-MSSA].dbo.MSSA_events set startdate = (select min(trialdate) from [Oqtane-MSSA].dbo.MSSA_trials group by eventid having eventid = [Oqtane-MSSA].dbo.MSSA_events.eventid);
update [Oqtane-MSSA].dbo.MSSA_events set enddate = (select max(trialdate) from [Oqtane-MSSA].dbo.MSSA_trials group by eventid having eventid = [Oqtane-MSSA].dbo.MSSA_events.eventid);

--============================================================
--Entries - insert only EntryIds not already present. Same column mapping as
--03_InsertFromOriginal_v2.sql, including the ptscore -> EnteredTotalScore fix and
--the hour/minute reinterpretation for Time/TieBreakerTime.
--Depends on Trials already existing in Oqtane-MSSA for the joins below to resolve -
--check the row count against MSSA_final's Entries count once this runs.
--============================================================

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Entries ON;

INSERT INTO [Oqtane-MSSA].dbo.MSSA_Entries (
    EntryId, TrialId, HandlerId, DogId, ClassId, Placing, RunTime, TieBreakerTime,
    TrialPoints, HandlerIsMSSAMember, EnteredTotalScore)
SELECT
    a.ID,
    b.Trial_ID,
    a.Handler_ID,
    a.Dog_ID,
    (SELECT ClassId
     FROM [Oqtane-MSSA].dbo.MSSA_Classes
     WHERE ClassName = CASE WHEN a.Class = 'Junior Handler' THEN 'Jr Handler' ELSE a.Class END
       AND SubClassName = 'On-foot') as ClassId,
    a.Placing,
    TIMEFROMPARTS(0, DATEPART(HOUR, a.[time]), DATEPART(MINUTE, a.[time]), 0, 0),
    TIMEFROMPARTS(0, DATEPART(HOUR, a.[tie_time]), DATEPART(MINUTE, a.[tie_time]), 0, 0),
    a.TrialPts,
    a.[MSSA Member],
    a.ptscore
FROM [MSSA_final].dbo.Entries a
JOIN [MSSA_final].dbo.Trials b ON a.Trial_Identifier = b.Trial_Identifier
JOIN [MSSA_final].dbo.Events c ON b.Event_Identifer = c.Event_Identifer
WHERE NOT EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Entries e WHERE e.EntryId = a.ID
)
-- Skip re-numbered duplicates of entries already in production under a different
-- EntryId - confirmed safe via a 2026-only spot check (matching scores). See
-- duplicate_entries_review.csv for the full 225-row review this decision was based on.
AND NOT EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Entries e3
    WHERE e3.TrialId = b.Trial_ID AND e3.DogId = a.Dog_ID AND e3.HandlerId = a.Handler_ID
      AND e3.ClassId = (SELECT ClassId FROM [Oqtane-MSSA].dbo.MSSA_Classes c2
                         WHERE c2.ClassName = CASE WHEN a.Class = 'Junior Handler' THEN 'Jr Handler' ELSE a.Class END
                           AND c2.SubClassName = 'On-foot')
)
-- Skip rows whose HandlerId doesn't exist yet in Oqtane-MSSA - Handlers hasn't been
-- merged from MSSA_final yet. These pick up automatically once that's done and this
-- section is re-run (18 handlers / 50 entries as of 2026-09-16).
AND EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Handlers h WHERE h.HandlerId = a.Handler_ID
);

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Entries OFF;

select count(*) as entries_inserted from [Oqtane-MSSA].dbo.MSSA_Entries
where EntryId in (select ID from [MSSA_final].dbo.Entries);

select count(*) as entries_in_source from [MSSA_final].dbo.Entries;

--============================================================
--EventClassOfferings - one row per Event+Class actually seen in Entries, same
--derivation as 03_InsertFromOriginal_v2.sql (no source columns for this on Events
--itself). Guarded on Event+Class so already-offered combinations aren't touched -
--in particular this won't bump PlannedRuns on an existing row if more entries get
--merged into an already-offered class later; delete and re-run this section if that
--needs picking up.
--============================================================

INSERT INTO [Oqtane-MSSA].dbo.MSSA_EventClassOfferings (EventId, ClassId, Stock, Venue, PlannedRuns)
SELECT
    t.EventId,
    e.ClassId,
    ISNULL(t.Stock, 'Cattle') as Stock,
    'Arena' as Venue,
    COUNT(*) as PlannedRuns
FROM [Oqtane-MSSA].dbo.MSSA_Entries e
JOIN [Oqtane-MSSA].dbo.MSSA_Trials t ON e.TrialId = t.TrialId
WHERE NOT EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_EventClassOfferings o
    WHERE o.EventId = t.EventId AND o.ClassId = e.ClassId
)
GROUP BY t.EventId, e.ClassId, t.Stock;

select count(*) as offerings_now from [Oqtane-MSSA].dbo.MSSA_EventClassOfferings;
