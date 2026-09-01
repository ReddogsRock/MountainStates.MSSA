-- Run this against PROD to confirm you're pointed at the right database and that
-- every column the Results module expects actually exists there.

SELECT
    @@SERVERNAME            AS ServerName,
    DB_NAME()               AS DatabaseName;

SELECT
    COL_LENGTH('MSSA_Events', 'ResultsApprovalStatus')     AS Events_ResultsApprovalStatus,
    COL_LENGTH('MSSA_Events', 'ResultsSubmittedDate')      AS Events_ResultsSubmittedDate,
    COL_LENGTH('MSSA_Events', 'ResultsSubmittedByUserId')  AS Events_ResultsSubmittedByUserId,
    COL_LENGTH('MSSA_Events', 'ResultsApprovedDate')       AS Events_ResultsApprovedDate,
    COL_LENGTH('MSSA_Events', 'ResultsApprovedByUserId')   AS Events_ResultsApprovedByUserId,
    COL_LENGTH('MSSA_Entries', 'EnteredTotalScore')        AS Entries_EnteredTotalScore,
    COL_LENGTH('MSSA_Trials', 'ScorekeeperUserId')         AS Trials_ScorekeeperUserId;
-- Every column above should return a number (its byte length), not NULL.
-- If any is NULL, that ALTER TABLE never actually ran against this database.

-- If all six columns are present, check that existing rows actually got a value -
-- ResultsApprovalStatus is NOT NULL, so every row should show 'NotSubmitted' here,
-- never a blank/NULL count.
SELECT ResultsApprovalStatus, COUNT(*) AS EventCount
FROM MSSA_Events
GROUP BY ResultsApprovalStatus;
