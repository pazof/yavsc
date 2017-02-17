#!/bin/sh

BUILDCMD=MSBuild.exe

PATH+=':/cygdrive/c/cygwin64/bin'

for p in 'C:\Program Files (x86)\MSBuild\14.0\Bin' 'C:\Users\paul\bin' 'C:\Users\paul\.dnx\runtimes\dnx-clr-win-x86.1.0.0-rc1-update2\bin' 'C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools' 'C:\Program Files (x86)\Microsoft Visual Studio14.0\Common7\IDE\' 'C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\BIN' 'C:\Windows\Microsoft.NET\Framework\v4.0.30319' 'C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\VCPackages'  'C:\Program Files (x86)\HTML Help Workshop' 'C:\Program Files (x86)\Microsoft Visual Studio 14.0\Team Tools\Performance Tools' 'C:\Program Files (x86)\Windows Kits\10\bin\x86' 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\' 'C:\ProgramData\Oracle\Java\javapath' 'C:\Program Files\dotnet' 'C:\Program Files (x86)\Microsoft VS Code\bin'
do
	PATH+=:`cygpath $p`
done


if [[ "$BUILDCONFIG" == "" ]]
then
	BUILDCONFIG="Lua Yavsc ZicMoove Debug"
fi
echo "Building $BUILDCONFIG ..."

for conf in $BUILDCONFIG
do
	$BUILDCMD /p:Configuration=$conf /t:SignAndroidPackage ZicMoove/ZicMoove.Droid/ZicMoove.Droid.csproj /logger:"Kobush.Build.Logging.XmlLogger,Kobush.Build.dll;build-$conf-result.xml"
	xsltproc -o "build-$conf-result.html" "msbuild.xsl" "build-$conf-result.xml"
done


# ;C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow
#  \;;;;;;;;;;C:\Program Files (x86)\PHP\;C:\csvn\bin\;C:\csvn\Python25\;C:\Windows\system32;C:\Windows;C:\Windows\System32\Wbem;C:\Windows\System32\WindowsPowerShell\v1.0\;C:\Program Files\TortoiseSVN\bin;C:\Program Files\MiKTeX 2.9\miktex\bin\x64\;C:\Program Files (x86)\GtkSharp\2.12\bin;C:\Program Files\Microsoft SQL Server\110\Tools\Binn\;C:\Program Files\Microsoft SQL Server\120\Tools\Binn\;C:\Program Files\Microsoft SQL Server\130\Tools\Binn\;C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\;C:\Program Files\Git\cmd;\;
