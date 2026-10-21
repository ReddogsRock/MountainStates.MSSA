-- Adds IsSanctioned to MSSA_Classes - non-sanctioned classes (Pro Novice, Misc
-- Unsanctioned) never accumulate competition points. Safe to re-run - only acts if the
-- column doesn't already exist on the target database. Defaults every existing class to
-- sanctioned (1); see 25_AddMiscUnsanctionedClassAndFlagProNovice.sql for flipping Pro
-- Novice to 0 and adding the new class.

IF COL_LENGTH('MSSA_Classes', 'IsSanctioned') IS NULL
BEGIN
    ALTER TABLE MSSA_Classes ADD IsSanctioned BIT NOT NULL CONSTRAINT DF_MSSA_Classes_IsSanctioned DEFAULT 1;
END
GO
