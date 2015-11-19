
-- Table: skill

-- DROP TABLE skill;

CREATE TABLE skill
(
  _id bigserial NOT NULL,
  name character varying(2024) NOT NULL,
  rate integer NOT NULL DEFAULT 50,
  CONSTRAINT skill_pkey PRIMARY KEY (_id),
  CONSTRAINT skill_name_key UNIQUE (name)
)
WITH (
  OIDS=FALSE
);


-- Table: userskills

-- DROP TABLE userskills;

CREATE TABLE userskills
(
  applicationname character varying(512) NOT NULL,
  username character varying(512) NOT NULL,
  comment character varying,
  skillid bigint NOT NULL, -- Skill identifier
  rate integer NOT NULL,
  _id bigserial NOT NULL, -- The id ...
  CONSTRAINT userskills_pkey PRIMARY KEY (applicationname, username, skillid),
  CONSTRAINT userskills_applicationname_fkey FOREIGN KEY (applicationname, username)
      REFERENCES users (applicationname, username) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT userskills_skillid_fkey FOREIGN KEY (skillid)
      REFERENCES skill (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT userskills__id_key UNIQUE (_id)
)
WITH (
  OIDS=FALSE
);
COMMENT ON COLUMN userskills.skillid IS 'Skill identifier';
COMMENT ON COLUMN userskills._id IS 'The id ...';

-- Table: satisfaction

-- DROP TABLE satisfaction;

CREATE TABLE satisfaction
(
  _id bigserial NOT NULL,
  userskillid bigint, -- the user's skill reference
  rate integer, -- The satisfaction rating associated by a client to an user's skill
  comnt character varying(8192), -- The satisfaction textual comment associated by a client to an user's skill, it could be formatted ala Markdown
  CONSTRAINT satisfaction_pkey PRIMARY KEY (_id),
  CONSTRAINT satisfaction_userskillid_fkey FOREIGN KEY (userskillid)
      REFERENCES userskills (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
COMMENT ON COLUMN satisfaction.userskillid IS 'the user''s skill reference';
COMMENT ON COLUMN satisfaction.rate IS 'The satisfaction rating associated by a client to an user''s skill';
COMMENT ON COLUMN satisfaction.comnt IS 'The satisfaction textual comment associated by a client to an user''s skill, it could be formatted ala Markdown';


