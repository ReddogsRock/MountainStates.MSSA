-- Backs the event-announcement mailing list with our own data instead of (or in
-- addition to) a Mailgun-hosted list, so first/last names survive and we can target
-- segments like "Handlers only" later. This is a one-time merge target for three
-- sources (MSSA_Handlers, the Oqtane User table, and an admin-provided Excel list),
-- not a live sync - rows are managed by hand (or re-import) after that initial load.
-- IsHandler is set at import time from whether the email matched MSSA_Handlers;
-- it's a snapshot, not a live link, so a Handler's info changing later doesn't
-- retroactively update this table. Safe to re-run.

IF OBJECT_ID('MSSA_MailingListSubscribers', 'U') IS NULL
BEGIN
    CREATE TABLE MSSA_MailingListSubscribers (
        SubscriberId INT IDENTITY(1,1) PRIMARY KEY,
        Email NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(100) NULL,
        LastName NVARCHAR(100) NULL,
        IsHandler BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        UnsubscribedDate DATETIME2 NULL,
        CreatedDate DATETIME2 NOT NULL,
        ModifiedDate DATETIME2 NOT NULL,
        CONSTRAINT UQ_MSSA_MailingListSubscribers_Email UNIQUE (Email)
    );
END
GO
