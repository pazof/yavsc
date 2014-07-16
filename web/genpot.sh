Le script a débuté sur ven. 03 janv. 2014 00:34:11 CET
paul@pazms:~/workspace/mae/mae$ ../GettextNet/Bin/Release/GNU.Gettext.Xgettext.exe  -D ./ --recursive -o ./po/Message s.pot
Template file '/home/paul/workspace/mae/mae/po/Messages.pot' generated
paul@pazms:~/workspace/mae/mae$ ../GettextNet/Bin/Release/GNU.Gettext.Msgfmt.exe -l fr-FR -d ../bin/Debug -r Examples .Hello.Messages -L ../../Bin/Debug fr.po
File fr.po not found
Error accepting options
paul@pazms:~/workspace/mae/mae$ exit

Script terminé sur ven. 03 janv. 2014 00:35:36 CET
