#!/bin/sh
PRJS="YavscLib/YavscLib ZicMoove/ZicMoove/ZicMoove ZicMoove/ZicMoove.Droid/ZicMoove.Droid"
MSB=msbuild.exe

if [ "clean" = "$1" ]; then
		echo \< $1
 rm -rf YavscLib/YavscLib/obj ZicMoove/*/obj
fi

for p in $PRJS
do

nuget restore $p.csproj -PackagesDirectory packages

done

$MSB /t:UpdateAndroidResources /p:Configuration=Debug ZicMoove/ZicMoove.Droid/ZicMoove.Droid.csproj

for p in $PRJS
do
$MSB /t:Build /p:Configuration=Debug $p.csproj
done
