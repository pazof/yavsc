-- Table: activity

-- DROP TABLE activity;

CREATE TABLE activity
(
  meacode character varying(512) NOT NULL, -- Identifiant de l'activité, à terme, il faudrait ajouter un champ à cette id: le code pays....
  title character varying(2048) NOT NULL, -- Description textuelle du code APE
  applicationname character varying(255) NOT NULL,
  cmnt character varying, -- a long description for this activity
  CONSTRAINT activity_pkey PRIMARY KEY (meacode, applicationname)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE activity
  OWNER TO yavscdev;
COMMENT ON TABLE activity
  IS 'Activités prises en charge par l''application désignée';
COMMENT ON COLUMN activity.meacode IS 'Identifiant de l''activité, à terme, il faudrait ajouter un champ à cette id: le code pays.

Definition francaise: 
un code NACE sur les quatre première lettre (code européen),
une lettre en cinquième position.

Exemple:  ''71.12B''  =>  "Ingénierie, études techniques"
';
COMMENT ON COLUMN activity.title IS 'Description textuelle du code APE';
COMMENT ON COLUMN activity.cmnt IS 'a long description for this activity';

