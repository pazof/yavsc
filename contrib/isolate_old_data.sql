ALTER TABLE estimate
  DROP CONSTRAINT estimate_username_fkey;
ALTER TABLE estimate
  DROP CONSTRAINT estimate_client_fkey;
ALTER TABLE estimate RENAME TO old_estimate;

ALTER TABLE blog  DROP CONSTRAINT bloguser;
ALTER TABLE blog RENAME TO old_blog;


ALTER TABLE activity RENAME TO old_activity;
ALTER TABLE tag RENAME TO old_tag;
ALTER TABLE tagged RENAME TO old_tagged;
ALTER TABLE writtings RENAME TO old_writtings;

drop table blfiles;
drop table blog_access;
drop table circle;
drop table circle_members;
drop table commandes;
drop table comment;
drop table histoestim;
drop table histowritting;
drop table hr;
drop table comment;
drop table product;
drop table profiledata;
drop table satisfaction;
drop table stocksymbols;
drop table taskdeps;
drop table tasks;
drop table userskills;
drop table wrfiles ;
drop table wrtags ;
drop table passwrecovery;
drop table projet;
drop table skill;
drop table usersroles;
drop table roles;
drop table users;
drop table profiles;
drop TABLE postheader;
