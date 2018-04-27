

all: build

build: project.lock.json
	dnu build


restore:
	touch project.json
	dnu restore
	
project.lock.json: project.json
	dnu restore

run: ../Yavsc/bin/Debug/dnx451/Yavsc.dll
	ASPNET_ENV=Development ASPNET_LOG_LEVEL=Debug dnx run

../Yavsc/bin/Debug/dnx451/Yavsc.dll:
	make -C ../Yavsc


