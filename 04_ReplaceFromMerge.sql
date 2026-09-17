--Replaces all data in the live Oqtane-MSSA's Dogs/Events/Handlers/Trials/Entries/
--EventClassOfferings/Memberships/MembershipHandlers with the cleaned data sitting in
--Oqtane-MSSA-Merge. This is a full replace, not a merge - everything currently in
--these tables (including live activity since the original import: Stripe Futurity
--payments, ownership transfer history, membership-to-handler links) gets deleted
--first. Confirmed intentional - none of that is real production data yet.
--
--Run each section in order. Recommended: run Step 0 (backup) first no matter what.

--============================================================
--Step 0: Backup the live database before doing anything destructive.
--============================================================

BACKUP DATABASE [Oqtane-MSSA]
TO DISK = 'C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\Backup\Oqtane-MSSA-PreReplace.bak'
WITH INIT;

--============================================================
--Step 1: Delete existing data, children before parents.
--Also clears MSSA_DogFuturityParticipation, MSSA_DogOwnershipHistory,
--MSSA_MembershipHandlers, and MSSA_Memberships itself, since none of that is real
--production data yet (Stripe integration is still being built/tested) - confirmed
--fine to delete rather than reconcile against the new Dog/Handler IDs.
--============================================================

DELETE FROM [Oqtane-MSSA].dbo.MSSA_DogFuturityParticipation;
DELETE FROM [Oqtane-MSSA].dbo.MSSA_DogOwnershipHistory;
DELETE FROM [Oqtane-MSSA].dbo.MSSA_MembershipHandlers;
DELETE FROM [Oqtane-MSSA].dbo.MSSA_Memberships;
DELETE FROM [Oqtane-MSSA].dbo.MSSA_Entries;
DELETE FROM [Oqtane-MSSA].dbo.MSSA_EventClassOfferings;
DELETE FROM [Oqtane-MSSA].dbo.MSSA_Trials;
DELETE FROM [Oqtane-MSSA].dbo.MSSA_Dogs;
DELETE FROM [Oqtane-MSSA].dbo.MSSA_Handlers;
DELETE FROM [Oqtane-MSSA].dbo.MSSA_Events;

--============================================================
--Step 2: Copy the cleaned data across from Oqtane-MSSA-Merge, parents before
--children, preserving the exact IDs so internal relationships (Entries -> Trial/
--Handler/Dog, Trials -> Event, EventClassOfferings -> Event) stay correct.
--============================================================

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Dogs ON;
INSERT INTO [Oqtane-MSSA].dbo.MSSA_Dogs (DogId, Name, Breed, DateOfBirth, RegistrationNumber, FirstCompetitionYear, OwnerName, OwnerIsMSSAMember, IsDeceased, IsSold, CreatedDate, ModifiedDate, IsActive)
SELECT DogId, Name, Breed, DateOfBirth, RegistrationNumber, FirstCompetitionYear, OwnerName, OwnerIsMSSAMember, IsDeceased, IsSold, CreatedDate, ModifiedDate, IsActive
FROM [Oqtane-MSSA-Merge].dbo.MSSA_Dogs;
SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Dogs OFF;

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Events ON;
INSERT INTO [Oqtane-MSSA].dbo.MSSA_Events (EventId, EventIdentifier, EventName, City, StateCode, StartDate, EndDate, PointYear, ChairmanName, ChairmanPhone, IsMSSASanctioned, CreatedDate, ResultsReceivedDate, ResultsUploaded, SanctionFee, FeeReceivedDate, ResultsApprovalStatus, ResultsApprovedDate, ApprovalStatus, ApprovedDate)
SELECT EventId, EventIdentifier, EventName, City, StateCode, StartDate, EndDate, PointYear, ChairmanName, ChairmanPhone, IsMSSASanctioned, CreatedDate, ResultsReceivedDate, ResultsUploaded, SanctionFee, FeeReceivedDate, ResultsApprovalStatus, ResultsApprovedDate, ApprovalStatus, ApprovedDate
FROM [Oqtane-MSSA-Merge].dbo.MSSA_Events;
SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Events OFF;

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Handlers ON;
INSERT INTO [Oqtane-MSSA].dbo.MSSA_Handlers (HandlerId, LastName, FirstName, Email, Phone, AlternatePhone, Address, City, StateCode, HandlerLevel, LevelMoveUpDate, PhotoReleaseConsent, CreatedDate, ModifiedDate, IsActive)
SELECT HandlerId, LastName, FirstName, Email, Phone, AlternatePhone, Address, City, StateCode, HandlerLevel, LevelMoveUpDate, PhotoReleaseConsent, CreatedDate, ModifiedDate, IsActive
FROM [Oqtane-MSSA-Merge].dbo.MSSA_Handlers;
SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Handlers OFF;

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Memberships ON;
INSERT INTO [Oqtane-MSSA].dbo.MSSA_Memberships (MembershipId, MembershipType, StartYear, EndYear, Amount, PaidBy, DateReceived, StripePaymentIntentId, CreatedDate, ModifiedDate)
SELECT MembershipId, MembershipType, StartYear, EndYear, Amount, PaidBy, DateReceived, StripePaymentIntentId, CreatedDate, ModifiedDate
FROM [Oqtane-MSSA-Merge].dbo.MSSA_Memberships;
SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Memberships OFF;

