
-- Table: blog

-- DROP TABLE blog;

CREATE TABLE blog
(
  applicationname character varying(255) NOT NULL,
  username character varying(255) NOT NULL,
  posted timestamp with time zone NOT NULL,
  modified timestamp with time zone NOT NULL,
  title character varying(255) NOT NULL,
  bcontent text NOT NULL,
  CONSTRAINT pk_blog PRIMARY KEY (username , applicationname , title ),
  CONSTRAINT bloguser FOREIGN KEY (applicationname, username)
      REFERENCES users (applicationname, username) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
