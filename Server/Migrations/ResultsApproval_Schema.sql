-- Adds the two schema pieces behind the new Results module (score entry + approval).
-- Safe to re-run: each block only acts if the column doesn't already exist.

IF COL_LENGTH('MSSA_Entries', 'EnteredTotalScore') IS NULL
BEGIN
    ALTER TABLE MSSA_Entries ADD EnteredTotalScore DECIMAL(10,2) NULL;
END
GO

IF COL_LENGTH('MSSA_Events', 'ResultsApprovalStatus') IS NULL
BEGIN
    ALTER TABLE MSSA_Events ADD ResultsApprovalStatus VARCHAR(20) NOT NULL DEFAULT 'NotSubmitted';
END
GO

IF COL_LENGTH('MSSA_Events', 'ResultsSubmittedDate') IS NULL
BEGIN
    ALTER TABLE MSSA_Events ADD ResultsSubmittedDate DATETIME NULL;
END
GO

IF COL_LENGTH('MSSA_Events', 'ResultsSubmittedByUserId') IS NULL
BEGIN
    ALTER TABLE MSSA_Events ADD ResultsSubmittedByUserId INT NULL;
END
GO

IF COL_LENGTH('MSSA_Events', 'ResultsApprovedDate') IS NULL
BEGIN
    ALTER TABLE MSSA_Events ADD ResultsApprovedDate DATETIME NULL;
END
GO

IF COL_LENGTH('MSSA_Events', 'ResultsApprovedByUserId') IS NULL
BEGIN
    ALTER TABLE MSSA_Events ADD ResultsApprovedByUserId INT NULL;
END
GO
