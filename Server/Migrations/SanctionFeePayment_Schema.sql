-- Adds Stripe payment tracking to MSSA_Events. "Paid" status is already signaled by
-- FeeReceivedDate being set (existing column, previously only ever set manually by an
-- Admin) - this only adds the one new column needed to trace a payment back to its
-- Stripe transaction. SanctionFee/FeeReceivedDate are set directly by the webhook using
-- the existing columns, same pattern as MembershipPayment_Schema.sql. Safe to re-run.

IF COL_LENGTH('MSSA_Events', 'StripePaymentIntentId') IS NULL
BEGIN
    ALTER TABLE MSSA_Events ADD StripePaymentIntentId VARCHAR(255) NULL;
END
GO
