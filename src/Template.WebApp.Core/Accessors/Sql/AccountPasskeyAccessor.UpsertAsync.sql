INSERT INTO AccountPasskey (CredentialId, AccountId, Data)
VALUES (/*@ credentialId */NULL, /*@ accountId */1, /*@ data */'')
ON CONFLICT (CredentialId) DO UPDATE SET Data = excluded.Data
