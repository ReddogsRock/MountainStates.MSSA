-- Adds the event-creation approval workflow: an event created by anyone other than
-- an Admin starts Pending and is hidden from public view until an Admin approves it.
-- DEFAULT 'Approved' means every event that already exists keeps showing normally -
-- only new events created by a non-Admin from here on start Pending. Safe to re-run.

IF COL_LENGTH('MSSA_Events', 'ApprovalStatus') IS NULL
BEGIN
    ALTER TABLE MSSA_Events ADD ApprovalStatus VARCHAR(20) NOT NULL DEFAULT 'Approved';
END
GO

IF COL_LENGTH('MSSA_Events', 'ApprovedDate') IS NULL
BEGIN
    ALTER TABLE MSSA_Events ADD ApprovedDate DATETIME2 NULL;
END
GO

IF COL_LENGTH('MSSA_Events', 'ApprovedByUserId') IS NULL
BEGIN
    ALTER TABLE MSSA_Events ADD ApprovedByUserId INT NULL;
END
GO
