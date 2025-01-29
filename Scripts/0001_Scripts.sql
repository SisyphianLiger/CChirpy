-- Up Migration
CREATE TABLE users (
	id UUID PRIMARY KEY, 			-- Primary key as UUID
	created_at TIMESTAMP NOT NULL,		-- Creation Timestamp
	updated_at TIMESTAMP NOT NULL,		-- Update Timestamp
	email TEXT NOT NULL UNIQUE		-- Unique Email Address
);

