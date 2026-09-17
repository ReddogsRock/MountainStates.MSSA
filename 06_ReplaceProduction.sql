--Same full replace as 04_ReplaceFromMerge.sql, but targeting the WinHost production
--database (DB_169191_mtstates) via a linked server instead of the local Oqtane-MSSA.
--Requires a linked server named WINHOST already set up on this local instance,
--pointing at WinHost's SQL Server.
--
--Sources from local Oqtane-MSSA directly (not Oqtane-MSSA-Merge) - Merge was only
--ever used for the original migration round; the 2026-09-16/17 final-import merge
--(08_MergeFinalImport.sql, 09_MergeHandlersAndMemberships2026.sql) went straight into
--local Oqtane-MSSA and was never replayed into Merge.
--
--Before running: check WinHost's hosting control panel for a built-in database
--backup option - shared hosting plans often restrict a direct T-SQL BACKUP DATABASE,
--so that's likely the more reliable way to get a safety net here.
--
--Confirmed: nothing on WinHost's database is worth preserving - full wipe-and-replace.

--============================================================
--Step 1: Delete existing data, children before parents.
--============================================================

DELETE FROM [WINHOST].[DB_169191_mtstates].dbo.MSSA_DogFuturityParticipation;
DELETE FROM [WINHOST].[DB_169191_mtstates].dbo.MSSA_DogOwnershipHistory;
DELETE FROM [WINHOST].[DB_169191_mtstates].dbo.MSSA_MembershipHandlers;
DELETE FROM [WINHOST].[DB_169191_mtstates].dbo.MSSA_Memberships;
DELETE FROM [WINHOST].[DB_169191_mtstates].dbo.MSSA_Entries;
DELETE FROM [WINHOST].[DB_169191_mtstates].dbo.MSSA_EventClassOfferings;
DELETE FROM [WINHOST].[DB_169191_mtstates].dbo.MSSA_Trials;
DELETE FROM [WINHOST].[DB_169191_mtstates].dbo.MSSA_Dogs;
DELETE FROM [WINHOST].[DB_169191_mtstates].dbo.MSSA_Handlers;
DELETE FROM [WINHOST].[DB_169191_mtstates].dbo.MSSA_Events;

--============================================================
--Step 2: Copy the cleaned data across from local Oqtane-MSSA, parents before
--children, preserving the exact IDs.
--============================================================

EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Dogs ON') AT WINHOST;
INSERT INTO [WINHOST].[DB_169191_mtstates].dbo.MSSA_Dogs (DogId, Name, Breed, DateOfBirth, RegistrationNumber, FirstCompetitionYear, OwnerName, OwnerIsMSSAMember, IsDeceased, IsSold, CreatedDate, ModifiedDate, IsActive)
SELECT DogId, Name, Breed, DateOfBirth, RegistrationNumber, FirstCompetitionYear, OwnerName, OwnerIsMSSAMember, IsDeceased, IsSold, CreatedDate, ModifiedDate, IsActive
FROM [Oqtane-MSSA].dbo.MSSA_Dogs;
EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Dogs OFF') AT WINHOST;

--DogFuturityParticipation - not identity-preserved (ParticipationId isn't referenced
--as an FK anywhere else), so no IDENTITY_INSERT needed, just insert after Dogs.
INSERT INTO [WINHOST].[DB_169191_mtstates].dbo.MSSA_DogFuturityParticipation
    (DogId, Year, DocumentFileName, DocumentPath, DocumentUploadedDate, Status, PaymentMethod, Amount, PaidBy, DateReceived, StripePaymentIntentId, CreatedDate, ModifiedDate)
SELECT DogId, Year, DocumentFileName, DocumentPath, DocumentUploadedDate, Status, PaymentMethod, Amount, PaidBy, DateReceived, StripePaymentIntentId, CreatedDate, ModifiedDate
FROM [Oqtane-MSSA].dbo.MSSA_DogFuturityParticipation;

EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Events ON') AT WINHOST;
INSERT INTO [WINHOST].[DB_169191_mtstates].dbo.MSSA_Events (EventId, EventIdentifier, EventName, City, StateCode, StartDate, EndDate, PointYear, ChairmanName, ChairmanPhone, IsMSSASanctioned, CreatedDate, ResultsReceivedDate, ResultsUploaded, SanctionFee, FeeReceivedDate, ResultsApprovalStatus, ResultsApprovedDate, ApprovalStatus, ApprovedDate)
SELECT EventId, EventIdentifier, EventName, City, StateCode, StartDate, EndDate, PointYear, ChairmanName, ChairmanPhone, IsMSSASanctioned, CreatedDate, ResultsReceivedDate, ResultsUploaded, SanctionFee, FeeReceivedDate, ResultsApprovalStatus, ResultsApprovedDate, ApprovalStatus, ApprovedDate
FROM [Oqtane-MSSA].dbo.MSSA_Events;
EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Events OFF') AT WINHOST;

EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Handlers ON') AT WINHOST;
INSERT INTO [WINHOST].[DB_169191_mtstates].dbo.MSSA_Handlers (HandlerId, LastName, FirstName, Email, Phone, AlternatePhone, Address, City, StateCode, HandlerLevel, LevelMoveUpDate, PhotoReleaseConsent, CreatedDate, ModifiedDate, IsActive)
SELECT HandlerId, LastName, FirstName, Email, Phone, AlternatePhone, Address, City, StateCode, HandlerLevel, LevelMoveUpDate, PhotoReleaseConsent, CreatedDate, ModifiedDate, IsActive
FROM [Oqtane-MSSA].dbo.MSSA_Handlers;
EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Handlers OFF') AT WINHOST;

EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Memberships ON') AT WINHOST;
INSERT INTO [WINHOST].[DB_169191_mtstates].dbo.MSSA_Memberships (MembershipId, MembershipType, StartYear, EndYear, Amount, PaidBy, DateReceived, StripePaymentIntentId, CreatedDate, ModifiedDate)
SELECT MembershipId, MembershipType, StartYear, EndYear, Amount, PaidBy, DateReceived, StripePaymentIntentId, CreatedDate, ModifiedDate
FROM [Oqtane-MSSA].dbo.MSSA_Memberships;
EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Memberships OFF') AT WINHOST;

INSERT INTO [WINHOST].[DB_169191_mtstates].dbo.MSSA_MembershipHandlers (MembershipId, HandlerId, IsPrimary)
SELECT MembershipId, HandlerId, IsPrimary
FROM [Oqtane-MSSA].dbo.MSSA_MembershipHandlers;

EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Trials ON') AT WINHOST;
INSERT INTO [WINHOST].[DB_169191_mtstates].dbo.MSSA_Trials (TrialId, EventId, TrialIdentifier, TrialDate, TrialName, Stock)
SELECT TrialId, EventId, TrialIdentifier, TrialDate, TrialName, Stock
FROM [Oqtane-MSSA].dbo.MSSA_Trials;
EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Trials OFF') AT WINHOST;

EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Entries ON') AT WINHOST;
INSERT INTO [WINHOST].[DB_169191_mtstates].dbo.MSSA_Entries (EntryId, TrialId, HandlerId, DogId, ClassId, Placing, RunTime, TieBreakerTime, TrialPoints, HandlerIsMSSAMember, EnteredTotalScore)
SELECT EntryId, TrialId, HandlerId, DogId, ClassId, Placing, RunTime, TieBreakerTime, TrialPoints, HandlerIsMSSAMember, EnteredTotalScore
FROM [Oqtane-MSSA].dbo.MSSA_Entries;
EXEC ('SET IDENTITY_INSERT DB_169191_mtstates.dbo.MSSA_Entries OFF') AT WINHOST;

INSERT INTO [WINHOST].[DB_169191_mtstates].dbo.MSSA_EventClassOfferings (EventId, ClassId, Stock, Venue, PlannedRuns)
SELECT EventId, ClassId, Stock, Venue, PlannedRuns
FROM [Oqtane-MSSA].dbo.MSSA_EventClassOfferings;

--============================================================
--Step 3: Reseed identity counters, run on WinHost itself via EXEC ... AT, since
--DBCC CHECKIDENT can't be issued remotely through a linked server reference.
--============================================================

EXEC ('DBCC CHECKIDENT (''MSSA_Dogs'', RESEED)') AT WINHOST;
EXEC ('DBCC CHECKIDENT (''MSSA_DogFuturityParticipation'', RESEED)') AT WINHOST;
EXEC ('DBCC CHECKIDENT (''MSSA_Events'', RESEED)') AT WINHOST;
EXEC ('DBCC CHECKIDENT (''MSSA_Handlers'', RESEED)') AT WINHOST;
EXEC ('DBCC CHECKIDENT (''MSSA_Memberships'', RESEED)') AT WINHOST;
EXEC ('DBCC CHECKIDENT (''MSSA_MembershipHandlers'', RESEED)') AT WINHOST;
EXEC ('DBCC CHECKIDENT (''MSSA_Trials'', RESEED)') AT WINHOST;
EXEC ('DBCC CHECKIDENT (''MSSA_Entries'', RESEED)') AT WINHOST;

--============================================================
--Step 4: Drop the old boolean planning columns on MSSA_Events - run this in a
--SEPARATE query window connected DIRECTLY to WinHost (not through the WINHOST
--linked server - nesting dynamic SQL inside EXEC(...) AT gets into fragile
--triple-escaped quoting that's not worth the risk). Same cleanup already done on
--local Oqtane-MSSA and Oqtane-MSSA-Merge; confirmed dead, nothing in the app
--code references these columns.
--============================================================

/*
USE [DB_169191_mtstates];

DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql = @sql + 'ALTER TABLE MSSA_Events DROP CONSTRAINT [' + dc.name + '];' + CHAR(13)
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE dc.parent_object_id = OBJECT_ID('MSSA_Events')
  AND c.name IN ('Cattle','Sheep','Arena','Field','OnFoot','Horseback','Open','Nursery','Intermediate','Novice','Junior');

EXEC sp_executesql @sql;

ALTER TABLE MSSA_Events
    DROP COLUMN Cattle, Sheep, Arena, Field, OnFoot, Horseback, [Open], Nursery, Intermediate, Novice, Junior;
*/
