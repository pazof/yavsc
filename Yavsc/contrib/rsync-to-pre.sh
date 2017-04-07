#!/bin/bash

FSPATH=/srv/www/yavscpre

ssh root@localhost rm -rf $FSPATH/approot/src

(
set -e
cd bin/output/
rsync -ravu wwwroot approot root@localhost:$FSPATH
ssh root@localhost service kestrel restart
)

echo "Now, go and try <https://yavscpre.pschneider.fr>"
echo "Then, feel free to launch contrib/rsync-to-prod.sh"

