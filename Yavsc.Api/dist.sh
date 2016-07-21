#!/bin/bash

dnu pack
cp -a bin/Debug/*.nupkg ~/Nupkgs
(cd ../Yavsc && dnu install Yavsc.Api)
(cd ../testOauthClient && dnu install Yavsc.Api)


