
-- Table: stocksymbols

-- DROP TABLE stocksymbols;

CREATE TABLE stocksymbols
(
  uniqueid integer,
  stocksymbol character varying(10),
  CONSTRAINT fkprofiles1 FOREIGN KEY (uniqueid)
      REFERENCES profiles (uniqueid) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);
