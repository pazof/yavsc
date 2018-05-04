
include ../msbuild.mk

all: build

run: $(BINTARGETPATH)
	$(MONO) $(BINTARGETPATH)

$(BINTARGETPATH): build

build:
	$(MSBUILD) $(PRJNAME).csproj /t:Build

restore:
	$(MSBUILD) $(PRJNAME).csproj /t:Restore

clean:
	$(MSBUILD) $(PRJNAME).csproj /t:Clean

.PHONY: run build restore

