#!/bin/bash

version="$1"
major=0
minor=0
build=0

# break down the version number into it's components
regex="([0-9]+).([0-9]+).([0-9]+)((-[A-Za-z]+)([0-9]+))?"
if [[ $version =~ $regex ]]; then
  major="${BASH_REMATCH[1]}"
  minor="${BASH_REMATCH[2]}"
  build="${BASH_REMATCH[3]}"
  patchtype="${BASH_REMATCH[5]}"
  patchnum="${BASH_REMATCH[6]}"
fi

# check paramater to see which number to increment
if [[ "$2" == "feature" ]]; then
  minor=$(echo $minor + 1 | bc)
  build=0
  patchtype=
  patchnum=
elif [[ "$2" == "build" ]]; then
  build=$(echo $build + 1 | bc)
  patchtype=
  patchnum=
elif [[ "$2" == "major" ]]; then
  major=$(echo $major+1 | bc)
  minor=0
  build=0
  patchtype=
  patchnum=
elif [[ "$2" == "patch" ]]; then
  patchnum=$(echo $patchnum + 1 | bc)
else
  echo "usage: ./version.sh version_number [major/feature/build/patch]" >&2

  exit -1
fi

# echo the new version number
echo "${major}.${minor}.${build}${patchtype}${patchnum}"

