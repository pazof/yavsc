#!/bin/bash

export FSPATH=/srv/www/yavscpre

ssh root@localhost rm -rf $FSPATH/approot

(
set -e
ssh root@localhost systemctl stop kestrel-pre
cd bin/output/
echo "sync to $FSPATH"
rsync -ravu wwwroot approot root@localhost:$FSPATH
sleep 1
ssh root@localhost sync
sleep 1
ssh root@localhost systemctl start kestrel-pre
)

echo "Now, go and try <https://yavscpre.pschneider.fr>"
echo "Then, feel free to launch contrib/rsync-to-prod.sh"



