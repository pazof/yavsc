#!/bin/bash
ssh root@localhost rm -rf /srv/www/yavscpre/approot/src
rsync -ravu --exclude=.git --chown=www-data:www-data wwwroot approot root@localhost:/srv/www/yavscpre


