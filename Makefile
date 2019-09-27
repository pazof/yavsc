

test:
	make -C src/test

web:
	make -C src/Yavsc web

pushInProd:
	make -C src/Yavsc pushInProd

pushInPre:
	make -C src/Yavsc pushInPre

packages:
	make -C src/Yavsc.Abstract pack

findResources:
	find src -name "*.resx" |sort

prepare_all_code: 
	make -C  src/Yavsc.Abstract prepare_code 
	make -C  src/Yavsc.Server prepare_code 
	make -C  src/Yavsc prepare_code


.PHONY: packages
