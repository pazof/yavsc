#!/bin/bash

FSPATH=/srv/www/yavsc

(
  set -e
  ssh root@localhost service kestrel stop
  ssh root@localhost rm -rf $FSPATH/approot/src
  cd bin/output/
  sleep 1
  rsync -ravu wwwroot approot root@localhost:$FSPATH
  sleep 1
  ssh root@localhost sync
  sleep 1
  ssh root@localhost service kestrel start
)

