
set -e

# config
export DNX_USER_HOME="`pwd -P`/dnx"

# rt
mkdir -p dnx/runtimes
cd dnx/runtimes
curl --insecure -sSL https://freespeech.pschneider.fr/files/Paul/dnx-mono.1.0.0-rc1-update2.tar.bz2 |tar xj
cd ..

# dnvm
mkdir -p dnvm
cd dnvm
curl --insecure -sSL https://freespeech.pschneider.fr/files/Paul/dnvm.sh >dnvm.sh
cd ..

# alias
mkdir -p alias
echo "dnx-mono.1.0.0-rc1-update2" >alias/default.alias
. dnvm/dnvm.sh

# end
cd ..

echo "DNX a été ressucité dans $DNX_USER_HOME"
echo "Pour utiliser dnx et dnu:" 
echo " . ${DNX_USER_HOME}/dnvm/dnvm.sh"

