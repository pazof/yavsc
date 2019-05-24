

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

