--Builds MSSA_Memberships and MSSA_MembershipHandlers in Oqtane-MSSA-Merge from the
--membership fields embedded in [MSSA-2026].dbo.Handlers (Membership Type/Amount/Paid
--By/Date Received/Due Date is a snapshot of the *current* membership; M2016-M2029 are
--per-year "was a member that year" flags going back further than the snapshot).
--
--Part 1 creates one "current" membership row per handler with a real Membership Type,
--linking a second handler via [2nd Handler] (matched by name) for Family types.
--Part 2 creates 'Unknown' placeholder rows (no Amount/PaidBy - matches the existing
--app convention for "known member, no payment detail") for any M-flag year not
--already covered by that handler's current snapshot range.
--
--Membership Type cleanup applied: 1F->AF, a1->AI, 3y->3F, 3A->3I, 4I kept as its own
--type (a real 4-year membership the app's dropdown doesn't offer - StartYear/EndYear
--hardcoded to 2024/2027 for that one record), garbage/dash-string values -> 'Unknown'.

USE [Oqtane-MSSA-Merge];

--Safe to re-run in the same SSMS session - temp tables persist for the life of the
--connection, not just one execution, so clear out anything left from a prior run.
--The "needs review" list is a real table in MSSA-2026 (not a temp table) so it's
--still there to query after today - it's not part of what gets migrated anywhere.
DROP TABLE IF EXISTS #CurrentMemberships;
DROP TABLE IF EXISTS #PlaceholderMemberships;
DROP TABLE IF EXISTS [MSSA-2026].dbo.MembershipSecondHandler_NeedsReview;

--============================================================
--Part 1: Current membership snapshot
--============================================================

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
            ELSE 'Unknown' -- catches the garbage dash-string and anything unexpected
        END AS MembershipType,
        h.Amount,
        h.[Paid By] AS PaidBy,
        h.[Date Received] AS DateReceived,
        h.[Due Date] AS DueDate
    FROM [MSSA-2026].dbo.Handlers h
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
SELECT *, ROW_NUMBER() OVER (ORDER BY HandlerId) AS RowNum
INTO #CurrentMemberships
FROM WithYears;

DECLARE @StartId INT = (SELECT ISNULL(MAX(MembershipId), 0) FROM MSSA_Memberships) + 1;

SET IDENTITY_INSERT MSSA_Memberships ON;
INSERT INTO MSSA_Memberships (MembershipId, MembershipType, StartYear, EndYear, Amount, PaidBy, DateReceived, CreatedDate, ModifiedDate)
SELECT @StartId + RowNum - 1, MembershipType, StartYear, EndYear, Amount, PaidBy, DateReceived, GETDATE(), GETDATE()
FROM #CurrentMemberships;
SET IDENTITY_INSERT MSSA_Memberships OFF;

--Primary handler link
INSERT INTO MSSA_MembershipHandlers (MembershipId, HandlerId, IsPrimary)
SELECT @StartId + RowNum - 1, HandlerId, 1
FROM #CurrentMemberships;

--Second handler link, matched by name. [2nd Handler] can hold more than one name
--comma-separated (e.g. "Stone, Stockton, Sage"), and a bare first name with no space
--(e.g. "Kathy") is treated as sharing the primary handler's last name - both patterns
--seen in the actual data. Review #UnmatchedSecondHandlers afterward for anything that
--still doesn't match (typos, or a person who isn't in Handlers at all).
INSERT INTO MSSA_MembershipHandlers (MembershipId, HandlerId, IsPrimary)
SELECT DISTINCT cm.RowNum + @StartId - 1, mh.HandlerId, 0
FROM #CurrentMemberships cm
CROSS APPLY STRING_SPLIT(cm.SecondHandlerName, ',') AS piece
JOIN [Oqtane-MSSA-Merge].dbo.MSSA_Handlers primary_mh ON primary_mh.HandlerId = cm.HandlerId
JOIN [Oqtane-MSSA-Merge].dbo.MSSA_Handlers mh
    ON (
        LTRIM(RTRIM(mh.FirstName + ' ' + mh.LastName)) = LTRIM(RTRIM(piece.value))
        OR (CHARINDEX(' ', LTRIM(RTRIM(piece.value))) = 0
            AND mh.FirstName = LTRIM(RTRIM(piece.value))
            AND mh.LastName = primary_mh.LastName)
    )
    AND mh.HandlerId <> cm.HandlerId -- never link the primary handler to themselves
