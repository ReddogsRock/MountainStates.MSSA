-- Merge Deb Conroy's duplicate login accounts.
--
-- Background: the original account (Username 'dconroy') couldn't be recovered via
-- "Forgot Password" because she typed her email instead of her username, so Oqtane's
-- username lookup failed. She then self-registered a new account using her email as
-- the username, creating a second login for the same person.
--
-- Decision (Janet, 2026-10-20): keep the NEW account (Username 'conroyfarm@hotmail.com',
-- UserId 120) since that's the one she's actively using, but give it the OLD account's
-- (UserId 36, 'dconroy') site role(s), then deactivate the old account so it can't be
-- used or confused with the new one again.
--
-- Scope found: UserId 36 has two roles - "Registered Users" (auto-assigned to every
-- registered user, so 120 almost certainly already has it) and "Trial Secretary"
-- (granted 2026-09-18, no expiry - the one that actually matters here). No MSSA_Handler/
-- Membership rows reference a UserId directly (that module's data links by name/email
-- string, not a login FK - see CLAUDE.md), so there is nothing else in this codebase's
-- tables to move. Not touching AspNetUserClaims/Logins/Tokens - normal for these to be
-- empty and they aren't "permissions" in the Oqtane-role sense.
--
-- Run this in SSMS against the WinHost production database after reviewing the SELECTs.
-- Each write is guarded so re-running this script is harmless.

SET NOCOUNT ON;

-- ---------------------------------------------------------------------------
-- STEP 0: Verify the two accounts and the old account's role(s) before changing
-- anything. Confirm UserId 36 = dconroy (old, to retire) and UserId 120 =
-- conroyfarm@hotmail.com (new, to keep) still match what's described above.
-- ---------------------------------------------------------------------------
SELECT UserId, Username, DisplayName, Email, IsDeleted, LastLoginOn, CreatedOn
FROM [User]
WHERE UserId IN (36, 120)
ORDER BY UserId;

SELECT ur.UserId, ur.RoleId, r.Name AS RoleName, ur.EffectiveDate, ur.ExpiryDate
FROM UserRole ur
JOIN [Role] r ON ur.RoleId = r.RoleId
WHERE ur.UserId IN (36, 120)
ORDER BY ur.UserId, r.Name;

-- Stop here and review the two SELECTs above before running anything below.

-- ---------------------------------------------------------------------------
-- STEP 1: Copy every role UserId 36 has onto UserId 120, skipping any role 120
-- already has (so "Registered Users" won't be duplicated if it's already there).
-- ---------------------------------------------------------------------------
INSERT INTO UserRole (UserId, RoleId, EffectiveDate, ExpiryDate, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 120, old.RoleId, old.EffectiveDate, old.ExpiryDate, 'merge-script', GETUTCDATE(), 'merge-script', GETUTCDATE()
FROM UserRole old
WHERE old.UserId = 36
  AND NOT EXISTS (
      SELECT 1 FROM UserRole existing
      WHERE existing.UserId = 120 AND existing.RoleId = old.RoleId
  );

-- ---------------------------------------------------------------------------
-- STEP 2: Retire the old account - soft-delete it in Oqtane's own User table
-- (hides it from Admin > User Management) and lock it out at the ASP.NET
-- Identity level (so the old credentials can no longer be used to sign in).
-- Deliberately NOT hard-deleting AspNetUsers/[User] rows - preserves history
-- (Log entries, CreatedBy audit fields elsewhere) that reference this account.
-- ---------------------------------------------------------------------------
UPDATE [User]
SET IsDeleted = 1,
    DeletedBy = 'merge-script',
    DeletedOn = GETUTCDATE()
WHERE UserId = 36 AND IsDeleted = 0;

UPDATE AspNetUsers
SET LockoutEnabled = 1,
    LockoutEnd = '9999-12-31 23:59:59.9999999 +00:00'
WHERE Id = (SELECT TOP 1 au.Id
            FROM AspNetUsers au
            JOIN [User] u ON u.Username = au.UserName
            WHERE u.UserId = 36);

-- ---------------------------------------------------------------------------
-- STEP 3: Confirm the result.
-- ---------------------------------------------------------------------------
SELECT UserId, Username, IsDeleted, DeletedOn FROM [User] WHERE UserId IN (36, 120);
SELECT ur.UserId, r.Name AS RoleName FROM UserRole ur JOIN [Role] r ON ur.RoleId = r.RoleId WHERE ur.UserId = 120 ORDER BY r.Name;
