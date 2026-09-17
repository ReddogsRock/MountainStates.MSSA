--Final Access export (MSSA_final). Only 24 new HandlerIds vs what's already in
--Oqtane-MSSA. Memberships scoped to 2026 activity only, per Janet - everything
--before that was already handled by 05_MigrateMemberships.sql in the original round.
--
--Reuses the exact two-part logic from 05_MigrateMemberships.sql (current snapshot +
--M-flag placeholder), just scoped to year 2026 and guarded against anything already
--covering 2026 for that handler, so this is safe to re-run.
--
--Run Handlers first - Memberships needs the HandlerId FK to already exist.

USE [Oqtane-MSSA];

--============================================================
--Handlers - insert only HandlerIds not already present. Same column mapping as
--03_InsertFromOriginal_v2.sql.
--============================================================

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Handlers ON;

INSERT INTO [Oqtane-MSSA].dbo.MSSA_Handlers
    ([HandlerId],[LastName],[FirstName],[Email],Phone,AlternatePhone,Address,City,StateCode,
     HandlerLevel,LevelMoveUpDate,PhotoReleaseConsent)
SELECT
    s.ID, ISNULL(s.[Last Name], ''), ISNULL(s.[First Name], ''), s.[E-mail], s.Phone, s.Other_Phone,
    s.Address, s.City,
    CASE
        WHEN LEN(LTRIM(RTRIM(s.State))) <= 2 THEN s.State
        ELSE (SELECT TOP 1 StateCode FROM [Oqtane-MSSA].dbo.MSSA_States WHERE StateName = LTRIM(RTRIM(s.State)))
    END,
    s.Handler_Level,
    s.[Move Up Date],
    CASE
        WHEN s.[Photo Release] LIKE '%y%' THEN 1
        WHEN s.[Photo Release] LIKE '%f%' THEN 0
        ELSE 0
    END
FROM [MSSA_final].dbo.Handlers s
WHERE NOT EXISTS (
    SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_Handlers h WHERE h.HandlerId = s.ID
);

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Handlers OFF;

select count(*) as handlers_inserted from [Oqtane-MSSA].dbo.MSSA_Handlers
where HandlerId in (select ID from [MSSA_final].dbo.Handlers);

--============================================================
--Memberships - 2026 activity only.
--Part 1: real "current snapshot" rows (same type cleanup as 05_MigrateMemberships.sql:
--1F->AF, a1->AI, 3y->3F, 3A->3I, 4I kept, anything else -> Unknown) whose computed
--StartYear/EndYear range includes 2026, for any handler not already covered for 2026.
--============================================================

DROP TABLE IF EXISTS #New2026Snapshot;
DROP TABLE IF EXISTS #New2026Placeholder;

;WITH CurrentMemberships AS (
    SELECT
        h.ID AS HandlerId,
        h.[2nd Handler] AS SecondHandlerName,
        CASE
            WHEN LTRIM(RTRIM(h.[Membership Type])) IN ('AF','AI','3I','3F') THEN LTRIM(RTRIM(h.[Membership Type]))
            WHEN LTRIM(RTRIM(h.[Membership Type])) = '1F' THEN 'AF'
            WHEN LTRIM(RTRIM(h.[Membership Type])) = 'a1' THEN 'AI'
            WHEN LTRIM(RTRIM(h.[Membership Type])) = '3y' THEN '3F'
            WHEN LTRIM(RTRIM(h.[Membership Type])) = '3A' THEN '3I'
            WHEN LTRIM(RTRIM(h.[Membership Type])) = '4I' THEN '4I'
            ELSE 'Unknown'
        END AS MembershipType,
        h.Amount,
        h.[Paid By] AS PaidBy,
        h.[Date Received] AS DateReceived,
        h.[Due Date] AS DueDate
    FROM [MSSA_final].dbo.Handlers h
    WHERE h.[Membership Type] IS NOT NULL
),
WithYears AS (
    SELECT
        HandlerId, SecondHandlerName, MembershipType, Amount, PaidBy, DateReceived,
        CASE WHEN MembershipType = '4I' THEN 2024
             ELSE COALESCE(YEAR(DateReceived), YEAR(DueDate), YEAR(GETDATE()))
        END AS StartYear,
        CASE WHEN MembershipType = '4I' THEN 2027
             WHEN MembershipType IN ('3I','3F') THEN COALESCE(YEAR(DateReceived), YEAR(DueDate), YEAR(GETDATE())) + 2
             ELSE COALESCE(YEAR(DateReceived), YEAR(DueDate), YEAR(GETDATE()))
        END AS EndYear
    FROM CurrentMemberships
)
SELECT wy.*, ROW_NUMBER() OVER (ORDER BY wy.HandlerId) AS RowNum
INTO #New2026Snapshot
FROM WithYears wy
WHERE 2026 BETWEEN wy.StartYear AND wy.EndYear
  AND NOT EXISTS (
      SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_MembershipHandlers mh
      JOIN [Oqtane-MSSA].dbo.MSSA_Memberships m ON m.MembershipId = mh.MembershipId
      WHERE mh.HandlerId = wy.HandlerId AND 2026 BETWEEN m.StartYear AND ISNULL(m.EndYear, 9999)
  );