WHERE cm.SecondHandlerName IS NOT NULL AND LTRIM(RTRIM(cm.SecondHandlerName)) <> '';

SELECT cm.HandlerId, LTRIM(RTRIM(piece.value)) AS UnmatchedName
INTO [MSSA-2026].dbo.MembershipSecondHandler_NeedsReview
FROM #CurrentMemberships cm
CROSS APPLY STRING_SPLIT(cm.SecondHandlerName, ',') AS piece
JOIN [Oqtane-MSSA-Merge].dbo.MSSA_Handlers primary_mh ON primary_mh.HandlerId = cm.HandlerId
WHERE cm.SecondHandlerName IS NOT NULL AND LTRIM(RTRIM(cm.SecondHandlerName)) <> ''
  AND NOT EXISTS (
      SELECT 1 FROM [Oqtane-MSSA-Merge].dbo.MSSA_Handlers mh
      WHERE (
          LTRIM(RTRIM(mh.FirstName + ' ' + mh.LastName)) = LTRIM(RTRIM(piece.value))
          OR (CHARINDEX(' ', LTRIM(RTRIM(piece.value))) = 0
              AND mh.FirstName = LTRIM(RTRIM(piece.value))
              AND mh.LastName = primary_mh.LastName)
      )
      AND mh.HandlerId <> cm.HandlerId
  );

SELECT * FROM [MSSA-2026].dbo.MembershipSecondHandler_NeedsReview; --review these - couldn't find a matching handler by name

--============================================================
--Part 2: Historical placeholder rows from M2016-M2029 flags, for years not already
--covered by that handler's current snapshot range.
--============================================================

;WITH Flagged AS (
    SELECT h.ID AS HandlerId, y.YearNum
    FROM [MSSA-2026].dbo.Handlers h
    CROSS APPLY (VALUES
        (2016, h.M2016), (2017, h.M2017), (2018, h.M2018), (2019, h.M2019), (2020, h.M2020),
        (2021, h.M2021), (2022, h.M2022), (2023, h.M2023), (2024, h.M2024), (2025, h.M2025),
        (2026, h.M2026), (2027, h.M2027), (2028, h.M2028), (2029, h.M2029)
    ) AS y(YearNum, IsMember)
    WHERE y.IsMember = 1
),
NotCovered AS (
    SELECT f.HandlerId, f.YearNum
    FROM Flagged f
    LEFT JOIN #CurrentMemberships cm
        ON cm.HandlerId = f.HandlerId
       AND f.YearNum BETWEEN cm.StartYear AND ISNULL(cm.EndYear, 9999)
    WHERE cm.HandlerId IS NULL
)
SELECT *, ROW_NUMBER() OVER (ORDER BY HandlerId, YearNum) AS RowNum
INTO #PlaceholderMemberships
FROM NotCovered;

DECLARE @StartId2 INT = (SELECT ISNULL(MAX(MembershipId), 0) FROM MSSA_Memberships) + 1;

SET IDENTITY_INSERT MSSA_Memberships ON;
INSERT INTO MSSA_Memberships (MembershipId, MembershipType, StartYear, EndYear, Amount, PaidBy, DateReceived, CreatedDate, ModifiedDate)
SELECT @StartId2 + RowNum - 1, 'Unknown', YearNum, YearNum, NULL, NULL, NULL, GETDATE(), GETDATE()
FROM #PlaceholderMemberships;
SET IDENTITY_INSERT MSSA_Memberships OFF;

INSERT INTO MSSA_MembershipHandlers (MembershipId, HandlerId, IsPrimary)
SELECT @StartId2 + RowNum - 1, HandlerId, 1
FROM #PlaceholderMemberships;

DROP TABLE #CurrentMemberships;
DROP TABLE #PlaceholderMemberships;
--MembershipSecondHandler_NeedsReview intentionally left in place in MSSA-2026 - a
--real table to query later, not part of anything that gets migrated.
