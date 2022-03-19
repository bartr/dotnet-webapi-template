#!/bin/bash

# this runs as part of pre-build

echo "on-create start"
echo "$(date +'%Y-%m-%d %H:%M:%S')    on-create start" >> "$HOME/status"

# copy .vscode to root
cp -r .devcontainer/.vscode .

# clone repos
git clone https://github.com/microsoft/webvalidate /workspaces/webvalidate
git clone https://github.com/retaildevcrews/edge-gitops /workspaces/edge-gitops

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

# restore the repos
dotnet restore /workspaces/webvalidate/src/webvalidate.sln

# copy grafana.db to /grafana
sudo cp .devcontainer/grafana.db /grafana
sudo chown -R 472:0 /grafana

# make sure everything is up to date
sudo apt-get update
sudo apt-get upgrade -y
sudo apt-get autoremove -y
sudo apt-get clean -y

# create local registry
docker network create k3d
k3d registry create registry.localhost --port 5500
docker network connect k3d k3d-registry.localhost

# update the base docker images
docker pull mcr.microsoft.com/dotnet/aspnet:6.0-alpine
docker pull mcr.microsoft.com/dotnet/sdk:6.0
docker pull ghcr.io/cse-labs/webvalidate:latest
docker pull ghcr.io/retaildevcrews/autogitops:beta

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
