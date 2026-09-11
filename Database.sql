-- SQLite
CREATE TABLE IF NOT EXISTS Data (
    Id         INTEGER  NOT NULL,
    Name       TEXT     NOT NULL,
    Value      INTEGER  NOT NULL,
    CreatedAt  TEXT     NOT NULL,
    PRIMARY KEY (Id AUTOINCREMENT),
    UNIQUE (Name)
);

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

CREATE TABLE IF NOT EXISTS AccountPasskey (
    CredentialId  BLOB     NOT NULL,
    AccountId     INTEGER  NOT NULL,
    Data          TEXT     NOT NULL,
    PRIMARY KEY (CredentialId),
    FOREIGN KEY (AccountId) REFERENCES Account(Id) ON DELETE CASCADE
);
