
-- Table: usersroles

-- DROP TABLE usersroles;

CREATE TABLE usersroles
(
  applicationname character varying(255) NOT NULL,
  rolename character varying(255) NOT NULL,
  username character varying(255) NOT NULL,
  CONSTRAINT attrroles_pkey PRIMARY KEY (applicationname , rolename , username ),
  CONSTRAINT usersroles_fk_role FOREIGN KEY (applicationname, rolename)
      REFERENCES roles (applicationname, rolename) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT usersroles_fk_user FOREIGN KEY (applicationname, username)
      REFERENCES users (applicationname, username) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
