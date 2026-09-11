CREATE TABLE IF NOT EXISTS Account (
    Id                 INTEGER  NOT NULL,
    Name               TEXT     NOT NULL,
    NormalizedName     TEXT     NOT NULL,
    Password           BLOB     NOT NULL,
    Role               TEXT     NOT NULL,
    SecurityStamp      TEXT     NOT NULL,
    AccessFailedCount  INTEGER  NOT NULL,
    LockoutEnd         TEXT,
    CreatedAt          TEXT     NOT NULL,
    PRIMARY KEY (Id AUTOINCREMENT),
    UNIQUE (Name),
    UNIQUE (NormalizedName)
);
