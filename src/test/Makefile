CONFIGURATION=Debug
BINTARGET=bin/$(CONFIGURATION)/dnx451/test.dll

all: test

project.lock.json: project.json
	dnu restore

../Yavsc/bin/$(CONFIGURATION)/dnx451/Yavsc.dll:
	make -C ../Yavsc

../Yavsc.Abstract/bin/$(CONFIGURATION)/dnx451/Yavsc.Abstract.dll:
	make -C ../Yavsc.Abstract

../Yavsc.Server/bin/$(CONFIGURATION)/dnx451/Yavsc.Server.dll:
	make -C ../Yavsc.Server

$(BINTARGET): project.lock.json ../Yavsc/bin/$(CONFIGURATION)/dnx451/Yavsc.dll ../Yavsc.Abstract/bin/$(CONFIGURATION)/dnx451/Yavsc.Abstract.dll ../Yavsc.Server/bin/$(CONFIGURATION)/dnx451/Yavsc.Server.dll
	dnu build --configuration $(CONFIGURATION)

breaking:
	dnx test -trait regres=yes

testdev: $(BINTARGET)
	ASPNET_ENV=Development dnx test -maxthreads 1 -trait dev=wip 


test: $(BINTARGET)
	ASPNET_ENV=Development dnx test -maxthreads 1 -trait regres=no

clean:
	rm -rf bin obj testingrepo

.PHONY: test

