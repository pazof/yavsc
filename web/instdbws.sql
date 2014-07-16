
CREATE TABLE users
(
  pkid character varying NOT NULL,
  username character varying(255) NOT NULL,
  applicationname character varying(255) NOT NULL,
  email character varying(128) NOT NULL,
  comment character varying(255),
  passw character varying(128) NOT NULL,
  passwordquestion character varying(255),
  passwordanswer character varying(255),
  isapproved boolean,
  lastactivitydate date,
  lastlogindate date,
  lastpasswordchangeddate date,
  creationdate date,
  isonline boolean,
  islockedout boolean,
  lastlockedoutdate date,
  failedpasswordattemptcount integer,
  failedpasswordattemptwindowstart date,
  failedpasswordanswerattemptcount integer,
  failedpasswordanswerattemptwindowstart date,
  CONSTRAINT users_pkey PRIMARY KEY (pkid ),
  CONSTRAINT uniquelogin UNIQUE (applicationname, email ),
  CONSTRAINT uniquemail UNIQUE (applicationname, username )
)
WITH (
  OIDS=FALSE
);

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

-- Table: usersroles

-- DROP TABLE usersroles;

CREATE TABLE usersroles
(
  applicationname character varying (255) NOT NULL,
  rolename character varying (255)  NOT NULL,
  username character varying (255)  NOT NULL,
  CONSTRAINT attrroles_pkey PRIMARY KEY (applicationname, rolename , username ),
  CONSTRAINT usersroles_fk_role FOREIGN KEY (applicationname,rolename)
      REFERENCES roles (applicationname,rolename) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT usersroles_fk_user FOREIGN KEY (applicationname,username)
      REFERENCES users (applicationname,username) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
