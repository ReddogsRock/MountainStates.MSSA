-- Flags Pro Novice as non-sanctioned and adds a second non-sanctioned class, "Misc
-- Unsanctioned" (On-foot + Horseback), for organizers who need to collect the
-- per-run insurance fee without the run counting toward points/standings.
--
-- Run AFTER Server/Migrations/ClassSanctioning_Schema.sql (adds the IsSanctioned
-- column this script sets). Safe to re-run: the UPDATE is idempotent by nature, and
-- the INSERT is guarded so it won't create duplicate rows on a second run.

SET NOCOUNT ON;

UPDATE MSSA_Classes
SET IsSanctioned = 0
WHERE ClassName = 'Pro Novice';

IF NOT EXISTS (SELECT 1 FROM MSSA_Classes WHERE ClassName = 'Misc Unsanctioned')
BEGIN
    INSERT INTO MSSA_Classes (ClassName, SubClassName, PointsAccumulateByDogOnly, PrintOrder, IsActive, IsSanctioned)
    VALUES
        ('Misc Unsanctioned', 'On-foot', 0, 17, 1, 0),
        ('Misc Unsanctioned', 'Horseback', 0, 18, 1, 0);
END

-- Verify
SELECT ClassId, ClassName, SubClassName, PointsAccumulateByDogOnly, PrintOrder, IsActive, IsSanctioned
FROM MSSA_Classes
ORDER BY PrintOrder, ClassId;
