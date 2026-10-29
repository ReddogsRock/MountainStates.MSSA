-- Adds an IsPaid flag to MSSA_Entries so a Trial Secretary can mark a run's entry
-- fee as paid while adding entries (Add Entries screen) and see it in the Entries
-- for This Event list. Defaults existing rows to 0 (unpaid) - there's no way to
-- infer historical payment status from existing data. Safe to re-run.

IF COL_LENGTH('MSSA_Entries', 'IsPaid') IS NULL
BEGIN
    ALTER TABLE MSSA_Entries ADD IsPaid BIT NOT NULL DEFAULT 0;
END
GO
