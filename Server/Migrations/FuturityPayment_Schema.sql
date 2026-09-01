-- Documents the columns already added to MSSA_DogFuturityParticipation for Stripe
-- payment tracking (per Janet, already applied to local dev). Safe to re-run - only
-- acts on whichever of these columns don't already exist on the target database.

IF COL_LENGTH('MSSA_DogFuturityParticipation', 'Status') IS NULL
BEGIN
    ALTER TABLE MSSA_DogFuturityParticipation ADD Status VARCHAR(20) NOT NULL DEFAULT 'PendingPayment';
END
GO

IF COL_LENGTH('MSSA_DogFuturityParticipation', 'PaymentMethod') IS NULL
BEGIN
    ALTER TABLE MSSA_DogFuturityParticipation ADD PaymentMethod VARCHAR(20) NULL;
END
GO

IF COL_LENGTH('MSSA_DogFuturityParticipation', 'Amount') IS NULL
BEGIN
    ALTER TABLE MSSA_DogFuturityParticipation ADD Amount DECIMAL(10,2) NULL;
END
GO

IF COL_LENGTH('MSSA_DogFuturityParticipation', 'PaidBy') IS NULL
BEGIN
    ALTER TABLE MSSA_DogFuturityParticipation ADD PaidBy VARCHAR(100) NULL;
END
GO

IF COL_LENGTH('MSSA_DogFuturityParticipation', 'DateReceived') IS NULL
BEGIN
    ALTER TABLE MSSA_DogFuturityParticipation ADD DateReceived DATE NULL;
END
GO

IF COL_LENGTH('MSSA_DogFuturityParticipation', 'StripePaymentIntentId') IS NULL
BEGIN
    ALTER TABLE MSSA_DogFuturityParticipation ADD StripePaymentIntentId VARCHAR(255) NULL;
END
GO

IF COL_LENGTH('MSSA_DogFuturityParticipation', 'CreatedDate') IS NULL
BEGIN
    ALTER TABLE MSSA_DogFuturityParticipation ADD CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
END
GO

IF COL_LENGTH('MSSA_DogFuturityParticipation', 'ModifiedDate') IS NULL
BEGIN
    ALTER TABLE MSSA_DogFuturityParticipation ADD ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
END
GO