-- MembershipHandlerId isn't referenced anywhere else as a FK, so no need to preserve
-- its exact value - just needs MembershipId/HandlerId to line up correctly.
INSERT INTO [Oqtane-MSSA].dbo.MSSA_MembershipHandlers (MembershipId, HandlerId, IsPrimary)
SELECT MembershipId, HandlerId, IsPrimary
FROM [Oqtane-MSSA-Merge].dbo.MSSA_MembershipHandlers;

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Trials ON;
INSERT INTO [Oqtane-MSSA].dbo.MSSA_Trials (TrialId, EventId, TrialIdentifier, TrialDate, TrialName, Stock)
SELECT TrialId, EventId, TrialIdentifier, TrialDate, TrialName, Stock
FROM [Oqtane-MSSA-Merge].dbo.MSSA_Trials;
SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Trials OFF;

SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Entries ON;
INSERT INTO [Oqtane-MSSA].dbo.MSSA_Entries (EntryId, TrialId, HandlerId, DogId, ClassId, Placing, RunTime, TieBreakerTime, TrialPoints, HandlerIsMSSAMember)
SELECT EntryId, TrialId, HandlerId, DogId, ClassId, Placing, RunTime, TieBreakerTime, TrialPoints, HandlerIsMSSAMember
FROM [Oqtane-MSSA-Merge].dbo.MSSA_Entries;
SET IDENTITY_INSERT [Oqtane-MSSA].dbo.MSSA_Entries OFF;

INSERT INTO [Oqtane-MSSA].dbo.MSSA_EventClassOfferings (EventId, ClassId, Stock, Venue, PlannedRuns)
SELECT EventId, ClassId, Stock, Venue, PlannedRuns
FROM [Oqtane-MSSA-Merge].dbo.MSSA_EventClassOfferings;

--============================================================
--Step 3: Reseed identity counters so the next row created live through the app
--gets a fresh, non-colliding ID instead of continuing from wherever it left off
--before the replace.
--============================================================

DBCC CHECKIDENT ('[Oqtane-MSSA].dbo.MSSA_Dogs', RESEED);
DBCC CHECKIDENT ('[Oqtane-MSSA].dbo.MSSA_Events', RESEED);
DBCC CHECKIDENT ('[Oqtane-MSSA].dbo.MSSA_Handlers', RESEED);
DBCC CHECKIDENT ('[Oqtane-MSSA].dbo.MSSA_Memberships', RESEED);
DBCC CHECKIDENT ('[Oqtane-MSSA].dbo.MSSA_MembershipHandlers', RESEED);
DBCC CHECKIDENT ('[Oqtane-MSSA].dbo.MSSA_Trials', RESEED);
DBCC CHECKIDENT ('[Oqtane-MSSA].dbo.MSSA_Entries', RESEED);
