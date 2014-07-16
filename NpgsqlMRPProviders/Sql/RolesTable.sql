-- Table: roles

-- DROP TABLE roles;

CREATE TABLE roles
(
  rolename character varying(255) NOT NULL,
  applicationname character varying(255) NOT NULL,
  comment character varying(255) NOT NULL,
  CONSTRAINT roles_pkey PRIMARY KEY (rolename , applicationname )
)
WITH (
  OIDS=FALSE
);

COMMENT ON TABLE roles
  IS 'Web application roles';

