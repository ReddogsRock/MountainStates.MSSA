-- Lets several Trial Secretaries share management access to one Event (adding trials,
-- editing, entries, submitting for approval) instead of only the person who created
-- it. The creator (MSSA_Events.CreatedByUserId) is unaffected and always has access;
-- this table adds everyone else who should too. Only the creator or an Admin may add
-- or remove team members - see MSSA_EventController. Safe to re-run.

IF OBJECT_ID('MSSA_EventTeamMembers', 'U') IS NULL
BEGIN
    CREATE TABLE MSSA_EventTeamMembers (
        EventTeamMemberId INT IDENTITY(1,1) PRIMARY KEY,
        EventId INT NOT NULL,
        UserId INT NOT NULL,
        CreatedDate DATETIME2 NOT NULL,
        CONSTRAINT FK_MSSA_EventTeamMembers_MSSA_Events FOREIGN KEY (EventId)
            REFERENCES MSSA_Events (EventId),
        CONSTRAINT UQ_MSSA_EventTeamMembers_Event_User UNIQUE (EventId, UserId)
    );
END
GO
