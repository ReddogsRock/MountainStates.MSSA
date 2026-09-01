-- Adds per-Trial Scorekeeper assignment, so a Scorekeeper's access can be scoped to
-- the specific trial(s) they're assigned to instead of being unrestricted.
-- Safe to re-run.

IF COL_LENGTH('MSSA_Trials', 'ScorekeeperUserId') IS NULL
BEGIN
    ALTER TABLE MSSA_Trials ADD ScorekeeperUserId INT NULL;
END
GO
