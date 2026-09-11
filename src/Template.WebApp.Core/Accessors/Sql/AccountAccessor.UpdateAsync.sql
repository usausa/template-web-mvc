UPDATE Account
SET Name = /*@ name */'',
    NormalizedName = /*@ normalizedName */'',
    Password = /*@ password */NULL,
    Role = /*@ role */'',
    SecurityStamp = /*@ securityStamp */'',
    AccessFailedCount = /*@ accessFailedCount */0,
    LockoutEnd = /*@ lockoutEnd */NULL
WHERE Id = /*@ id */1
