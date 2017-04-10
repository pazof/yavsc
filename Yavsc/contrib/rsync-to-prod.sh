#!/bin/bash

FSPATH=/srv/www/yavsc

ssh root@localhost rm -rf $FSPATH/approot/src

(
set -e
cd bin/output/
rsync -ravu wwwroot approot root@localhost:$FSPATH

sleep 1
ssh root@localhost service kestrel restart
)


# wait a little, for the processes to become stable
sleep 10
