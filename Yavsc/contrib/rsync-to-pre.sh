#!/bin/bash

FSPATH=/srv/www/yavscpre

ssh root@localhost rm -rf $FSPATH/approot

(
set -e
ssh root@localhost service kestrel stop
cd bin/output/
rsync -ravu wwwroot approot root@localhost:$FSPATH
sleep 1
ssh root@localhost sync
sleep 1
ssh root@localhost service kestrel start
)

echo "Now, go and try <https://yavscpre.pschneider.fr>"
echo "Then, feel free to launch contrib/rsync-to-prod.sh"



