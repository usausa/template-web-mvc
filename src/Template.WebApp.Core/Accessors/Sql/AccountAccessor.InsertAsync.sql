INSERT INTO Account (Name, NormalizedName, Password, Role, SecurityStamp, AccessFailedCount, LockoutEnd, CreatedAt)
VALUES (/*@ name */'', /*@ normalizedName */'', /*@ password */NULL, /*@ role */'', /*@ securityStamp */'', 0, NULL, /*@ createdAt */'');
SELECT last_insert_rowid();
