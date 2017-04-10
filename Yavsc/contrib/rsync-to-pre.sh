#!/bin/bash

FSPATH=/srv/www/yavscpre

ssh root@localhost rm -rf $FSPATH/approot/src

(
set -e
cd bin/output/
rsync -ravu wwwroot approot root@localhost:$FSPATH

sleep 5
ssh root@localhost service kestrel restart
)

sleep 15
echo "Now, go and try <https://yavscpre.pschneider.fr>"
# wait a little, for the processes to become stable
sleep 15
echo "Then, feel free to launch contrib/rsync-to-prod.sh"



