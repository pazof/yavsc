SELECT version();
CREATE EXTENSION "cube"; -- you will not be able install "earthdistance" w/o "cube" extension
CREATE EXTENSION "earthdistance"; --or any other extension you need
create extension earthdistance;
select earth();
select earth_distance(ll_to_earth(12.4,2.0),ll_to_earth(12.456,2.8043)); -- 87km