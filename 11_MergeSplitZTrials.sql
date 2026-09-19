--Found while spot-checking trial results: 23 trials from the original 9/3 migration
--exist TWICE in Oqtane-MSSA under two different TrialIds - once with a plain
--TrialIdentifier (e.g. "GSC_3_10_17A") and once with a trailing "Z" (e.g.
--"GSC_3_10_17AZ"), both under the same EventId. Janet's recollection: "Z" marked a
--duplicate hit during the original import. Because these "Z" trials sit on TrialIds
--that the *current* Access export has since reused for brand-new 2026 trials, the
--final-round merge (08_MergeFinalImport.sql) silently skipped those new trials -
--their NOT EXISTS check saw the ID as "already present" (holding the old Z-trial),
--even though the real occupant had changed.
--
--~45 of the entries under these Z trials collide on the natural key (TrialId, DogId,
--HandlerId, ClassId) with an entry already on the non-Z side - but unlike the earlier
--225-row Entries duplicate case, these do NOT have matching scores/times/placings in
--most cases. Could be genuine re-runs (the "Z" trial used as a workaround for the
--schema's one-entry-per-Trial+Dog+Handler+Class limit) rather than simple duplicate
--data entry. Exported to z_trial_duplicate_entries.csv for the admin to review -
--DO NOT touch those ~45 rows or delete any Z trial here; only the unambiguous part
--of the fix runs in this script.
--
--Run against local Oqtane-MSSA first, verify, then repeat directly against WinHost.

USE [Oqtane-MSSA];

--============================================================
--Step 1: Move every entry from each Z trial to its non-Z twin, except the ones that
--collide on the natural key - those stay under the Z trial untouched, pending the
--admin's review of z_trial_duplicate_entries.csv.
--============================================================

UPDATE ze
SET ze.TrialId = nz.TrialId
FROM [Oqtane-MSSA].dbo.MSSA_Entries ze
JOIN [Oqtane-MSSA].dbo.MSSA_Trials z ON z.TrialId = ze.TrialId
JOIN [Oqtane-MSSA].dbo.MSSA_Trials nz
    ON nz.TrialIdentifier = LEFT(z.TrialIdentifier, LEN(z.TrialIdentifier)-1)
   AND nz.EventId = z.EventId
WHERE z.TrialIdentifier LIKE '%Z' AND z.TrialId <> nz.TrialId
  AND NOT EXISTS (
      SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Entries nze
      WHERE nze.TrialId = nz.TrialId AND nze.DogId = ze.DogId
        AND nze.HandlerId = ze.HandlerId AND nze.ClassId = ze.ClassId
  );

PRINT 'Entries remaining under Z trials (should be only the disputed ~45, pending admin review):';
SELECT z.TrialId, z.TrialIdentifier, COUNT(*) as RemainingEntries
FROM [Oqtane-MSSA].dbo.MSSA_Trials z
JOIN [Oqtane-MSSA].dbo.MSSA_Entries ze ON ze.TrialId = z.TrialId
WHERE z.TrialIdentifier LIKE '%Z'
  AND EXISTS (
      SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Trials nz
      WHERE nz.TrialIdentifier = LEFT(z.TrialIdentifier, LEN(z.TrialIdentifier)-1) AND nz.EventId = z.EventId
  )
GROUP BY z.TrialId, z.TrialIdentifier
ORDER BY z.TrialId;

--============================================================
--Step 2: Insert the legitimate new 2026 trials under FRESH local TrialIds (not
--matching AccessMSSA's Trial_ID, since that number is still occupied by the
--not-yet-resolved Z trial). Only trials whose Access Trial_ID collides with an
--existing, differently-named local trial - i.e. exactly the 23 found, but written
--generally so it also catches any other such collision, not just this specific list.
--============================================================

INSERT INTO [Oqtane-MSSA].dbo.MSSA_Trials (EventId, TrialIdentifier, TrialDate, TrialName, Stock)
SELECT
    ev.EventId, c.Trial_Identifier, c.Trial_Date, c.Trial_Name, c.Species
FROM AccessMSSA.dbo.Trials c
JOIN AccessMSSA.dbo.Events ac ON ac.Event_Identifer = c.Event_Identifer
JOIN [Oqtane-MSSA].dbo.MSSA_Events ev ON ev.EventIdentifier = ac.Event_Identifer
JOIN [Oqtane-MSSA].dbo.MSSA_Trials stale ON stale.TrialId = c.Trial_ID AND stale.TrialIdentifier <> c.Trial_Identifier
WHERE NOT EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Trials existing
    WHERE existing.EventId = ev.EventId AND existing.TrialIdentifier = c.Trial_Identifier
);

