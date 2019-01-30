

test:
	make -C scripts/build/make test

web:
	make -C scripts/build/make watch

push:
	make -C src/Yavsc pushInProd

