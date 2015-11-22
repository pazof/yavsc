
ALTER TABLE profiledata ADD COLUMN meacode character varying(256);
COMMENT ON COLUMN profiledata.meacode IS 'Code d''activité principale éxercée.';


