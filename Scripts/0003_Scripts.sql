-- Adding Hashed Password Table
ALTER TABLE users ADD hashed_password TEXT NOT NULL DEFAULT 'unset';
