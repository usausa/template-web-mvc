CREATE TABLE IF NOT EXISTS AccountPasskey (
    CredentialId  BLOB     NOT NULL,
    AccountId     INTEGER  NOT NULL,
    Data          TEXT     NOT NULL,
    PRIMARY KEY (CredentialId),
    FOREIGN KEY (AccountId) REFERENCES Account(Id) ON DELETE CASCADE
);
