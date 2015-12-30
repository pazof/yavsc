		
  -- Table: users

-- DROP TABLE users;

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
  lastactivitydate timestamp with time zone,
  lastlogindate timestamp with time zone,
  lastpasswordchangeddate timestamp with time zone,
  creationdate timestamp with time zone,
  islockedout boolean,
  lastlockedoutdate timestamp with time zone,
  failedpasswordattemptcount integer,
  failedpasswordattemptwindowstart timestamp with time zone,
  failedpasswordanswerattemptcount integer,
  failedpasswordanswerattemptwindowstart timestamp with time zone,
  CONSTRAINT users_pkey PRIMARY KEY (pkid),
  CONSTRAINT users_applicationname_fkey FOREIGN KEY (applicationname, username)
      REFERENCES profiles (applicationname, username) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT uniquelogin UNIQUE (applicationname, email),
  CONSTRAINT uniquemail UNIQUE (applicationname, username)
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
  CONSTRAINT roles_pkey PRIMARY KEY (rolename, applicationname)
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
  applicationname character varying(255) NOT NULL,
  rolename character varying(255) NOT NULL,
  username character varying(255) NOT NULL,
  CONSTRAINT attrroles_pkey PRIMARY KEY (applicationname, rolename, username),
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

-- DROP TABLE profiles;

CREATE TABLE profiles
(
  uniqueid bigserial NOT NULL,
  username character varying(255) NOT NULL,
  applicationname character varying(255) NOT NULL,
  isanonymous boolean,
  lastactivitydate timestamp with time zone,
  lastupdateddate timestamp with time zone,
  CONSTRAINT profiles_pkey PRIMARY KEY (uniqueid),
  CONSTRAINT pkprofiles UNIQUE (username, applicationname)
)
WITH (
  OIDS=FALSE
  );

  -- Table: profiledata

-- DROP TABLE profiledata;

