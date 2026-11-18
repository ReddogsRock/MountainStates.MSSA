-- Backs the "Send a Campaign" feature's saved email content - lets the admin save a
-- Subject + Body under a name (e.g. "Annual Renewal") and reuse it on future sends
-- instead of recomposing from scratch every time. Safe to re-run.

IF OBJECT_ID('MSSA_MailingListEmailTemplates', 'U') IS NULL
BEGIN
    CREATE TABLE MSSA_MailingListEmailTemplates (
        TemplateId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Subject NVARCHAR(255) NOT NULL,
        BodyHtml NVARCHAR(MAX) NOT NULL,
        CreatedDate DATETIME2 NOT NULL,
        ModifiedDate DATETIME2 NOT NULL
    );
END
GO
