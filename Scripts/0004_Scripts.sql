-- Up Migration
CREATE TABLE refresh_tokens (
    token TEXT NOT NULL PRIMARY KEY,        -- The primary key - just a string
    created_at TIMESTAMP NOT NULL,          -- When the token was created
    updated_at TIMESTAMP NOT NULL,          -- When the token was last updated
    user_id UUID NOT NULL,                  -- Foreign key that deletes the row if the user is deleted
    expires_at TIMESTAMP NOT NULL,          -- When the token expires
    revoked_at TIMESTAMP,                   -- When the token was revoked (null if not revoked)
    FOREIGN KEY (user_id) REFERENCES users ON DELETE CASCADE
);
