-- Table: oauthtoken

-- DROP TABLE oauthtoken;

CREATE TABLE oauthtoken
(
  _id bigserial NOT NULL,
  profileid bigint NOT NULL,
  authtype character varying(255) NOT NULL,
  expiration timestamp with time zone NOT NULL,
  refresh character varying(2048) NOT NULL, -- refresh token
  access character varying(2048) NOT NULL, -- access token
  CONSTRAINT oauthtoken_pkey PRIMARY KEY (_id),
  CONSTRAINT fk_oauth_profileid FOREIGN KEY (profileid)
      REFERENCES profiles (uniqueid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
COMMENT ON COLUMN oauthtoken.refresh IS 'refresh token';
COMMENT ON COLUMN oauthtoken.access IS 'access token';


-- Index: fki_oauth_profileid

-- DROP INDEX fki_oauth_profileid;

CREATE INDEX fki_oauth_profileid
  ON oauthtoken
  USING btree
  (profileid);

