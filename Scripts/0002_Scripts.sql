
-- Up Migration
CREATE TABLE chirps (
	id UUID PRIMARY KEY,						-- Primary key as UUID
	created_at TIMESTAMP NOT NULL,					-- Creation Timestamp
	updated_at TIMESTAMP NOT NULL,					-- Update Timestamp
	body TEXT NOT NULL,						-- The Chirpp
	user_id UUID NOT NULL,
	FOREIGN KEY (user_id) REFERENCES users ON DELETE CASCADE 	-- Reference id of user  
);
