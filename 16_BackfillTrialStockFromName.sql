--Janet found 47 MSSA_Trials rows with Stock IS NULL. Broke down into: 6 disputed
--Z-trials (pending the admin's review - see project_access_migration_final_round
--memory), 28 with no Cattle/Sheep hint anywhere (mostly the new 2026 trials whose
--Access source never had a Species value - would need real source data, not
--inferable), and 13 where the TrialName/TrialIdentifier literally spells out the
--stock (e.g. "KJ Cattle Trial 1 2020", "Trial 1 Sheep", "MFC25 Cattle Trial 3").
--This backfills just those 13 - excludes Z-trials and anything mentioning both
--Cattle and Sheep (none as of 2026-09-21, but guarded in case that changes).
--
--Run against both local Oqtane-MSSA and WinHost (connect directly to WinHost).

UPDATE MSSA_Trials
SET Stock = 'Cattle'
WHERE Stock IS NULL
  AND TrialIdentifier NOT LIKE '%Z'
  AND (TrialName LIKE '%Cattle%' OR TrialIdentifier LIKE '%Cattle%')
  AND NOT (TrialName LIKE '%Sheep%' OR TrialIdentifier LIKE '%Sheep%');
SELECT @@ROWCOUNT as cattle_updated;

UPDATE MSSA_Trials
SET Stock = 'Sheep'
WHERE Stock IS NULL
  AND TrialIdentifier NOT LIKE '%Z'
  AND (TrialName LIKE '%Sheep%' OR TrialIdentifier LIKE '%Sheep%')
  AND NOT (TrialName LIKE '%Cattle%' OR TrialIdentifier LIKE '%Cattle%');
SELECT @@ROWCOUNT as sheep_updated;
