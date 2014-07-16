-- Table: profiledata

-- DROP TABLE profiledata;

CREATE TABLE profiledata
(
  uniqueid integer,
  zipcode character varying(10),
  cityandstate character varying(255),
  avatar bytea,
  CONSTRAINT fkprofiles2 FOREIGN KEY (uniqueid)
      REFERENCES profiles (uniqueid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);

-- Index: fki_fkprofiles2

-- DROP INDEX fki_fkprofiles2;

CREATE INDEX fki_fkprofiles2
  ON profiledata
  USING btree
  (uniqueid );


