--The full 03 -> 04 -> 06 pipeline already ran before EnteredTotalScore was added to
--03_InsertFromOriginal_v2.sql's Entries insert, so Oqtane-MSSA-Merge, local
--Oqtane-MSSA, and WinHost production all currently have EnteredTotalScore = NULL for
--every row. Source of truth is [MSSA-2026].dbo.Entries.ptscore, joined by
--EntryId = ID - the same join 03_InsertFromOriginal_v2.sql used for the original
--insert.
--
--Run each section in order and check the row count each returns.

--============================================================
--Oqtane-MSSA-Merge (so it matches what a future replay of 04/06 would produce)
--============================================================

UPDATE e
SET e.EnteredTotalScore = s.ptscore
FROM [Oqtane-MSSA-Merge].dbo.MSSA_Entries e
JOIN [MSSA-2026].dbo.Entries s ON e.EntryId = s.ID
WHERE s.ptscore IS NOT NULL
  AND e.EnteredTotalScore IS NULL;

select EntryId, EnteredTotalScore from [Oqtane-MSSA-Merge].dbo.MSSA_Entries where EnteredTotalScore is not null;

--============================================================
--Local Oqtane-MSSA
--============================================================

UPDATE e
SET e.EnteredTotalScore = s.ptscore
FROM [Oqtane-MSSA].dbo.MSSA_Entries e
JOIN [MSSA-2026].dbo.Entries s ON e.EntryId = s.ID
WHERE s.ptscore IS NOT NULL
  AND e.EnteredTotalScore IS NULL;

select EntryId, EnteredTotalScore from [Oqtane-MSSA].dbo.MSSA_Entries where EnteredTotalScore is not null;

--============================================================
--WinHost production - PLAN B, avoids the slow linked-server row-by-row join.
--Builds the whole UPDATE as one SQL string locally (cheap - MSSA-2026 is local),
--then sends it as a SINGLE remote batch via EXEC(...) AT WINHOST. Everything in the
--string runs entirely on WinHost's side once it arrives - no per-row round trips.
--Uses a local #temp table on the remote side as a mini staging table, joined once.
--============================================================

DECLARE @sql NVARCHAR(MAX) = N'
CREATE TABLE #scores (EntryId INT PRIMARY KEY, PtScore DECIMAL(18,2));
';

SELECT @sql = @sql + N'INSERT INTO #scores (EntryId, PtScore) VALUES (' +
    CAST(ID AS NVARCHAR(10)) + ',' + CAST(ptscore AS NVARCHAR(30)) + ');' + CHAR(13)
FROM [MSSA-2026].dbo.Entries
WHERE ptscore IS NOT NULL;

SET @sql = @sql + N'
UPDATE e SET e.EnteredTotalScore = s.PtScore
FROM MSSA_Entries e
JOIN #scores s ON e.EntryId = s.EntryId
WHERE e.EnteredTotalScore IS NULL;

DROP TABLE #scores;
';

-- Sanity check before sending - eyeball the row count and a sample of the text.
SELECT LEN(@sql) AS sql_length, (SELECT COUNT(*) FROM [MSSA-2026].dbo.Entries WHERE ptscore IS NOT NULL) AS rows_to_update;

EXEC (@sql) AT WINHOST;

select EntryId, EnteredTotalScore from [WINHOST].[DB_169191_mtstates].dbo.MSSA_Entries where EnteredTotalScore is not null;