DECLARE @StartId INT = (SELECT ISNULL(MAX(MembershipId), 0) FROM [Oqtane-MSSA].dbo.MSSA_Memberships) + 1;

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Memberships ON;
INSERT INTO [Oqtane-MSSA].dbo.MSSA_Memberships (MembershipId, MembershipType, StartYear, EndYear, Amount, PaidBy, DateReceived, CreatedDate, ModifiedDate)
SELECT @StartId + RowNum - 1, MembershipType, StartYear, EndYear, Amount, PaidBy, DateReceived, GETDATE(), GETDATE()
FROM #New2026Snapshot;
SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Memberships OFF;

INSERT INTO [Oqtane-MSSA].dbo.MSSA_MembershipHandlers (MembershipId, HandlerId, IsPrimary)
SELECT @StartId + RowNum - 1, HandlerId, 1
FROM #New2026Snapshot;

--Second handler link, same name-matching approach as 05_MigrateMemberships.sql -
--handles comma-separated names and a bare first name sharing the primary's last name.
INSERT INTO [Oqtane-MSSA].dbo.MSSA_MembershipHandlers (MembershipId, HandlerId, IsPrimary)
SELECT DISTINCT cm.RowNum + @StartId - 1, mh.HandlerId, 0
FROM #New2026Snapshot cm
CROSS APPLY STRING_SPLIT(cm.SecondHandlerName, ',') AS piece
JOIN [Oqtane-MSSA].dbo.MSSA_Handlers primary_mh ON primary_mh.HandlerId = cm.HandlerId
JOIN [Oqtane-MSSA].dbo.MSSA_Handlers mh
    ON (
        LTRIM(RTRIM(mh.FirstName + ' ' + mh.LastName)) = LTRIM(RTRIM(piece.value))
        OR (CHARINDEX(' ', LTRIM(RTRIM(piece.value))) = 0
            AND mh.FirstName = LTRIM(RTRIM(piece.value))
            AND mh.LastName = primary_mh.LastName)
    )
    AND mh.HandlerId <> cm.HandlerId
WHERE cm.SecondHandlerName IS NOT NULL AND LTRIM(RTRIM(cm.SecondHandlerName)) <> '';

select count(*) as new_2026_snapshot_memberships from #New2026Snapshot;

--============================================================
--Part 2: 'Unknown' placeholder rows for any handler with M2026=1 not covered by
--Part 1 above or by an already-existing row (same convention as
--05_MigrateMemberships.sql: known member, no payment detail).
--============================================================

;WITH Flagged2026 AS (
    SELECT h.ID AS HandlerId
    FROM [MSSA_final].dbo.Handlers h
    WHERE h.M2026 = 1
),
NotCovered AS (
    SELECT f.HandlerId
    FROM Flagged2026 f
    WHERE NOT EXISTS (SELECT 1 FROM #New2026Snapshot s WHERE s.HandlerId = f.HandlerId)
      AND NOT EXISTS (
          SELECT 1 FROM [Oqtane-MSSA].dbo.MSSA_MembershipHandlers mh
          JOIN [Oqtane-MSSA].dbo.MSSA_Memberships m ON m.MembershipId = mh.MembershipId
          WHERE mh.HandlerId = f.HandlerId AND 2026 BETWEEN m.StartYear AND ISNULL(m.EndYear, 9999)
      )
)
SELECT *, ROW_NUMBER() OVER (ORDER BY HandlerId) AS RowNum
INTO #New2026Placeholder
FROM NotCovered;

DECLARE @StartId2 INT = (SELECT ISNULL(MAX(MembershipId), 0) FROM [Oqtane-MSSA].dbo.MSSA_Memberships) + 1;

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Memberships ON;
INSERT INTO [Oqtane-MSSA].dbo.MSSA_Memberships (MembershipId, MembershipType, StartYear, EndYear, Amount, PaidBy, DateReceived, CreatedDate, ModifiedDate)
SELECT @StartId2 + RowNum - 1, 'Unknown', 2026, 2026, NULL, NULL, NULL, GETDATE(), GETDATE()
FROM #New2026Placeholder;
SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Memberships OFF;

INSERT INTO [Oqtane-MSSA].dbo.MSSA_MembershipHandlers (MembershipId, HandlerId, IsPrimary)
SELECT @StartId2 + RowNum - 1, HandlerId, 1
FROM #New2026Placeholder;

select count(*) as new_2026_placeholder_memberships from #New2026Placeholder;

DROP TABLE #New2026Snapshot;
DROP TABLE #New2026Placeholder;
