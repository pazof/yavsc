

test:
	make -C scripts/build/make test

web:
	make -C scripts/build/make watch

pushInProd:
	make -C src/Yavsc pushInProd

pushInPre:
	make -C src/Yavsc pushInPre
