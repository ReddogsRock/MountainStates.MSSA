-- Backs the "Send a Campaign" feature's reusable audience lists (e.g. "Board
-- Members") - the admin creates a named list once, picks members into it, and
-- reuses it on future sends. Membership is a join table: a subscriber can belong to
-- more than one named list, and a list's membership is fully replaced on each save
-- (no per-row add/remove tracking needed). Safe to re-run.

IF OBJECT_ID('MSSA_MailingListNamedLists', 'U') IS NULL
BEGIN
    CREATE TABLE MSSA_MailingListNamedLists (
        NamedListId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        CreatedDate DATETIME2 NOT NULL,
        ModifiedDate DATETIME2 NOT NULL
    );
END
GO

IF OBJECT_ID('MSSA_MailingListNamedListMembers', 'U') IS NULL
BEGIN
    CREATE TABLE MSSA_MailingListNamedListMembers (
        NamedListMemberId INT IDENTITY(1,1) PRIMARY KEY,
        NamedListId INT NOT NULL,
        SubscriberId INT NOT NULL,
        CreatedDate DATETIME2 NOT NULL,
        CONSTRAINT FK_MSSA_MailingListNamedListMembers_NamedList
            FOREIGN KEY (NamedListId) REFERENCES MSSA_MailingListNamedLists(NamedListId),
        CONSTRAINT FK_MSSA_MailingListNamedListMembers_Subscriber
            FOREIGN KEY (SubscriberId) REFERENCES MSSA_MailingListSubscribers(SubscriberId),
        CONSTRAINT UQ_MSSA_MailingListNamedListMembers_List_Subscriber
            UNIQUE (NamedListId, SubscriberId)
    );
END
GO
