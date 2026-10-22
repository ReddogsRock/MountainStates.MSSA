-- MSSA_Memberships.EndYear was NOT NULL on the database even though the C# model
-- (MSSA_Membership.EndYear) has always declared it as nullable int? - a Lifetime
-- membership has no end year by design (see MSSA_HandlerRepository.ComputeEndYear,
-- which returns null for "Lifetime"). Nobody had ever actually created a Lifetime
-- membership until now, so this mismatch sat dormant until the first attempt threw
-- "Cannot insert the value NULL into column 'EndYear'". Safe to re-run - only acts if
-- the column is still NOT NULL on the target database.

IF EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'MSSA_Memberships' AND c.name = 'EndYear' AND c.is_nullable = 0
)
BEGIN
    ALTER TABLE MSSA_Memberships ALTER COLUMN EndYear INT NULL;
END
GO
