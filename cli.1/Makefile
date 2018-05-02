

all: build

build: project.lock.json
	dnu build

deploy-pkg:


restore:
	touch project.json
	dnu restore
	
project.lock.json: project.json
	dnu restore

run: project.lock.json
	ASPNET_ENV=Development ASPNET_LOG_LEVEL=Debug dnx run



