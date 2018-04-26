

all: build

build: project.lock.json
	dnu build

project.lock.json: project.json

bin/output/approot/run: project.lock.json
	dnu restore

run: bin/output/approot/run
	ASPNET_ENV=Development dnx run
