INSERT INTO profiles (username,applicationname,isanonymous,lastactivitydate,lastupdateddate) 
SELECT users.username , users.applicationname, FALSE, 
  users.lastactivitydate, now()  
  FROM users LEFT OUTER JOIN profiles ON (users.username = profiles.username 
   AND users.applicationname = profiles.applicationname)
    where profiles.username IS NULL;
    
ALTER TABLE users
  ADD FOREIGN KEY (applicationname, username) 
  REFERENCES profiles (applicationname, username) ON UPDATE CASCADE ON DELETE CASCADE;
  
ALTER TABLE profiles
DROP CONSTRAINT fk_profileusers;

update profiles SET isanonymous = FALSE where isanonymous IS NULL;

ALTER TABLE profiles
   ALTER COLUMN isanonymous SET DEFAULT TRUE;
ALTER TABLE profiles
   ALTER COLUMN isanonymous SET NOT NULL;

