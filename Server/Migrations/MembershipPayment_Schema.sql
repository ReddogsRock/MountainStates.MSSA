-- Adds Stripe payment tracking to MSSA_Memberships. Unlike Futurity, membership
-- "paid" status is already signaled by DateReceived being set (see
-- MSSA_HandlerRepository.SearchMembershipsAsync's "PendingPayment" filter) - so this
-- only adds the one new column needed to trace a payment back to its Stripe
-- transaction. Amount/PaidBy/DateReceived are set directly by the webhook using the
-- existing columns. Safe to re-run.

IF COL_LENGTH('MSSA_Memberships', 'StripePaymentIntentId') IS NULL
BEGIN
    ALTER TABLE MSSA_Memberships ADD StripePaymentIntentId VARCHAR(255) NULL;
END
GO
