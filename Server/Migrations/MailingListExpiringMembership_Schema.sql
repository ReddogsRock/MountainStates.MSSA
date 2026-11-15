-- Flags subscribers whose membership is expiring this calendar year, refreshed by
-- MSSA_MailingListRepository.SyncExpiringMembershipSegmentAsync (an admin-triggered
-- "Sync Expiring Members" action, not automatic) - a reusable annual segment rather
-- than a one-off export, so it can be re-run each renewal cycle. Safe to re-run.

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('MSSA_MailingListSubscribers') AND name = 'IsExpiringMembership'
)
BEGIN
    ALTER TABLE MSSA_MailingListSubscribers ADD IsExpiringMembership BIT NOT NULL DEFAULT 0;
END
GO
