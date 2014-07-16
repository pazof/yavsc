-- Table: profiles

-- DROP TABLE profiles;

CREATE TABLE profiles
(
  uniqueid bigserial NOT NULL,
  username character varying(255) NOT NULL,
  applicationname character varying(255) NOT NULL,
  isanonymous boolean,
  lastactivitydate timestamp with time zone,
  lastupdateddate timestamp with time zone,
  CONSTRAINT profiles_pkey PRIMARY KEY (uniqueid ),
  CONSTRAINT pkprofiles UNIQUE (username , applicationname )
)
WITH (
  OIDS=FALSE
);

