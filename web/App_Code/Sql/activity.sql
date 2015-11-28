DROP TABLE satisfaction;
DROP TABLE userskills;
DROP TABLE skill;
DROP TABLE activity;



CREATE TABLE activity
(
  title character varying(2048) NOT NULL, -- Description textuelle du code APE
  applicationname character varying(255) NOT NULL,
  cmnt character varying, -- a long description for this activity
  meacode character varying(512) NOT NULL, -- Identifiant de l'activité, à terme, il faudrait ajouter un champ à cette id: le code pays....
  photo character varying(512) NOT NULL DEFAULT 'none'::character varying, -- a photo url, as a front image for this activity
  CONSTRAINT activity_pkey PRIMARY KEY (meacode, applicationname)
)
WITH (
  OIDS=FALSE
);
COMMENT ON TABLE activity
  IS 'Activités prises en charge par l''application désignée';
COMMENT ON COLUMN activity.title IS 'Description textuelle du code APE';
COMMENT ON COLUMN activity.cmnt IS 'a long description for this activity';
COMMENT ON COLUMN activity.meacode IS 'Identifiant de l''activité, à terme, il faudrait ajouter un champ à cette id: le code pays.

Definition francaise: 
un code NACE sur les quatre première lettre (code européen),
une lettre en cinquième position.

Exemple:  ''71.12B''  =>  "Ingénierie, études techniques"
';
COMMENT ON COLUMN activity.photo IS 'a photo url, as a front image for this activity';


INSERT INTO activity(
            title, applicationname, cmnt, meacode, photo)
    VALUES (
'Édition de logiciels applicatifs',
'/',
'Vente d''applications logicielles',
'6829C',
'http://www.janua.fr/wp-content/uploads/2014/02/born2code-xavier-niel-developpeurs-formation.jpg'
);

INSERT INTO activity(
            title, applicationname, cmnt, meacode, photo)
    VALUES (
'Artiste',
'/',
'Anime votre mariage, un anniversaire ou autre événnement.',
'Artiste',
'http://www.dancefair.tv/wp-content/uploads/2015/05/How-to-secure-DJ-gig.jpg'
);


-- Table: skill

-- DROP TABLE skill;

CREATE TABLE skill
(
  _id bigserial NOT NULL,
  name character varying(2024) NOT NULL,
  rate integer NOT NULL DEFAULT 50,
  meacode character varying(256) NOT NULL,
  applicationname character varying(255),
  CONSTRAINT skill_pkey PRIMARY KEY (_id),
  CONSTRAINT skill_app FOREIGN KEY (applicationname, meacode)
      REFERENCES activity (applicationname, meacode) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT skill_name_meacode_applicationname_key UNIQUE (name, meacode, applicationname)
)
WITH (
  OIDS=FALSE
);

-- Index: fki_skill_app

-- DROP INDEX fki_skill_app;

CREATE INDEX fki_skill_app
  ON skill
  USING btree
  (applicationname COLLATE pg_catalog."default", meacode COLLATE pg_catalog."default");



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

ALTER TABLE satisfaction OWNER TO lua;
ALTER TABLE userskills OWNER TO lua;
ALTER TABLE skill OWNER TO lua;
ALTER TABLE activity OWNER TO lua;


