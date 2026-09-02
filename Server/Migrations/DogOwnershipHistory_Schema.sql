-- Tracks each ownership transfer for a dog. MSSA_Dogs.OwnerName continues to hold
-- the CURRENT owner; this table is purely the audit trail of past transfers, written
-- whenever someone records a sale via the Ownership Transfer section on the dog's
-- Detail page. Safe to re-run.

IF OBJECT_ID('MSSA_DogOwnershipHistory', 'U') IS NULL
BEGIN
    CREATE TABLE MSSA_DogOwnershipHistory (
        DogOwnershipHistoryId INT IDENTITY(1,1) PRIMARY KEY,
        DogId INT NOT NULL,
        PreviousOwnerName VARCHAR(255) NULL,
        NewOwnerName VARCHAR(255) NOT NULL,
        TransferDate DATETIME2 NOT NULL,
        CreatedDate DATETIME2 NOT NULL,
        CONSTRAINT FK_MSSA_DogOwnershipHistory_MSSA_Dogs FOREIGN KEY (DogId)
            REFERENCES MSSA_Dogs (DogId)
    );
END
GO
