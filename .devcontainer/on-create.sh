#!/bin/bash

# this runs as part of pre-build

echo "on-create start"
echo "$(date +'%Y-%m-%d %H:%M:%S')    on-create start" >> "$HOME/status"

# copy .vscode to root
cp -r .devcontainer/.vscode .

export REPO_BASE=$PWD
export PATH="$PATH:$REPO_BASE/bin"

mkdir -p "$HOME/.ssh"
mkdir -p "$HOME/.oh-my-zsh/completions"

{
    # add cli to path
    echo "export PATH=\$PATH:$REPO_BASE/bin"
    echo "export REPO_BASE=$REPO_BASE"
    echo "compinit"
} >> "$HOME/.zshrc"

# make sure everything is up to date
# sudo apt-get update
# sudo apt-get upgrade -y
# sudo apt-get autoremove -y
# sudo apt-get clean -y

# create local registry
docker network create k3d
k3d registry create registry.localhost --port 5500
docker network connect k3d k3d-registry.localhost

# update the base docker images
docker pull mcr.microsoft.com/dotnet/aspnet:6.0-alpine
docker pull mcr.microsoft.com/dotnet/sdk:6.0
docker pull ghcr.io/cse-labs/webvalidate:latest

# update the app name if a valid name
export APP_NAME=$(echo ${PWD##*/})
export APP_LOWER=$(echo "$APP_NAME" | tr '[:upper:]' '[:lower:]')

if [[ "$APP_NAME" =~ ^[A-Z][A-Za-z][A-Za-z][A-Za-z][A-Za-z]+$ ]]
then
  if [[ "$APP_NAME" != "$APP_LOWER" ]]
  then
    mv src/csapp.csproj "src/$APP_LOWER.csproj"
    dotnet restore src

    sed -i "s/csapp/$APP_LOWER/g" "$REPO_BASE/bin/.kic/commands/app/build"
    sed -i "s/csapp/$APP_LOWER/g" "$REPO_BASE/bin/.kic/commands/app/deploy"
    sed -i "s/csapp/$APP_LOWER/g" Dockerfile
    find . -name '*.*' -type f -exec sed -i "s/CSApp/$APP_NAME/g" {} \;
    find . -name '*.*' -type f -exec sed -i "s/csapp/$APP_LOWER/g" {} \;

    git checkout .devcontainer/*.sh
    git add .
    git commit -am "updated application from template"
    git push
  fi
fi

echo "dowloading kic CLI"
version=$(git ls-remote --refs --sort="version:refname" --tags https://github.com/retaildevcrews/akdc | cut -d/ -f3-|tail -n1)
wget -O kic.tar.gz "https://github.com/retaildevcrews/akdc/releases/download/$version/kic-$version-linux-amd64.tar.gz"
tar -xvzf kic.tar.gz
rm kic.tar.gz
mv kic bin

echo "generating kic completion"
kic completion zsh > "$HOME/.oh-my-zsh/completions/_kic"

echo "creating k3d cluster"
kic cluster rebuild

echo "on-create complete"
echo "$(date +'%Y-%m-%d %H:%M:%S')    on-create complete" >> "$HOME/status"
