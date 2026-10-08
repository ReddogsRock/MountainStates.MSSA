-- Adds a Status column to MSSA_Memberships so Admin can flag a Stripe attempt that
-- didn't go through as Failed - distinct from a plain not-yet-attempted pending
-- membership. NULL means "no flag": Paid/Pending is still determined by DateReceived
-- as before (see SearchMembershipsAsync) - the only meaningful stored value here is
-- 'Failed'. Mirrors MSSA_DogFuturityParticipation.Status, just narrower since
-- Membership already had its own Paid signal. Safe to re-run.

IF COL_LENGTH('MSSA_Memberships', 'Status') IS NULL
BEGIN
    ALTER TABLE MSSA_Memberships ADD Status VARCHAR(20) NULL;
END
GO
