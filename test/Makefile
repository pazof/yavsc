CONFIGURATION=Debug
BINTARGET=bin/$(CONFIGURATION)/dnx451/test.dll

project.lock.json: project.json
	dnu restore

$(BINTARGET): project.lock.json
	dnu build

all: $(BINTARGET)

test: $(BINTARGET)
	dnx test

.PHONY: test