CREATE TABLE profiledata
(
  uniqueid integer,
  zipcode character varying(10),
  cityandstate character varying(255),
  blogtitle character varying(255), -- Blog Title
  address character varying(2048), -- Postal address
  country character varying(100),
  website character varying(256),
  blogvisible boolean,
  name character varying(1024),
  phone character varying(15),
  mobile character varying(15),
  accountnumber character varying(15), -- Numero de compte
  bankedkey integer, -- clé RIB
  bankcode character varying(5), -- Code banque
  wicketcode character varying(5),
  iban character varying(33),
  bic character varying(15),
  gtoken character varying(512), -- Google authentification token
  grefreshtoken character varying(512), -- Google refresh token
  gtokentype character varying(256), -- Google access token type
  gcalid character varying(255), -- Google calendar identifier
  gtokenexpir timestamp with time zone NOT NULL DEFAULT now(), -- Google access token expiration date
  avatar character varying(512), -- url for an avatar
  gcalapi boolean NOT NULL DEFAULT false,
  gregid character varying(1024), -- Google Cloud Message registration identifier
  allowcookies boolean NOT NULL DEFAULT false,
  uitheme character varying(64),
  CONSTRAINT fkprofiles2 FOREIGN KEY (uniqueid)
      REFERENCES profiles (uniqueid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
COMMENT ON COLUMN profiledata.blogtitle IS 'Blog Title';
COMMENT ON COLUMN profiledata.address IS 'Postal address';
COMMENT ON COLUMN profiledata.accountnumber IS 'Numero de compte';
COMMENT ON COLUMN profiledata.bankedkey IS 'clé RIB';
COMMENT ON COLUMN profiledata.bankcode IS 'Code banque';
COMMENT ON COLUMN profiledata.gtoken IS 'Google authentification token';
COMMENT ON COLUMN profiledata.grefreshtoken IS 'Google refresh token';
COMMENT ON COLUMN profiledata.gtokentype IS 'Google access token type';
COMMENT ON COLUMN profiledata.gcalid IS 'Google calendar identifier';
COMMENT ON COLUMN profiledata.gtokenexpir IS 'Google access token expiration date';
COMMENT ON COLUMN profiledata.avatar IS 'url for an avatar';
COMMENT ON COLUMN profiledata.gregid IS 'Google Cloud Message registration identifier';


-- Index: fki_fkprofiles2

-- DROP INDEX fki_fkprofiles2;

CREATE INDEX fki_fkprofiles2
  ON profiledata
  USING btree
  (uniqueid);






  -- Table: profiles


  -- Table: blog

-- DROP TABLE blog;

CREATE TABLE blog
(
  applicationname character varying(255) NOT NULL,
  username character varying(255) NOT NULL,
  posted timestamp with time zone NOT NULL,
  modified timestamp with time zone NOT NULL,
  title character varying(512) NOT NULL,
  bcontent text NOT NULL,
  visible boolean NOT NULL,
  _id bigserial NOT NULL,
  photo character varying(512), -- a photo url, supposed to be the main photo...
  rate integer NOT NULL DEFAULT 50, -- a global note for this entry, between 0 and 100
  CONSTRAINT blog_pkey PRIMARY KEY (_id),
  CONSTRAINT bloguser FOREIGN KEY (applicationname, username)
      REFERENCES users (applicationname, username) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);

COMMENT ON COLUMN blog.photo IS 'a photo url, supposed to be the main photo
related to this post';
COMMENT ON COLUMN blog.note IS 'a global note for this entry, between 0 and 100';

CREATE TABLE blfiles
(
  _id bigserial NOT NULL, -- Identifier
  name character varying(2048), -- File Name, relative to the user home directory, must not begin with a slash.
  alt_text character varying(512),
  CONSTRAINT bltags_pkey PRIMARY KEY (_id)
)
WITH (
  OIDS=FALSE
);
COMMENT ON COLUMN blfiles._id IS 'Identifier';
COMMENT ON COLUMN blfiles.name IS 'File Name, relative to the user home directory, must not begin with a slash.';


  -- Table: commandes

-- DROP TABLE commandes;

CREATE TABLE commandes
(
  id bigserial NOT NULL, -- Identifiant unique de commande e, cours
  validation date, -- Date de validation
  prdref character varying(255) NOT NULL, -- Product reference from the unique valid catalog at the validation date
  creation timestamp with time zone NOT NULL DEFAULT now(), -- creation date
  clientname character varying(255), -- user who created the command, client of this command
  applicationname character varying(255), -- application concerned by this command
  class character varying(512), -- Classe de commande:...
  params jsonb,
  CONSTRAINT commandes_pkey PRIMARY KEY (id),
  CONSTRAINT commforeignuser FOREIGN KEY (clientname, applicationname)
      REFERENCES users (username, applicationname) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
ALTER TABLE commandes
  OWNER TO yavscdev;
COMMENT ON COLUMN commandes.id IS 'Identifiant unique de commande e, cours';
COMMENT ON COLUMN commandes.validation IS 'Date de validation';
COMMENT ON COLUMN commandes.prdref IS 'Product reference from the unique valid catalog at the validation date';
COMMENT ON COLUMN commandes.creation IS 'creation date';
COMMENT ON COLUMN commandes.clientname IS 'user who created the command, client of this command';
COMMENT ON COLUMN commandes.applicationname IS 'application concerned by this command';
COMMENT ON COLUMN commandes.class IS 'Classe de commande:
Utilisé pour l''instanciation de l''objet du SI, 
le nom du contrôle Html, et 
determiner les fournisseurs du Workflow
à mettre en oeuvre pour traiter la commande.';


-- Index: fki_commforeignuser

-- DROP INDEX fki_commforeignuser;

CREATE INDEX fki_commforeignuser
  ON commandes
  USING btree
  (clientname COLLATE pg_catalog."default", applicationname COLLATE pg_catalog."default");



  -- Table: comment

-- DROP TABLE comment;

CREATE TABLE comment
(
  username character varying(255) NOT NULL,
  bcontent text NOT NULL,
  posted timestamp with time zone NOT NULL,
  modified timestamp with time zone NOT NULL,
  visible boolean NOT NULL,
  _id bigserial NOT NULL,
  postid bigint,
  applicationname character varying(255),
  CONSTRAINT comment_pkey PRIMARY KEY (_id),
  CONSTRAINT fkey_blog FOREIGN KEY (postid)
      REFERENCES blog (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fkey_users FOREIGN KEY (username, applicationname)
      REFERENCES users (username, applicationname) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
-- Index: fki_fkey_blog

-- DROP INDEX fki_fkey_blog;

CREATE INDEX fki_fkey_blog
  ON comment
  USING btree
  (postid);

-- Index: fki_fkey_users

-- DROP INDEX fki_fkey_users;

CREATE INDEX fki_fkey_users
  ON comment
  USING btree
  (username COLLATE pg_catalog."default", applicationname COLLATE pg_catalog."default");



  -- Table: estimate

-- DROP TABLE estimate;

CREATE TABLE estimate
(
  _id bigserial NOT NULL, -- identifier
  title character varying(1024) NOT NULL,
  username character varying(255) NOT NULL, -- User name of the owner and creator for this estimate
  applicationname character varying(255) NOT NULL,
  status integer,
  client character varying(255) NOT NULL, -- a login name
  description character varying(65000),
  CONSTRAINT estimate_pkey PRIMARY KEY (_id),
  CONSTRAINT estimate_client_fkey FOREIGN KEY (client, applicationname)
      REFERENCES users (username, applicationname) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT estimate_username_fkey FOREIGN KEY (username, applicationname)
      REFERENCES users (username, applicationname) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
COMMENT ON COLUMN estimate._id IS 'identifier';
COMMENT ON COLUMN estimate.username IS 'User name of the owner and creator for this estimate';
COMMENT ON COLUMN estimate.client IS 'a login name';


-- Index: cliuser

-- DROP INDEX cliuser;

CREATE INDEX cliuser
  ON estimate
  USING btree
  (client COLLATE pg_catalog."default", applicationname COLLATE pg_catalog."default");



  -- Table: histoestim

-- DROP TABLE histoestim;

CREATE TABLE histoestim
(
  datechange timestamp with time zone NOT NULL DEFAULT now(),
  status integer,
  estid bigint NOT NULL,
  username character varying(255),
  applicationname character varying(255),
  _id bigserial NOT NULL,
  CONSTRAINT histoestim_pkey PRIMARY KEY (_id),
  CONSTRAINT histoestim_estid_fkey FOREIGN KEY (estid)
      REFERENCES estimate (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT histoestim_username_fkey FOREIGN KEY (username, applicationname)
      REFERENCES users (username, applicationname) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);



  -- Table: hr

-- DROP TABLE hr;

CREATE TABLE hr
(
  userid character varying NOT NULL,
  rate numeric NOT NULL,
  comment text NOT NULL,
  CONSTRAINT hr_pk_new PRIMARY KEY (userid)
)
WITH (
  OIDS=FALSE
);
  -- Table: product

-- DROP TABLE product;

CREATE TABLE product
(
  ref character varying(255) NOT NULL, -- Product reference from the catalog
  "Name" character varying(1000), -- Product Name
  "Description" character varying(64000), -- Product description
  CONSTRAINT product_pkey PRIMARY KEY (ref)
)
WITH (
  OIDS=FALSE
);
COMMENT ON COLUMN product.ref IS 'Product reference from the catalog';
COMMENT ON COLUMN product."Name" IS 'Product Name';
COMMENT ON COLUMN product."Description" IS 'Product description';






  -- Table: projet

-- DROP TABLE projet;

CREATE TABLE projet
(
  id bigserial NOT NULL,
  name character varying NOT NULL,
  prdesc text,
  managerid character varying NOT NULL,
  "ApplicationName" character varying(250),
  CONSTRAINT projet_pk_new PRIMARY KEY (id),
  CONSTRAINT pk_project_manager FOREIGN KEY (managerid, "ApplicationName")
      REFERENCES users (username, applicationname) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);

-- Index: fki_pk_project_manager

-- DROP INDEX fki_pk_project_manager;

CREATE INDEX fki_pk_project_manager
  ON projet
  USING btree
  (managerid COLLATE pg_catalog."default", "ApplicationName" COLLATE pg_catalog."default");

  
  -- Table: stocksymbols

-- DROP TABLE stocksymbols;

CREATE TABLE stocksymbols
(
  uniqueid integer,
  stocksymbol character varying(10),
  CONSTRAINT fkprofiles1 FOREIGN KEY (uniqueid)
      REFERENCES profiles (uniqueid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
  -- Table: tag

-- DROP TABLE tag;

CREATE TABLE tag
(
  _id bigserial NOT NULL, -- Identifier
  name character varying(30), -- Tag name
  CONSTRAINT tag_pkey PRIMARY KEY (_id),
  CONSTRAINT tag_name_key UNIQUE (name)
)
WITH (
  OIDS=FALSE
);

COMMENT ON COLUMN tag._id IS 'Identifier';
COMMENT ON COLUMN tag.name IS 'Tag name';




  -- Table: tagged

-- DROP TABLE tagged;

CREATE TABLE tagged
(
  postid bigint NOT NULL,
  tagid bigint NOT NULL,
  CONSTRAINT tagged_pkey PRIMARY KEY (postid, tagid),
  CONSTRAINT tagged_postid_fkey FOREIGN KEY (postid)
      REFERENCES blog (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT tagged_tagid_fkey FOREIGN KEY (tagid)
      REFERENCES tag (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);


  -- Table: tasks

-- DROP TABLE tasks;

CREATE TABLE tasks
(
  id bigserial NOT NULL,
  name character varying NOT NULL,
  start date NOT NULL,
  endd date NOT NULL,
  tdesc text NOT NULL,
  prid bigint NOT NULL,
  CONSTRAINT tasks_pk_new PRIMARY KEY (id),
  CONSTRAINT tasks_fk_new FOREIGN KEY (prid)
      REFERENCES projet (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);


  -- Table: taskdeps

-- DROP TABLE taskdeps;

CREATE TABLE taskdeps
(
  "taskId" bigint NOT NULL, -- dependent task
  deptask bigint NOT NULL, -- Dependency
  CONSTRAINT pk_tasks_deps PRIMARY KEY ("taskId", deptask),
  CONSTRAINT pk_foreign_dep FOREIGN KEY (deptask)
      REFERENCES tasks (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT pk_foreign_task FOREIGN KEY ("taskId")
      REFERENCES tasks (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
COMMENT ON TABLE taskdeps
  IS 'Dependencies between tasks';
COMMENT ON COLUMN taskdeps."taskId" IS 'dependent task';
COMMENT ON COLUMN taskdeps.deptask IS 'Dependency';


-- Index: fki_pk_foreign_dep

-- DROP INDEX fki_pk_foreign_dep;

CREATE INDEX fki_pk_foreign_dep
  ON taskdeps
  USING btree
  (deptask);

-- Index: fki_pk_foreign_task

-- DROP INDEX fki_pk_foreign_task;

CREATE INDEX fki_pk_foreign_task
  ON taskdeps
  USING btree
  ("taskId");



  -- Table: writtings

-- DROP TABLE writtings;

CREATE TABLE writtings
(
  _id bigserial NOT NULL, -- identifier
  count integer, -- multiplier
  estimid bigint NOT NULL, -- Estimaton identifier
  description character varying(2047), -- item textual description
  productid character varying(512), -- Product reference ... may be a key in a catalog, may contain a catalog id
  ucost numeric(8,2), -- en euro.?
  CONSTRAINT writtings_pkey PRIMARY KEY (_id),
  CONSTRAINT writtings_estimid_fkey FOREIGN KEY (estimid)
      REFERENCES estimate (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);

COMMENT ON COLUMN writtings._id IS 'identifier';
COMMENT ON COLUMN writtings.count IS 'multiplier';
COMMENT ON COLUMN writtings.estimid IS 'Estimaton identifier';
COMMENT ON COLUMN writtings.description IS 'item textual description';
COMMENT ON COLUMN writtings.productid IS 'Product reference ... may be a key in a catalog, may contain a catalog id';
COMMENT ON COLUMN writtings.ucost IS 'en euro.?';




  -- Table: wrtags

-- DROP TABLE wrtags;

CREATE TABLE wrtags
(
  wrid bigint NOT NULL,
  tagid bigint NOT NULL,
  CONSTRAINT wrtags_pkey1 PRIMARY KEY (wrid, tagid),
  CONSTRAINT cstwrtagsref FOREIGN KEY (tagid)
      REFERENCES tag (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT wrtags_wrid_fkey1 FOREIGN KEY (wrid)
      REFERENCES writtings (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);

-- Index: fki_cstwrtagsref

-- DROP INDEX fki_cstwrtagsref;

CREATE INDEX fki_cstwrtagsref
  ON wrtags
  USING btree
  (tagid);


  -- Table: wrfiles

-- DROP TABLE wrfiles;

CREATE TABLE wrfiles
(
  _id bigserial NOT NULL , -- Identifier
  name character varying(2048), -- File Name, relative to the user home directory, must not begin with a slash.
  wrid bigint, -- Writting identifier (foreign key)
  CONSTRAINT wrtags_pkey PRIMARY KEY (_id),
  CONSTRAINT wrtags_wrid_fkey FOREIGN KEY (wrid)
      REFERENCES writtings (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);

COMMENT ON COLUMN wrfiles._id IS 'Identifier';
COMMENT ON COLUMN wrfiles.name IS 'File Name, relative to the user home directory, must not begin with a slash.';
COMMENT ON COLUMN wrfiles.wrid IS 'Writting identifier (foreign key)';


  -- Table: histowritting

-- DROP TABLE histowritting;

CREATE TABLE histowritting
(
  datechange timestamp with time zone NOT NULL DEFAULT now(),
  status integer,
  wrtid bigint NOT NULL,
  username character varying(255),
  applicationname character varying,
  _id bigserial NOT NULL,
  CONSTRAINT histowritting_pkey PRIMARY KEY (_id),
  CONSTRAINT histowritting_username_fkey FOREIGN KEY (username, applicationname)
      REFERENCES users (username, applicationname) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT histowritting_wrtid_fkey FOREIGN KEY (wrtid)
      REFERENCES writtings (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
-- Table: circle

-- DROP TABLE circle;

CREATE TABLE circle
(
  _id bigserial NOT NULL, -- Circle identifier
  owner character varying(255) NOT NULL, -- creator of this circle
  applicationname character varying(255) NOT NULL, -- Application name
  title character varying(512) NOT NULL,
  CONSTRAINT circle_pkey PRIMARY KEY (_id),
  CONSTRAINT circle_owner_fkey FOREIGN KEY (owner, applicationname)
      REFERENCES users (username, applicationname) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT circle_owner_applicationname_title_key UNIQUE (owner, applicationname, title)
)
WITH (
  OIDS=FALSE
);

COMMENT ON COLUMN circle._id IS 'Circle identifier';
COMMENT ON COLUMN circle.owner IS 'creator of this circle';
COMMENT ON COLUMN circle.applicationname IS 'Application name';


-- Table: circle_members

-- DROP TABLE circle_members;

CREATE TABLE circle_members
(
  circle_id bigserial NOT NULL,
  member character varying (255) NOT NULL,
  applicationname character varying (255) NOT NULL,
  CONSTRAINT circle_members_pkey PRIMARY KEY (circle_id, member),
  CONSTRAINT fk_circle_members_users FOREIGN KEY (member, applicationname)
      REFERENCES users (username, applicationname) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);


-- Table: passwrecovery

-- DROP TABLE passwrecovery;

CREATE TABLE passwrecovery
(
  pkid character varying NOT NULL,
  one_time_pass character varying(512) NOT NULL,
  creation timestamp with time zone NOT NULL,
  CONSTRAINT passwrecovery_pkey PRIMARY KEY (pkid),
  CONSTRAINT passwrecovery_pkid_fkey FOREIGN KEY (pkid)
      REFERENCES users (pkid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);


-- Table: blog_access

-- DROP TABLE blog_access;

CREATE TABLE blog_access
(
  post_id bigint NOT NULL,
  circle_id bigint NOT NULL,
  CONSTRAINT blog_access_pkey PRIMARY KEY (post_id, circle_id),
  CONSTRAINT blog_access_circle_id_fkey FOREIGN KEY (circle_id)
      REFERENCES circle (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT blog_access_post_id_fkey FOREIGN KEY (post_id)
      REFERENCES blog (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);

-- Table: postheader

-- DROP TABLE postheader;

CREATE TABLE postheader
(
  postid bigserial NOT NULL, -- Blog post identifier, to which will be associated this head image
  url character varying(512), -- Url for this header image,...
  CONSTRAINT postheader_pkey PRIMARY KEY (postid),
  CONSTRAINT postheader_postid_fkey FOREIGN KEY (postid)
      REFERENCES blog (_id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
COMMENT ON COLUMN postheader.postid IS 'Blog post identifier, to which will be associated this head image';
COMMENT ON COLUMN postheader.url IS 'Url for this header image, if relative, it will be on the path "~/bfiles/<postid>"';


