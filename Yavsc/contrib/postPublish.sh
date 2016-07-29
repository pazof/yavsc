#!/bin/bash
ssh root@localhost rm -rf /srv/www/yavscpre/approot/src

(cd bin/output/

rsync -ravu --exclude=.git --chown=www-data:www-data wwwroot approot root@localhost:/srv/www/yavscpre
ssh root@localhost service kestrel restart
)