PRINT 'New 2026 trials inserted under fresh local IDs:';
SELECT TrialId, EventId, TrialIdentifier, TrialDate, TrialName, Stock, CreatedDate
FROM [Oqtane-MSSA].dbo.MSSA_Trials
WHERE TrialIdentifier IN (SELECT Trial_Identifier FROM AccessMSSA.dbo.Trials)
  AND CreatedDate >= CAST(GETDATE() AS DATE)
ORDER BY TrialId;

--============================================================
--Step 3: Insert Entries for those new trials, matched by TrialIdentifier text (not
--numeric ID, since these trials didn't get Access's original Trial_ID).
--============================================================

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Entries ON;

INSERT INTO [Oqtane-MSSA].dbo.MSSA_Entries (
    EntryId, TrialId, HandlerId, DogId, ClassId, Placing, RunTime, TieBreakerTime,
    TrialPoints, HandlerIsMSSAMember, EnteredTotalScore)
SELECT
    a.ID,
    t.TrialId,
    a.Handler_ID,
    a.Dog_ID,
    (SELECT ClassId FROM [Oqtane-MSSA].dbo.MSSA_Classes
     WHERE ClassName = CASE WHEN a.Class = 'Junior Handler' THEN 'Jr Handler' ELSE a.Class END
       AND SubClassName = 'On-foot') as ClassId,
    a.Placing,
    TIMEFROMPARTS(0, DATEPART(HOUR, a.[time]), DATEPART(MINUTE, a.[time]), 0, 0),
    TIMEFROMPARTS(0, DATEPART(HOUR, a.[tie_time]), DATEPART(MINUTE, a.[tie_time]), 0, 0),
    a.TrialPts,
    a.[MSSA Member],
    a.ptscore
FROM AccessMSSA.dbo.Entries a
JOIN AccessMSSA.dbo.Trials b ON a.Trial_Identifier = b.Trial_Identifier
JOIN [Oqtane-MSSA].dbo.MSSA_Trials stale ON stale.TrialId = b.Trial_ID AND stale.TrialIdentifier <> b.Trial_Identifier
JOIN [Oqtane-MSSA].dbo.MSSA_Trials t ON t.TrialIdentifier = b.Trial_Identifier
WHERE NOT EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Entries e WHERE e.EntryId = a.ID
)
AND EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Handlers h WHERE h.HandlerId = a.Handler_ID
)
AND EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Dogs dg WHERE dg.DogId = a.Dog_ID
);

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Entries OFF;

PRINT 'Entries inserted for the new trials:';
SELECT COUNT(*) as new_trial_entries_inserted
FROM [Oqtane-MSSA].dbo.MSSA_Entries e
JOIN [Oqtane-MSSA].dbo.MSSA_Trials t ON t.TrialId = e.TrialId
WHERE t.CreatedDate >= CAST(GETDATE() AS DATE);

--============================================================
--Step 4: EventClassOfferings for anything new.
--============================================================

INSERT INTO [Oqtane-MSSA].dbo.MSSA_EventClassOfferings (EventId, ClassId, Stock, Venue, PlannedRuns)
SELECT
    t.EventId, e.ClassId, ISNULL(t.Stock, 'Cattle') as Stock, 'Arena' as Venue, COUNT(*) as PlannedRuns
FROM [Oqtane-MSSA].dbo.MSSA_Entries e
JOIN [Oqtane-MSSA].dbo.MSSA_Trials t ON e.TrialId = t.TrialId
WHERE NOT EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_EventClassOfferings o
    WHERE o.EventId = t.EventId AND o.ClassId = e.ClassId
)
GROUP BY t.EventId, e.ClassId, t.Stock;

PRINT 'Done. Remaining TODO: admin reviews z_trial_duplicate_entries.csv, then for';
PRINT 'each row either move it over (if a genuine dupe) or leave both as separate';
PRINT 'runs (if a real re-run) - once every entry is off a Z trial, delete that Z';
PRINT 'trial row so its number is fully retired.';
