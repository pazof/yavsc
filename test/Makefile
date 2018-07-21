CONFIGURATION=Debug
BINTARGET=bin/$(CONFIGURATION)/dnx451/test.dll

all: test

project.lock.json: project.json
	dnu restore

$(BINTARGET): project.lock.json
	dnu build --configuration $(CONFIGURATION)


breaking:
	dnx test -class test.YavscWorkInProgress.GitClone


test: $(BINTARGET)
	ASPNET_ENV=Development dnx test -maxthreads 1

.PHONY: test

