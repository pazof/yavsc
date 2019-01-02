#!/bin/bash

export FSPATH=/srv/www/yavsc

(
  set -e
  ssh root@localhost systemctl stop kestrel
  ssh root@localhost rm -rf $FSPATH/approot
  cd bin/output/
  sleep 1
  echo "Sync: > $FSPATH"
  rsync -ravu wwwroot approot root@localhost:$FSPATH
  sleep 1
  ssh root@localhost sync
  sleep 1
  ssh root@localhost systemctl start kestrel
)

