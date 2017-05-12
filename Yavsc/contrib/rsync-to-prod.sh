#!/bin/bash

FSPATH=/srv/www/yavsc

(
  set -e
  ssh root@localhost rm -rf $FSPATH/approot/src
  cd bin/output/
  rsync -ravu wwwroot approot root@localhost:$FSPATH
  ssh root@localhost sync
  ssh root@localhost service kestrel restart
)

