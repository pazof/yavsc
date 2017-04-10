#!/bin/bash

FSPATH=/srv/www/yavsc



(
  set -e
  ssh root@localhost rm -rf $FSPATH/approot/src
  cd bin/output/
  rsync -ravu wwwroot approot root@localhost:$FSPATH

  sleep 5
  ssh root@localhost service kestrel restart
)


# wait a little, for the processes to become stable
sleep 15
