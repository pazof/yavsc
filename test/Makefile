CONFIGURATION=Debug
BINTARGET=bin/$(CONFIGURATION)/dnx451/test.dll

all: test

project.lock.json: project.json
	dnu restore

$(BINTARGET): project.lock.json
	dnu build


test: $(BINTARGET)
	dnx test

.PHONY: test

